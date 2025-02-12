import copy
import io
import unittest

import numpy.testing

import constants
from preprocess import *
from map_structure import MapStructureInfos, MapStructureInfo


def parse_event_helper(line: str) -> Optional[GameEvent]:
    line_with_header = constants.GAME_EVENT_HEADER + '\n' + line
    return parse_event(next(csv.DictReader(io.StringIO(line_with_header))))


class GameEventTest(unittest.TestCase):
    _map_structure_infos: MapStructureInfos = MapStructureInfos()


class GameStartEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('14678452,2022-05-01 23:30:59.664+00,gamestart,"{map_day,False,0,False,17.26}",363946')
        self.assertEqual(event.map, Map.map_day)
        self.assertEqual(event.timestamp, datetime.datetime(2022, 5, 1, 23, 30, 59, 664000,
                                                            tzinfo=datetime.timezone.utc))


class MapStartEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper(
            '34229734,2022-09-19 00:13:35.058+00,mapstart,"{map_night,False,0,False,17.26}",430536')
        self.assertEqual(event.map, Map.map_night)
        self.assertFalse(event.gold_on_left)
        self.assertEqual(event.game_version, '17.26')

        event = parse_event_helper(
            '33993561,2022-09-17 01:49:48.488+00,mapstart,"{map_twilight,True,0,False,17.26}",429021')
        self.assertEqual(event.map, Map.map_twilight)
        self.assertTrue(event.gold_on_left)
        self.assertEqual(event.game_version, '17.26')


class SpawnEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('47808686,2023-02-26 02:57:08.71+00,spawn,"{7,False}",644914')
        self.assertEqual(event.position_id, 7)
        self.assertFalse(event.is_bot)

        event = parse_event_helper('48009355,2023-02-27 06:42:20.193+00,spawn,"{9,True}",645988')
        self.assertEqual(event.position_id, 9)
        self.assertTrue(event.is_bot)

    def test_modify_game_state(self):
        event = parse_event_helper('48009355,2023-02-27 06:42:20.193+00,spawn,"{9,True}",645988')
        gs = GameState(self._map_structure_infos.get_map_info(Map.map_dusk, False))
        event.modify_game_state(gs)
        self.assertTrue(gs.get_worker_by_position_id(9).is_bot)


class BerryDepositEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('35074841,2022-09-26 02:20:23.549+00,berryDeposit,"{1058,722,9}",434751')
        self.assertEqual(event.hole_x, 1058)
        self.assertEqual(event.hole_y, 722)
        self.assertEqual(event.position_id, 9)

    def test_modify_game_state(self):
        gs = GameState(self._map_structure_infos.get_map_info(Map.map_dusk, False))
        gs.get_team(Team.GOLD).workers[3].has_food = True
        orig_gs = copy.deepcopy(gs)
        event = parse_event_helper('35074841,2022-09-26 02:20:23.549+00,berryDeposit,"{1058,722,9}",434751')

        event.modify_game_state(gs)

        self.assertFalse(gs.get_team(Team.GOLD).workers[3].has_food)
        self.assertEqual(gs.berries_available, orig_gs.berries_available - 1)
        self.assertTrue(gs.get_team(Team.GOLD).food_deposited[11])


class BerryKickInEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('35079762,2022-09-26 02:47:08.521+00,berryKickIn,"{1692,110,1,True}",434770')
        self.assertEqual(event.hole_x, 1692)
        self.assertEqual(event.hole_y, 110)
        self.assertEqual(event.position_id, 1)
        self.assertTrue(event.counts_for_own_team)

    def test_modify_game_state(self):
        map_info: MapStructureInfo = self._map_structure_infos.get_map_info(Map.map_night, False)
        gs = GameState(map_info)
        orig_gs = copy.deepcopy(gs)
        event = parse_event_helper('35079762,2022-09-26 02:47:08.521+00,berryKickIn,"{1692,110,1,True}",434770')

        event.modify_game_state(gs)

        self.assertEqual(gs.berries_available, orig_gs.berries_available - 1)
        self.assertTrue(gs.get_team(Team.GOLD).food_deposited[11])

        gs = GameState(map_info)
        event = parse_event_helper('35079762,2022-09-26 02:47:08.521+00,berryKickIn,"{1692,110,2,False}",434770')

        event.modify_game_state(gs)

        self.assertEqual(gs.berries_available, orig_gs.berries_available - 1)
        self.assertTrue(gs.get_team(Team.GOLD).food_deposited[11])


class BlessMaidenEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('35079630,2022-09-26 02:46:44.638+00,blessMaiden,"{960,700,Blue}",434770')
        self.assertEqual(event.maiden_x, 960)
        self.assertEqual(event.maiden_y, 700)
        self.assertEqual(event.gate_color, ContestableState.BLUE)

        event = parse_event_helper('35079537,2022-09-26 02:46:13.12+00,blessMaiden,"{1750,740,Red}",434770')
        self.assertEqual(event.gate_color, ContestableState.GOLD)
        self.assertEqual(event.timestamp, datetime.datetime(2022, 9, 26, 2, 46, 13, 120000,
                                                            tzinfo=datetime.timezone.utc))

    def test_modify_game_state(self):
        gs: GameState = GameState(self._map_structure_infos.get_map_info(Map.map_night, True))
        event = parse_event_helper('35079630,2022-09-26 02:46:44.638+00,blessMaiden,"{700,260,Blue}",434770')
        event.modify_game_state(gs)
        self.assertEqual(gs.maiden_states[2], ContestableState.BLUE)

        flipped_gs = GameState(self._map_structure_infos.get_map_info(Map.map_night, False))
        event.modify_game_state(flipped_gs)
        self.assertEqual(flipped_gs.maiden_states[4], ContestableState.BLUE)


class CarryFoodEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('35079652,2022-09-26 02:46:49.591+00,carryFood,{10},434770')
        self.assertEqual(event.position_id, 10)

    def test_modify_game_state(self):
        gs: GameState = GameState(self._map_structure_infos.get_map_info(Map.map_night, True))
        event = parse_event_helper('35079652,2022-09-26 02:46:49.591+00,carryFood,{10},434770')
        event.modify_game_state(gs)
        self.assertTrue(gs.get_team(Team.BLUE).workers[3].has_food)


class GetOffSnailEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('14683854,2022-05-01 23:42:10.787+00,getOffSnail,"{1680,11,"""",9}",363957')
        self.assertEqual(event.snail_x, 1680)
        self.assertEqual(event.position_id, 9)


class GetOnSnailEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('14683840,2022-05-01 23:42:08.362+00,getOnSnail,"{1669,11,9}",363957')
        self.assertEqual(event.snail_x, 1669)
        self.assertEqual(event.rider_position_id, 9)


class SnailEatEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('47808785,2023-02-26 02:57:26.419+00,snailEat,"{1075,11,3,8}",644914')
        self.assertEqual(event.snail_x, 1075)
        self.assertEqual(event.rider_position_id, 3)
        self.assertEqual(event.eaten_position_id, 8)


class SnailEscapeEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('48009252,2023-02-27 06:41:16.835+00,snailEscape,"{681,491,4}",645987')
        self.assertEqual(event.snail_x, 681)
        self.assertEqual(event.escaped_position_id, 4)
# class GlanceEventTest(GameEventTest):
#     def test_parse(self):
#         event = parse_event_helper('14683945,2022-05-01 23:42:25.96+00,glance,"{91,1015,8,3}",363957')
#         self.assertEqual(event.glance_x, 91)
#         self.assertEqual(event.glance_y, 1015)
#         self.assertEqual(event.position_ids, [8, 3])


class PlayerKillEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('14683949,2022-05-01 23:42:26.433+00,playerKill,"{1822,998,3,2,Queen}",363957')
        self.assertEqual(event.killer_x, 1822)
        self.assertEqual(event.killer_y, 998)
        self.assertEqual(event.killer_position_id, 3)
        self.assertEqual(event.killed_position_id, 2)
        self.assertEqual(event.killed_player_category, PlayerCategory.Queen)

    def test_modify_game_state(self):
        gs: GameState = GameState(self._map_structure_infos.get_map_info(Map.map_night, True))
        event = parse_event_helper('14683949,2022-05-01 23:42:26.433+00,playerKill,"{1822,998,3,2,Queen}",363957')

        event.modify_game_state(gs)
        self.assertEqual(gs.get_team(Team.BLUE).eggs, 1)
        event.modify_game_state(gs)
        self.assertEqual(gs.get_team(Team.BLUE).eggs, 0)

        event = parse_event_helper('14683949,2022-05-01 23:42:26.433+00,playerKill,"{1822,998,3,4,Soldier}",363957')
        gs: GameState = GameState(self._map_structure_infos.get_map_info(Map.map_night, True))
        gs.get_team(Team.BLUE).workers[0].has_wings = True
        gs.get_team(Team.BLUE).workers[0].has_speed = True

        event.modify_game_state(gs)
        self.assertFalse(gs.get_team(Team.BLUE).workers[0].has_wings)
        self.assertFalse(gs.get_team(Team.BLUE).workers[0].has_speed)

        event = parse_event_helper('14683949,2022-05-01 23:42:26.433+00,playerKill,"{1822,998,3,4,Worker}",363957')
        gs: GameState = GameState(self._map_structure_infos.get_map_info(Map.map_night, True))
        gs.get_team(Team.BLUE).workers[0].has_food = True
        event.modify_game_state(gs)
        self.assertFalse(gs.get_team(Team.BLUE).workers[0].has_food)


class UseMaidenEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('14683930,2022-05-01 23:42:23.774+00,useMaiden,"{310,620,maiden_wings,3}",363957')
        self.assertEqual(event.maiden_x, 310)
        self.assertEqual(event.maiden_y, 620)
        self.assertEqual(event.maiden_type, MaidenType.maiden_wings)
        self.assertEqual(event.position_id, 3)

        event = parse_event_helper('14682204,2022-05-01 23:39:06.502+00,useMaiden,"{340,140,maiden_speed,8}",363957')
        self.assertEqual(event.maiden_type, MaidenType.maiden_speed)

    def test_modify_game_state(self):
        gs: GameState = GameState(self._map_structure_infos.get_map_info(Map.map_night, True))
        event = parse_event_helper('47809649,2023-02-26 02:59:43.587+00,useMaiden,"{170,740,maiden_speed,6}",644919')
        gs.get_team(Team.BLUE).workers[1].has_food = True
        event.modify_game_state(gs)
        self.assertTrue(gs.get_team(Team.BLUE).workers[1].has_speed)

        gs: GameState = GameState(self._map_structure_infos.get_map_info(Map.map_night, True))
        event = parse_event_helper('47809647,2023-02-26 02:59:43.305+00,useMaiden,"{1220,260,maiden_wings,7}",644919')
        gs.get_team(Team.GOLD).workers[2].has_food = True
        event.modify_game_state(gs)
        self.assertTrue(gs.get_team(Team.GOLD).workers[2].has_wings)


class VictoryEventTest(GameEventTest):
    def test_parse(self):
        event = parse_event_helper('34245901,2022-09-19 02:53:46.448+00,victory,"{Blue,military}",430601')
        self.assertEqual(event.winning_team, Team.BLUE)
        self.assertEqual(event.victory_condition, VictoryCondition.military)

        event = parse_event_helper('34258141,2022-09-19 04:02:28.067+00,victory,"{Gold,snail}",430647')
        self.assertEqual(event.winning_team, Team.GOLD)
        self.assertEqual(event.victory_condition, VictoryCondition.snail)

        event = parse_event_helper('34258141,2022-09-19 04:02:28.067+00,victory,"{Gold,economic}",430647')
        self.assertEqual(event.winning_team, Team.GOLD)
        self.assertEqual(event.victory_condition, VictoryCondition.economic)


class GetMapStartTest(unittest.TestCase):
    def test_find_map_start(self):
        input_events = [parse_event_helper(raw_event) for raw_event in [
            '32485092,2022-09-01 01:07:39.498+00,spawn,"{10,False}",420179',
            '32485095,2022-09-01 01:07:40.095+00,spawn,"{5,False}",420179',
            '32485097,2022-09-01 01:07:40.335+00,mapstart,"{map_night,False,0,False,17.26}",420179',
            '32485097,2022-09-01 01:07:49.335+00,gamestart,"{map_night,False,0,False,17.26}",420179',
            '32485791,2022-09-01 01:10:24.926+00,victory,"{Blue,economic}",420179'
            ]
        ]
        self.assertEqual(get_map_start(input_events), input_events[2])


class NormalizeTimestampsTest(unittest.TestCase):

    def test_normalize_timestamps(self):
        input_events = [parse_event_helper(raw_event) for raw_event in [
            '32485092,2022-09-01 01:07:39.498+00,spawn,"{10,False}",420179',
            '32485095,2022-09-01 01:07:40.095+00,spawn,"{5,False}",420179',
            '32485097,2022-09-01 01:07:40.335+00,gamestart,"{map_night,False,0,False,17.26}",420179',
            '32485791,2022-09-01 01:10:24.926+00,victory,"{Blue,economic}",420179'
            ]
        ]
        input_events = normalize_times(input_events)
        self.assertLess(input_events[0].timestamp, 0)
        self.assertLess(input_events[1].timestamp, 0)
        self.assertEqual(input_events[2].timestamp, 0)
        self.assertGreater(input_events[3].timestamp, 0)


