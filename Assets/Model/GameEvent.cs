using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/*
 *             case "spawn":
            case "blessMaiden":
            case "playernames":
            case "gamestart":
            case "carryfood":
            case "glance":
            case "reserveMaiden":
            case "useMaiden":
            case "playerKill":
*/

public class TeamType
{
    public const string BLUE = "Blue";
    public const string GOLD = "Red";
}

public class EventTargetType
{
    public const string DRONE = "Worker";
    public const string WARRIOR = "Soldier";
    public const string QUEEN = "Queen";
    public const string GATE_SPEED = "maiden_speed";
    public const string GATE_SWORD = "maiden_wings";
}
public class GameEventType
{
    //cab events
    public const string SPAWN = "spawn";
    public const string GATE_TAG = "blessMaiden";
    public const string GATE_USE_START = "reserveMaiden";
    public const string GATE_USE_CANCEL = "unreserveMaiden";
    public const string GATE_USE_END = "useMaiden";
    public const string PLAYER_NAMES = "playernames";
    public const string BERRY_GRAB = "carryFood";
    public const string BERRY_SCORE = "berryDeposit";
    public const string BERRY_KICK = "berryKickIn";
    public const string BOUNCE = "glance";
    public const string SNAIL_START = "getOnSnail";
    public const string SNAIL_END = "getOffSnail";
    public const string SNAIL_EAT = "snailEat";
    public const string SNAIL_ESCAPE = "snailEscape";
    public const string PLAYER_KILL = "playerKill";
    public const string GAME_END_DETAIL = "victory";
    public const string GAME_END = "gameend";

    //hivemind events
    public const string GAME_START = "gamestart";
    
}
public class GameEventData
{
    public int teamID = -1;
    public int playerID = -1;
    public int targetID = -1;
    public int rawPlayerID = -1;
    public int rawTargetID = -1;
    public string playerType = "";
    public string targetType = "";
    public string eventType = "";

    public string mapName = "";
    public bool teamPositionsInverted = false;

    public Vector2Int coordinates = new Vector2Int();
    
}

public class GameEvent : UnityEvent<string, GameEventData> { }

//public class GameEndEvent : UnityEvent<int>

