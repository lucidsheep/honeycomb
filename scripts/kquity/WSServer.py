import asyncio
import websockets
import sys
import lightgbm as lgb

import datetime
from typing import List, Optional

import dateutil.parser
import numpy as np

import map_structure
from preprocess import *

class GameState:
	def __init__(self, map_info: map_structure.MapStructureInfo, start_time: datetime.datetime):
		self.map_info = map_info
		self.teams = [TeamState() for _ in range(2)]
		self.berries_available = map_info.total_berries
		self.maiden_states = [ContestableState.NEUTRAL for _ in range(5)]
		self.snail_state = InferredSnailState(self)
		self.start_time = start_time

	def get_team(self, team: Team) -> TeamState:
		return self.teams[team.value]

	def get_worker_by_position_id(self, position_id: int) -> WorkerState:
		team: Team = position_id_to_team(position_id)
		worker_index = position_id_to_worker_index(position_id)
		return self.get_team(team).workers[worker_index]

	def num_bots(self) -> int:
		return sum([worker.is_bot for team in self.teams for worker in team.workers])

map_structure_infos = map_structure.MapStructureInfos()

CurrentGameState = GameState(map_structure_infos.get_map_info(Map["map_day"], True), datetime.datetime.today())
game_in_progress = False
connected = False

model = lgb.Booster(None, None, "model.bst")

def split_payload(payload: str) -> List[str]:
    assert payload.startswith('{')
    assert payload.endswith('}')
    return payload[1:-1].split(';')

def parse_line(raw_event: str) -> Optional[GameEvent]:
	skippable_events = {'gameend', 'playernames',
                        'reserveMaiden', 'unreserveMaiden',
                        'cabinetOnline', 'cabinetOffline',
                        'bracket', 'tstart', 'tournamentValidation', 'checkIfTournamentRunning'
                        }
	dispatcher = {'berryDeposit': BerryDepositEvent,
                  'berryKickIn': BerryKickInEvent,
                  'carryFood': CarryFoodEvent,
                  'snailEat': SnailEatEvent,
                  'snailEscape': SnailEscapeEvent,
                  'getOnSnail': GetOnSnailEvent,
                  'getOffSnail': GetOffSnailEvent,
                  'glance': GlanceEvent,
                  'playerKill': PlayerKillEvent,
                  'blessMaiden': BlessMaidenEvent,
                  'useMaiden': UseMaidenEvent,
                  'spawn': SpawnEvent,
                  'gamestart': GameStartEvent,
                  'mapstart': MapStartEvent,
                  'victory': VictoryEvent,
                  }
	event_row = raw_event.split(",")
	event_type = event_row[2]
	if event_type in skippable_events:
		return None
	assert event_type in dispatcher, f'Unknown event type: {event_type}'

	payload_values = split_payload(event_row[3])
	event = dispatcher[event_type](payload_values)
	event.timestamp = dateutil.parser.isoparse(event_row[1])
	event.game_id = int(event_row[4])
	return event

def wsevent_to_event(wsevent: str) -> Optional[GameEvent]:
	try:
		maybe_event = parse_line(wsevent)
	except Exception as e:
		print(f'Failed to parse event: {wsevent}')
		return None
	return maybe_event

def process_ws_event(wsevent: str) -> Optional[GameEvent]:
	global CurrentGameState, game_in_progress
	event = wsevent_to_event(wsevent)
	if(event is None):
		return None
	if(isinstance(event, GameStartEvent)):
		# new game
		# = map_structure.MapStructureInfos()
		map_info = map_structure_infos.get_map_info(
            event.map, event.gold_on_left)
		CurrentGameState = GameState(map_info, event.timestamp)
		print("new game started")
		game_in_progress = True
		event.timestamp = 0.0
	elif(game_in_progress):
		event.timestamp = (event.timestamp - CurrentGameState.start_time).total_seconds()
		event.modify_game_state(CurrentGameState)
		print("event processed")
	else:
		return None
	return event

def vectorize_game_state(game_state: GameState, next_event: GameEvent) -> np.ndarray:
    blue_team_vec = vectorize_team(game_state.get_team(Team.BLUE))
    gold_team_vec = vectorize_team(game_state.get_team(Team.GOLD))

    parts = [
        blue_team_vec,
        gold_team_vec,
        vectorize_maidens(game_state.maiden_states),
        vectorize_map_one_hot(game_state.map_info.map_id),
        vectorize_snail_state(game_state, next_event),
        [game_state.berries_available / 70.0],
    ]

    return np.concatenate(parts)

async def onWS(websocket):
	global game_in_progress, connected
	print("KQIS connected")
	connected = True
	try:
		async for message in websocket:
			decoded_message = message.decode("utf-8")
			if(decoded_message == "exit"):
				await websocket.close()
				sys.exit()
			ev = process_ws_event(decoded_message)
			if(ev is not None) and game_in_progress:
				encoded = vectorize_game_state(CurrentGameState, ev)
				prediction = model.predict([encoded])
				prediction = prediction[0]
				print("new win %: {}".format(prediction))
				await websocket.send("{}".format(prediction))
	except websockets.exceptions.ConnectionClosedError:
		#await websocket.close()
		game_in_progress = False
		connected = False
		print("disconnected")
	except Exception:
		game_in_progress = False
		connected = False
		print("disconnected")

async def connLoop():
	global connected
	noConnCount = 0
	async with websockets.serve(onWS, "localhost", 8765):
		while noConnCount < 10:
			await asyncio.sleep(1)
			if(connected):
				noConnCount = 0
			else:
				noConnCount += 1
#async def main():
	#async with websockets.serve(onWS, "localhost", 8765):
		#await asyncio.Future()  # run forever

if __name__ == '__main__':
	loop = asyncio.get_event_loop()
	task = loop.create_task(connLoop())
	try:
		loop.run_until_complete(task)
	except asyncio.CancelledError:
		pass
	#asyncio.run(main())
