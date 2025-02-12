import collections
from typing import Dict, Type, Any

import copy
from constants import SCREEN_WIDTH, MaidenType, Map
import json


Coord: Type = tuple[int, int]


class MapStructureInfo:

    def get_berry_index(self, berry_x, berry_y) -> int:
        berry_coord: Coord = (berry_x, berry_y)
        if berry_coord in self._gold_berries:
            return self._gold_berries[berry_coord]
        elif berry_coord in self._blue_berries:
            return self._blue_berries[berry_coord]
        raise ValueError('Berry not found: ({}, {})'.format(berry_x, berry_y))

    def get_type_and_maiden_index(self, maiden_x, maiden_y) -> (MaidenType, int):
        if not (maiden_x, maiden_y) in self._maidens:
            raise ValueError('Maiden not found: ({}, {})'.format(maiden_x, maiden_y))
        return self._maidens[(maiden_x, maiden_y)]

    def __init__(self, map_id: Map, raw_info: Dict[str, Any]):
        self.map_id = map_id
        # Not meant to be called directly, use MapStructureInfos.get_map_info()
        self._gold_berries: Dict[Coord, int] = {}
        self._blue_berries: Dict[Coord, int] = {}
        self._maidens: Dict[Coord, (MaidenType, int)] = {}

        def index_coord_list(l):
            return {tuple(value): i for i, value in enumerate(l)}

        self.gold_on_left = True
        self._gold_berries = index_coord_list(raw_info['left_berries'])
        self._blue_berries = index_coord_list(raw_info['right_berries'])

        for idx, maiden in enumerate(raw_info['maiden_info']):
            maiden_type, x, y = maiden
            maiden_type = MaidenType(maiden_type)
            self._maidens[(x, y)] = (maiden_type, idx)

        self.snail_track_width = raw_info['snail_track_width']
        self.total_berries = raw_info['total_berries']

    def flip_sides(self) -> 'MapStructureInfo':
        flipped = copy.deepcopy(self)
        flipped._gold_berries, flipped._blue_berries = flipped._blue_berries, flipped._gold_berries
        flipped._maidens = {(SCREEN_WIDTH - k[0], k[1]): v for k, v in flipped._maidens.items()}
        flipped.gold_on_left = not self.gold_on_left
        return flipped


class MapStructureInfos(object):

    def __init__(self):
        self.backing = {}
        with open('map_structure_info.json', 'rb') as f:
            raw_info_dict = json.load(f)
            for map_name, raw_info in raw_info_dict.items():
                original = MapStructureInfo(Map[map_name], raw_info)
                self.backing[(Map[map_name], True)] = original
                self.backing[(Map[map_name], False)] = original.flip_sides()

    def get_map_info(self, map: Map, gold_on_left: bool) -> MapStructureInfo:
        return self.backing[(map, gold_on_left)]
