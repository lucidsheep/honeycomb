import csv

import collections
import datetime
import glob
import random
import pickle
import os
from typing import List, Optional, Union, Type, Iterator, Tuple

import dateutil.parser
import numpy as np


import constants
import map_structure
from constants import Team, ContestableState, VictoryCondition, PlayerCategory, MaidenType, Map

# https://kqhivemind.com/wiki/Stats_Socket_Events


def opposing_team(team: Team) -> Team:
    return Team.GOLD if team == Team.BLUE else Team.BLUE


class WorkerState:
    def __init__(self):
        self.has_speed = False
        self.has_wings = False
        self.has_food = False
        self.is_bot = False


class TeamState:
    def __init__(self):
        self.eggs = 2
        self.food_deposited = [False for _ in range(12)]
        self.workers = [WorkerState() for _ in range(4)]


StartSnailEvent: Type = Union['GetOnSnailEvent', 'SnailEatEvent']
StopSnailEvent: Type = Union['GetOffSnailEvent', 'SnailEscapeEvent']


class InferredSnailState:
    VANILLA_SNAIL_PIXELS_PER_SECOND = 20.896215463
    SPEED_SNAIL_PIXELS_PER_SECOND = 28.209890875

    def __init__(self, game_state: 'GameState'):
        self.snail_x = constants.SCREEN_WIDTH / 2
        self.snail_velocity = 0
        self.last_touch_timestamp = 0.0
        self.game_state = game_state

    @staticmethod
    def snail_movement_multiplier(gold_on_left, team):
        if gold_on_left and team == Team.GOLD: return -1
        if gold_on_left and team == Team.BLUE: return 1
        if not gold_on_left and team == Team.GOLD: return 1
        if not gold_on_left and team == Team.BLUE: return -1

    def inferred_snail_position(self, current_ts: float) -> float:
        return self.snail_x + (current_ts - self.last_touch_timestamp) * self.snail_velocity

    def compute_snail_velocity(self, rider_position_id) -> float:
        rider = self.game_state.get_worker_by_position_id(rider_position_id)
        velocity = self.SPEED_SNAIL_PIXELS_PER_SECOND if rider.has_speed else self.VANILLA_SNAIL_PIXELS_PER_SECOND
        velocity *= InferredSnailState.snail_movement_multiplier(self.game_state.map_info.gold_on_left,
                                                                 position_id_to_team(rider_position_id))

        # print('snail_x:', self.snail_x,
        #     'velocity:', velocity,
        #     'gold_on_left:', self.game_state.map_info.gold_on_left,
        #     'team:', position_id_to_team(rider_position_id))
        return velocity

    def _update_position_and_timestamp(self, snail_event):
        # if should_print:
        #     inferred_position = self.inferred_snail_position(snail_event.timestamp)
        #     diff = inferred_position - snail_event.snail_x
        #     print('inferred_position:', inferred_position, 'event_snail_x:', snail_event.snail_x, 'diff:', diff)
        self.last_touch_timestamp = snail_event.timestamp
        self.snail_x = snail_event.snail_x

    def start_snail(self, start_snail_event: StartSnailEvent):
        self.snail_velocity = self.compute_snail_velocity(start_snail_event.rider_position_id)
        self._update_position_and_timestamp(start_snail_event)

    def stop_snail(self, stop_snail_event: StopSnailEvent):
        self._update_position_and_timestamp(stop_snail_event)
        self.snail_velocity = 0


class GameState:
    def __init__(self, map_info: map_structure.MapStructureInfo):
        self.map_info = map_info
        self.teams = [TeamState() for _ in range(2)]
        self.berries_available = map_info.total_berries
        self.maiden_states = [ContestableState.NEUTRAL for _ in range(5)]
        self.snail_state = InferredSnailState(self)

    def get_team(self, team: Team) -> TeamState:
        return self.teams[team.value]

    def get_worker_by_position_id(self, position_id: int) -> WorkerState:
        team: Team = position_id_to_team(position_id)
        worker_index = position_id_to_worker_index(position_id)
        return self.get_team(team).workers[worker_index]

    def num_bots(self) -> int:
        return sum([worker.is_bot for team in self.teams for worker in team.workers])


class GameEvent:
    def __init__(self):
        self.timestamp = None
        self.game_id = None

    def modify_game_state(self, game_state: GameState):
        pass


GameEventsList = List[GameEvent]
GameEventsIterator = Iterator[GameEvent]


def position_id_to_team(position_id: int) -> Team:
    return Team(position_id % 2)