class PositionIdToTeamTest(unittest.TestCase):
    def test(self):
        self.assertEqual(position_id_to_team(1), Team.GOLD)
        self.assertEqual(position_id_to_team(2), Team.BLUE)
        self.assertEqual(position_id_to_team(3), Team.GOLD)


class PositionIdToWorkerIndex(unittest.TestCase):
    def test(self):
        self.assertEqual(position_id_to_worker_index(3), 0)
        self.assertEqual(position_id_to_worker_index(4), 0)
        self.assertEqual(position_id_to_worker_index(9), 3)
        self.assertEqual(position_id_to_worker_index(10), 3)


def make_full_worker():
    full_worker = WorkerState()
    full_worker.has_food = True
    full_worker.has_wings = True
    full_worker.has_speed = True
    full_worker.is_bot = True
    return full_worker


class VectorizeWorkerTest(unittest.TestCase):
    def test(self):
        blank_worker = WorkerState()
        full_worker = make_full_worker()
        self.assertTrue(np.allclose(vectorize_worker(blank_worker), [0, 0, 0, 0]))
        self.assertTrue(np.allclose(vectorize_worker(full_worker), [1, 1, 1, 1]))


class VectorizeTeamTest(unittest.TestCase):
    def test(self):
        blank_team = TeamState()
        eggs_and_0_berries_deposited_and_4_bots = np.concatenate([[2.0], [0.0] * (3 + 12 + 16)])
        np.testing.assert_array_equal(vectorize_team(blank_team),
                                      eggs_and_0_berries_deposited_and_4_bots)

        full_team = TeamState()
        full_team.food_deposited = [True] * 12
        full_team.workers = [make_full_worker() for _ in range(4)]
        eggs_and_12_berries_deposited_and_4_full_workers = np.concatenate([[2.0, 12.0, 0.0, 4.0] + [1.0] * (12 + 16)])

        np.testing.assert_array_equal(vectorize_team(full_team),
                                      eggs_and_12_berries_deposited_and_4_full_workers)


class VectorizeSnailStateTest(GameEventTest):
    def test(self):
        def create_scenario(gold_on_left, rider_position_id):
            gold_on_left_str = 'True' if gold_on_left else 'False'
            events = normalize_times([
                parse_event_helper(
                    '32534898,2022-09-01 00:00:00.00+00,gamestart,"{map_night,False,0,%s,17.26}",420463' %
                    gold_on_left_str),
                parse_event_helper('32534898,2022-09-01 00:00:00.00+00,getOnSnail,"{960,491,%d}",420463' %
                rider_position_id),
                parse_event_helper('32534898,2022-09-01 00:00:02.00+00,useMaiden,"{960,700,maiden_wings,8}",420463'),
            ])
            game_state = GameState(self._map_structure_infos.get_map_info(Map.map_night, gold_on_left))
            events[0].modify_game_state(game_state)
            events[1].modify_game_state(game_state)
            return game_state, events

        # Create 4 scenarios, where [gold_on_left, gold_on_right] * [gold_rider, blue_rider]
        gold_riding_for_2_second_vec = [-2 * InferredSnailState.VANILLA_SNAIL_PIXELS_PER_SECOND / constants.SCREEN_WIDTH,
                                        -InferredSnailState.VANILLA_SNAIL_PIXELS_PER_SECOND /
                                         InferredSnailState.SPEED_SNAIL_PIXELS_PER_SECOND]

        game_state, events = create_scenario(False, 3)
        encoded_snail_state = vectorize_snail_state(game_state, events[2])
        np.testing.assert_allclose(encoded_snail_state, gold_riding_for_2_second_vec)

        game_state, events = create_scenario(True, 3)
        encoded_snail_state = vectorize_snail_state(game_state, events[2])
        np.testing.assert_allclose(encoded_snail_state, gold_riding_for_2_second_vec)

        blue_riding_for_2_second_vec = [
            2 * InferredSnailState.VANILLA_SNAIL_PIXELS_PER_SECOND / constants.SCREEN_WIDTH,
            InferredSnailState.VANILLA_SNAIL_PIXELS_PER_SECOND /
            InferredSnailState.SPEED_SNAIL_PIXELS_PER_SECOND]

        game_state, events = create_scenario(False, 4)
        encoded_snail_state = vectorize_snail_state(game_state, events[2])
        np.testing.assert_allclose(encoded_snail_state, blue_riding_for_2_second_vec)

        game_state, events = create_scenario(True, 4)
        encoded_snail_state = vectorize_snail_state(game_state, events[2])
        np.testing.assert_allclose(encoded_snail_state, blue_riding_for_2_second_vec)

if __name__ == '__main__':
    unittest.main()
