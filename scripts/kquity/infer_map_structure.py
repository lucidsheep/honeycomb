import collections
import pickle
import json
from typing import Dict

import constants
import preprocess

# This takes a sample of game logs and extracts berry locations and maiden locations from them.

class MapStructureRawInfo:
    def __init__(self):
        self.drone_kickin_positions = []
        self.drone_deposit_positions = []
        self.maiden_info = set()
        self.game_count = 0


def skip_bad_events(row: Dict[str, str]) -> bool:
    values = row['values']
    event_type = row['event_type']
    is_good_event_type = event_type in ['berryDeposit', 'berryKickIn', 'mapstart', 'useMaiden']
    return (row['timestamp'] < '2022' or
            'map_twilight2' in values or
            'bonus' in values or
            not is_good_event_type)


def serialize_raw_info():
    map_structure_info_dict = collections.defaultdict(MapStructureRawInfo)

    events = []
    #for filename in glob.glob('export_20230227_223126_all_data/gameevent.csv'):
    for filename in ['sampled_events.csv']:
        events += preprocess.read_events_from_csv(filename, skip_bad_events)
    grouped_events = preprocess.group_events_by_game_and_normalize_time(events)

    map_structure_info_dict = collections.defaultdict(MapStructureRawInfo)

    for idx, (game_id, game_events) in enumerate(grouped_events.items()):
        map_start: preprocess.MapStartEvent = preprocess.get_map_start(game_events)
        map_name = map_start.map.value

        if map_start.version != '17.26':
            continue

        raw_info = map_structure_info_dict[map_name]
        raw_info.game_count += 1
        for event in game_events:
            if type(event) == preprocess.BerryKickInEvent:
                raw_info.drone_kickin_positions.append((event.hole_x, event.hole_y))
            elif type(event) == preprocess.BerryDepositEvent:
                raw_info.drone_deposit_positions.append((event.hole_x, event.hole_y))
            elif type(event) == preprocess.UseMaidenEvent:
                raw_info.maiden_info.add((event.maiden_type, event.maiden_x, event.maiden_y))

    open('map_structure_raw_info.pickle', 'wb').write(pickle.dumps(map_structure_info_dict))


def condense_raw_info_into_json():
    map_structure_info_dict = pickle.loads(open('map_structure_raw_info.pickle', 'rb').read())

    serializable_summary = {}

    for map_name, map_structure_info in map_structure_info_dict.items():
        location_counter = collections.Counter(map_structure_info.drone_deposit_positions +
                                               map_structure_info.drone_kickin_positions)
        real_berry_locations_with_counts = location_counter.most_common(24)
        assert len(real_berry_locations_with_counts) == 24
        assert sum(x[1] for x in real_berry_locations_with_counts) / location_counter.total() > .995

        real_berry_locations = sorted([x[0] for x in real_berry_locations_with_counts])
        real_berry_locations_set = set(real_berry_locations)

        for x, y in real_berry_locations_set:
            assert (constants.SCREEN_WIDTH - x, y) in real_berry_locations_set

        # Left and right berries that are mirrored have the same index.
        left_berries = sorted([x for x in real_berry_locations if x[0] < constants.SCREEN_WIDTH / 2])
        right_berries = [(constants.SCREEN_WIDTH - x, y) for x, y in left_berries]

        serializable_maiden_info = sorted([(maiden_type.value, x, y) for maiden_type, x, y in
                                           map_structure_info.maiden_info])

        snail_track_widths = {'map_day': 900, 'map_night': 700, 'map_dusk': 900, 'map_twilight': 900}
        total_berries = {'map_day': 66, 'map_night': 54, 'map_dusk': 66, 'map_twilight': 60}

        serializable_summary[map_name.value] = {
            'left_berries': left_berries,
            'right_berries': right_berries,
            'maiden_info': serializable_maiden_info,
            'snail_track_width': snail_track_widths[map_name.value],
            'total_berries': total_berries[map_name.value],
        }
    json.dump(serializable_summary, open('map_structure_info.json', 'w'), indent=2)


if __name__ == '__main__':
    # serialize_raw_info()
    condense_raw_info_into_json()
