import unittest

import preprocess
from map_structure import MapStructureInfos


class MapStructureInfosTest(unittest.TestCase):
    def test_get_map_info(self):
        map_structure_infos = MapStructureInfos()
        map_structure_info = map_structure_infos.get_map_info(preprocess.Map.map_day, True)

        self.assertEqual(map_structure_info.snail_track_width, 900)
        self.assertEqual(map_structure_info.total_berries, 66)

        self.assertEqual(map_structure_info.get_berry_index(772, 923), 0)
        self.assertEqual(map_structure_info.get_berry_index(1148, 923), 0)
        self.assertEqual(map_structure_info.get_berry_index(888, 950), 11)
        self.assertEqual(map_structure_info.get_berry_index(1032, 950), 11)

        self.assertEqual(map_structure_info.get_type_and_maiden_index(410, 860),
                         (preprocess.MaidenType.maiden_speed, 0))
        self.assertEqual(map_structure_info.get_type_and_maiden_index(1360, 260),
                         (preprocess.MaidenType.maiden_wings, 4))

        map_structure_info = map_structure_infos.get_map_info(preprocess.Map.map_day, False)
        self.assertEqual(map_structure_info.get_berry_index(772, 923), 0)
        self.assertEqual(map_structure_info.get_berry_index(1148, 923), 0)
        self.assertEqual(map_structure_info.get_berry_index(888, 950), 11)
        self.assertEqual(map_structure_info.get_berry_index(1032, 950), 11)

        self.assertEqual(map_structure_info.get_type_and_maiden_index(1920 - 410, 860),
                         (preprocess.MaidenType.maiden_speed, 0))
        self.assertEqual(map_structure_info.get_type_and_maiden_index(1920 - 1360, 260),
                         (preprocess.MaidenType.maiden_wings, 4))


if __name__ == '__main__':
    unittest.main()