def position_id_to_worker_index(position_id: int) -> int:
    return (position_id - 3) // 2


def is_queen_position_id(position_id: int) -> bool:
    return position_id <= 2


def split_payload(payload: str) -> List[str]:
    assert payload.startswith('{')
    assert payload.endswith('}')
    return payload[1:-1].split(',')


class GameStartEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.map = Map[payload_values[0]]
        self.gold_on_left = payload_values[1] == 'True'
        self.game_version = payload_values[4]


class GameEndEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.game_duration = float(payload_values[2])


class MapStartEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.map = Map[payload_values[0]]
        self.gold_on_left = payload_values[1] == 'True'
        self.game_version = payload_values[4]


class SpawnEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.position_id = int(payload_values[0])
        self.is_bot = payload_values[1] == 'True'

    def modify_game_state(self, game_state: GameState):
        game_state.get_worker_by_position_id(self.position_id).is_bot = self.is_bot


class BerryDepositEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.hole_x = int(payload_values[0])
        self.hole_y = int(payload_values[1])
        self.position_id = int(payload_values[2])

    def modify_game_state(self, game_state: GameState):
        team: Team = position_id_to_team(self.position_id)
        worker_index = position_id_to_worker_index(self.position_id)
        team_state: TeamState = game_state.get_team(team)
        team_state.workers[worker_index].has_food = False
        try:
            berry_index = game_state.map_info.get_berry_index(self.hole_x, self.hole_y)
        except ValueError:
            raise GameValidationError('Invalid berry deposit event: {} {}'.format(self.hole_x, self.hole_y))

        team_state.food_deposited[berry_index] = True
        game_state.berries_available -= 1


class BerryKickInEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.hole_x = int(payload_values[0])
        self.hole_y = int(payload_values[1])
        self.position_id = int(payload_values[2])
        self.counts_for_own_team = payload_values[3] == 'True'

    def modify_game_state(self, game_state: GameState):
        team: Team = position_id_to_team(self.position_id)
        if not self.counts_for_own_team:
            team = opposing_team(team)

        try:
            berry_index = game_state.map_info.get_berry_index(self.hole_x, self.hole_y)
        except ValueError:
            raise GameValidationError('Invalid berry deposit event: {} {}'.format(self.hole_x, self.hole_y))

        game_state.get_team(team).food_deposited[berry_index] = True
        game_state.berries_available -= 1


class BlessMaidenEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.maiden_x = int(payload_values[0])
        self.maiden_y = int(payload_values[1])
        self.gate_color = ContestableState.BLUE if payload_values[2] == 'Blue' else ContestableState.GOLD

    def modify_game_state(self, game_state: GameState):
        try:
            _, maiden_index = game_state.map_info.get_type_and_maiden_index(self.maiden_x, self.maiden_y)
            game_state.maiden_states[maiden_index] = self.gate_color
        except ValueError:
            raise GameValidationError('Invalid maiden event: {} {}'.format(self.maiden_x, self.maiden_y))


class CarryFoodEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.position_id = int(payload_values[0])

    def modify_game_state(self, game_state: GameState):
        game_state.get_worker_by_position_id(self.position_id).has_food = True


class GetOnSnailEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.snail_x = int(payload_values[0])
        self.rider_position_id = int(payload_values[2])

    def modify_game_state(self, game_state: GameState):
        game_state.snail_state.start_snail(self)


class SnailEatEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.snail_x = int(payload_values[0])
        self.rider_position_id = int(payload_values[2])
        self.eaten_position_id = int(payload_values[3])

    def modify_game_state(self, game_state: GameState):
        game_state.snail_state.start_snail(self)


class GetOffSnailEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.snail_x = int(payload_values[0])
        self.position_id = int(payload_values[3])

    def modify_game_state(self, game_state: GameState):
        game_state.snail_state.stop_snail(self)


class SnailEscapeEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.snail_x = int(payload_values[0])
        self.escaped_position_id = int(payload_values[2])

    def modify_game_state(self, game_state: GameState):
        game_state.snail_state.stop_snail(self)


class GlanceEvent(GameEvent):
    __slots__ = ['glance_x', 'glance_y', 'position_ids']

    def __init__(self, payload_values: List[str]):
        super().__init__()
        if len(payload_values) == 2:
            # Hack for older versions where the payload was missing the glance coordinates.
            payload_values = ['-1', '-1'] + payload_values
        self.glance_x = int(payload_values[0])
        self.glance_y = int(payload_values[1])
        self.position_ids = [int(payload_values[2]), int(payload_values[3])]


class PlayerKillEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.killer_x = int(payload_values[0])
        self.killer_y = int(payload_values[1])
        self.killer_position_id = int(payload_values[2])
        self.killed_position_id = int(payload_values[3])
        self.killed_player_category = PlayerCategory[payload_values[4]]

    def modify_game_state(self, game_state: GameState):
        team: Team = position_id_to_team(self.killed_position_id)
        if self.killed_player_category == PlayerCategory.Queen:
            game_state.get_team(team).eggs -= 1
        else:
            killed_worker = game_state.get_worker_by_position_id(self.killed_position_id)
            validate_condition(killed_worker.has_wings == (self.killed_player_category == PlayerCategory.Soldier),
                               'Worker has wings but is not a soldier')
            killed_worker.has_speed = killed_worker.has_food = killed_worker.has_wings = False


class GameValidationError(ValueError):
    def __init__(self, message: str):
        super().__init__(message)


def validate_condition(condition, message):
    if not condition:
        raise GameValidationError(message)


class UseMaidenEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.maiden_x = int(payload_values[0])
        self.maiden_y = int(payload_values[1])
        self.maiden_type = MaidenType[payload_values[2]]
        self.position_id = int(payload_values[3])

    def modify_game_state(self, game_state: GameState):
        worker_state = game_state.get_worker_by_position_id(self.position_id)

        validate_condition(worker_state.has_food, "Worker using maiden missing food")
        if self.maiden_type == MaidenType.maiden_speed:
            validate_condition(not worker_state.has_speed, "Worker using speed maiden already has speed")
            worker_state.has_speed = True
        elif self.maiden_type == MaidenType.maiden_wings:
            validate_condition(not worker_state.has_wings, "Worker using wings maiden already has wings")
            worker_state.has_wings = True

        worker_state.has_food = False


class VictoryEvent(GameEvent):
    def __init__(self, payload_values: List[str]):
        super().__init__()
        self.winning_team = Team.BLUE if payload_values[0] == 'Blue' else Team.GOLD
        self.victory_condition = VictoryCondition[payload_values[1]]

    def modify_game_state(self, game_state: GameState):
        if self.victory_condition == VictoryCondition.economic:
            validate_condition(sum(game_state.get_team(self.winning_team).food_deposited) == 12,
                               "Econ victory team does not have 12 berries deposited")
        elif self.victory_condition == VictoryCondition.military:
            validate_condition(game_state.get_team(opposing_team(self.winning_team)).eggs == -1,
                               "Military loss team has a living queen?")
        elif self.victory_condition == VictoryCondition.snail:
            map_info = game_state.map_info
            snail_direction = InferredSnailState.snail_movement_multiplier(map_info.gold_on_left,
                                                                           self.winning_team)
            snail_post = constants.SCREEN_WIDTH / 2 + snail_direction * map_info.snail_track_width
            distance_from_goal = snail_post - game_state.snail_state.inferred_snail_position(self.timestamp)

            # This condition failed one in 65 games in a sample of otherwise validated snail victories.
            validate_condition(abs(distance_from_goal) < 100, 'inferred snail position far from goal')


