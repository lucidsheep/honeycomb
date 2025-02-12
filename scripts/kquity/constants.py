from enum import Enum

SCREEN_WIDTH = 1920


GAME_EVENT_HEADER = 'id,timestamp,event_type,values,game_id'


class ContestableState(Enum):
    BLUE = 0
    GOLD = 1
    NEUTRAL = 2


class Team(Enum):
    BLUE = 0
    GOLD = 1


class VictoryCondition(Enum):
    military = 'military'
    economic = 'economic'
    snail = 'snail'


class PlayerCategory(Enum):
    Queen = 'Queen'
    Soldier = 'Soldier'
    Worker = 'Worker'


class MaidenType(Enum):
    maiden_speed = 'maiden_speed'
    maiden_wings = 'maiden_wings'


class Map(Enum):
    map_day = 'map_day'
    map_night = 'map_night'
    map_dusk = 'map_dusk'
    map_twilight = 'map_twilight'