def parse_event(raw_event_row) -> Optional[GameEvent]:
    skippable_events = {'gameend', 'playernames',
                        'reserveMaiden', 'unreserveMaiden',
                        'cabinetOnline', 'cabinetOffline',
                        'bracket', 'tstart', 'tournamentValidation', 'checkIfTournamentRunning',
                        'glance'
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
    event_type = raw_event_row['event_type']
    if event_type in skippable_events:
        return None
    assert event_type in dispatcher, f'Unknown event type: {event_type}'

    payload_values = split_payload(raw_event_row['values'])
    event = dispatcher[event_type](payload_values)
    event.timestamp = dateutil.parser.isoparse(raw_event_row['timestamp'])
    event.game_id = int(raw_event_row['game_id'])
    return event


def normalize_times(events: GameEventsList) -> GameEventsList:
    events.sort(key=lambda e: e.timestamp)

    start_ts: datetime.datetime = get_game_start(events).timestamp
    for game_event in events:
        game_event.timestamp = (game_event.timestamp - start_ts).total_seconds()
    return events


def iterate_events_by_game_and_normalize_time(events: GameEventsIterator) -> Iterator[Tuple[int, GameEventsList]]:
    last_game_id = None
    single_game_events = []
    for event in events:
        cur_game_id = event.game_id
        if last_game_id != cur_game_id and single_game_events:
            yield last_game_id, normalize_times(single_game_events)
            single_game_events = []
        single_game_events.append(event)
        last_game_id = cur_game_id
    if single_game_events:
        yield last_game_id, normalize_times(single_game_events)


def debug_print_events(events: GameEventsList):
    for event in events:
        def ignore_attr(attr_name):
            return attr_name.startswith('_') or attr_name == 'modify_game_state'
        attrs = {a: getattr(event, a) for a in dir(event) if not ignore_attr(a)}
        print(event.__class__.__name__.removesuffix('Event'), attrs)


def find_first_event_of_type(events: GameEventsList, event_type: Type[GameEvent]) -> Optional[GameEvent]:
    for event in events:
        if isinstance(event, event_type):
            return event
    return None


def get_game_start(events: GameEventsList) -> GameStartEvent:
    game_start = find_first_event_of_type(events, GameStartEvent)
    if not game_start:
        raise GameValidationError(f'No game start found in events for game {events[0].game_id}')
    return game_start


def get_map_start(events: GameEventsList) -> MapStartEvent:
    map_start = find_first_event_of_type(events, MapStartEvent)
    if not map_start:
        raise GameValidationError(f'No map start found in events for game {events[0].game_id}')
    return map_start


def is_valid_game(events: GameEventsList,
                  map_structure_infos: map_structure.MapStructureInfos) -> Optional[GameValidationError]:
    try:
        game_start: GameStartEvent = get_map_start(events)
        map_info: map_structure.MapStructureInfo = map_structure_infos.get_map_info(
            game_start.map, game_start.gold_on_left)

        game_state = GameState(map_info)
        for event in events:
            event.modify_game_state(game_state)

        if not isinstance(events[-1], VictoryEvent):
            raise GameValidationError('Game did not end in victory')
    except GameValidationError as e:
        return e
    return None


def iterate_events_from_csv(csv_path: str, skip_raw_events_fn=None) -> GameEventsIterator:
    for filename in glob.glob(csv_path):
        yield from iterate_events_from_csv_reader(csv.DictReader(open(filename)), skip_raw_events_fn)


def iterate_events_from_csv_reader(csv_event_reader, skip_raw_events_fn=None) -> GameEventsIterator:
    for row in csv_event_reader:
        try:
            if skip_raw_events_fn and skip_raw_events_fn(row):
                continue
            maybe_event = parse_event(row)
        except Exception as e:
            print(f'Failed to parse event: {row}, {e}')
            continue
        if maybe_event is not None:
            yield maybe_event


def has_bots(events: GameEventsList) -> bool:
    for event in events:
        if hasattr(event, 'is_bot') and event.is_bot:
            return True
    return False


def validate_game_data(csv_path):
    events = iterate_events_from_csv(csv_path, lambda d: d['timestamp'] <= '2022-09')

    map_structure_infos = map_structure.MapStructureInfos()
    validation_errors = collections.Counter()
    validated_game_ids = set()
    for game_id, game_events in iterate_events_by_game_and_normalize_time(events):
        maybe_error = is_valid_game(game_events, map_structure_infos)
        if maybe_error:
            validation_errors[maybe_error.args[0]] += 1
        else:
            validated_game_ids.add(game_id)
            validation_errors['valid'] += 1

    print('validation_errors', validation_errors.most_common())

    with open(csv_path, 'r') as f:
        event_reader = csv.DictReader(f)
        fieldnames = event_reader.fieldnames
        with open('validated_' + csv_path, 'w') as output_file:
            event_writer = csv.DictWriter(output_file, fieldnames=fieldnames)
            event_writer.writeheader()
            for row in event_reader:
                if int(row['game_id']) in validated_game_ids:
                    event_writer.writerow(row)


def iterate_game_events_with_state(events: GameEventsIterator, map_structure_infos: map_structure.MapStructureInfos) -> \
        Iterator[Tuple[int, GameEvent, GameState, GameEventsList]]:
    grouped_events = iterate_events_by_game_and_normalize_time(events)
    for game_id, game_events in grouped_events:
        map_start: MapStartEvent = get_map_start(game_events)
        map_info: map_structure.MapStructureInfo = map_structure_infos.get_map_info(
            map_start.map, map_start.gold_on_left)

        game_state = GameState(map_info)
        for game_event in game_events:
            yield game_id, game_event, game_state, game_events
            game_event.modify_game_state(game_state)


def validate_big_batch():
    validate_game_data('all_gameevent.csv')


def compute_kill_matrix():
    csv_path = 'validated_all_gameevent_partitioned/gameevents_*.csv'
    events = iterate_events_from_csv(csv_path)
    queen, speed, vanilla = range(3)

    kill_matrix_by_map = {m: np.zeros((3, 3), dtype=np.int32) for m in Map}

    for game_id, event, game_state, _ in iterate_game_events_with_state(events, map_structure.MapStructureInfos()):
        if type(event) == PlayerKillEvent:
            event: PlayerKillEvent = event

            def categorize_position_id(position_id):
                if is_queen_position_id(position_id):
                    return queen
                if game_state.get_worker_by_position_id(position_id).has_speed:
                    return speed
                return vanilla

            killer_type = categorize_position_id(event.killer_position_id)
            killed_type = categorize_position_id(event.killed_position_id)
            if killed_type == queen or game_state.get_worker_by_position_id(event.killed_position_id).has_wings:
                kill_matrix_by_map[game_state.map_info.map_id][killer_type, killed_type] += 1

    pickle.dump(kill_matrix_by_map, open('kill_matrix_by_map.pkl', 'wb'))


def vectorize_worker(worker: WorkerState) -> np.ndarray:
    return np.array([worker.is_bot, worker.has_food, worker.has_speed, worker.has_wings], float)


def vectorize_team(team_state: TeamState) -> np.ndarray:
    eggs = float(team_state.eggs)
    num_food_deposits = float(sum(team_state.food_deposited))
    num_vanilla = float(sum(w.has_wings and not w.has_speed for w in team_state.workers))
    num_speed_warriors = float(sum(w.has_wings and w.has_speed for w in team_state.workers))

    parts = [[eggs, num_food_deposits, num_vanilla, num_speed_warriors],
             np.array(team_state.food_deposited, float)]
    for worker in team_state.workers:
        parts.append(vectorize_worker(worker))
    return np.concatenate(parts)


def vectorize_maidens(maidens: List[ContestableState]) -> np.ndarray:
    def encode_maiden_state(maiden_color: ContestableState):
        if maiden_color == ContestableState.NEUTRAL:
            return 0.0
        if maiden_color == ContestableState.BLUE:
            return 1.0
        if maiden_color == ContestableState.GOLD:
            return -1.0

    return np.array([encode_maiden_state(maiden) for maiden in maidens], float)


def vectorize_map_one_hot(map_id: Map) -> np.ndarray:
    return np.array([float(map_id == m) for m in Map], float)


def vectorize_snail_state(game_state: GameState, next_event: GameEvent) -> np.ndarray:
    gold_on_right_symmetry_mult = 1.0 if game_state.map_info.gold_on_left else -1.0
    snail_pos = game_state.snail_state.inferred_snail_position(next_event.timestamp) / constants.SCREEN_WIDTH - 0.5
    snail_speed = game_state.snail_state.snail_velocity / InferredSnailState.SPEED_SNAIL_PIXELS_PER_SECOND

    return np.array([snail_pos, snail_speed], float) * gold_on_right_symmetry_mult


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


def vectorize_game_states(csv_path, drop_state_probability):
    basename = os.path.basename(csv_path)
    events = iterate_events_from_csv(csv_path)
    map_structure_infos = map_structure.MapStructureInfos()
    vectorized_states = []
    labels = []

    game_started = False

    random.seed(42)
    count = 0
    for game_id, event, game_state, all_game_events in iterate_game_events_with_state(events, map_structure_infos):
        count += 1
        if count % 10000 == 0: print('vec_game_states', count)
        if type(event) == MapStartEvent:
            game_started = False
        elif type(event) == GameStartEvent:
            game_started = True

        if game_started:
            if random.random() > drop_state_probability:
                vectorized_states.append(vectorize_game_state(game_state, event))
                labels.append(1 if all_game_events[-1].winning_team == Team.BLUE else 0)

    expt_name = 'gold_symmetry_no_id'
    np.save(f'{basename}_{expt_name}_states.npy', np.vstack(vectorized_states))
    np.save(f'{basename}_{expt_name}_labels.npy', np.array(labels))


if __name__ == '__main__':
    # validate_big_batch()
    # validate_game_data('sampled_events.csv')
    vectorize_game_states('validated_all_gameevent_partitioned/gameevents_0[0-7][0-9].csv', .9)
    vectorize_game_states('validated_all_gameevent_partitioned/gameevents_0[8-9][0-9].csv', 0)
    # compute_kill_matrix()
