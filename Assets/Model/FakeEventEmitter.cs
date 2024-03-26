using UnityEngine;
using System.Collections;

public class FakeEventEmitter : MonoBehaviour
{
    public bool useEmitter;
    public bool alwaysUseMax;
    public bool sequential;
    public bool fullGame;
    public bool reversedSides = false;
    public string[] eventTypes;
    public float nextEventTimeMin = 1f;
    public float nextEventTimeMax = 5f;
    public float fullGameLength = 10f;
    public float startGameDelay = 5f;
    public float endGameDelay = 10f;

    public static FakeEventEmitter instance;

    float timer;
    float fullGameTimer;
    bool fullGameStarted = false;
    int nextEvent = 0;

    private void Awake()
    {
        instance = this;
    }
    // Use this for initialization
    void Start()
	{
        timer = 1f; // Random.Range(nextEventTimeMin, nextEventTimeMax);
        LSConsole.AddCommandHook("fakeEvents", "[start] or [stop] sending fake game events for testing", GameEventCommand);
        LSConsole.AddCommandHook("startGame", "forces the next game to start", ForceStartgame);
        LSConsole.AddCommandHook("endGame", "forces the game to end", ForceEndgame);
	}

    bool nextEventIsGameStart = false;

    string GameEventCommand(string[] parameters)
    {
        var error = "run command with [start] or [stop] to start or stop game events";
        if (parameters.Length == 0)
            instance.useEmitter = !instance.useEmitter;
        else if (parameters[0] == "start")
            instance.useEmitter = true;
        else if (parameters[0] == "stop")
            instance.useEmitter = false;
        else
            return error;

        return "";
    }

    string ForceEndgame(string[] parameters)
    {
        fullGameStarted = false;
        var e = FakeEvent("victory");
        NetworkManager.FakeEvent(e);
        return "";

    }
    string ForceStartgame(string[] parameters)
    {
        fullGameStarted = true;
        var e = FakeEvent("spawn");
        NetworkManager.FakeEvent(e);

        var ee = FakeEvent("gamestart");
        NetworkManager.FakeEvent(ee);

        return "";
    }
	// Update is called once per frame
	void Update()
	{
        if (!useEmitter) return;

        timer -= Time.deltaTime;
        fullGameTimer -= Time.deltaTime;
        if(timer <= 0f)
        {
            int id = sequential ? nextEvent : Random.Range(0, eventTypes.Length);
            if (++nextEvent >= eventTypes.Length) nextEvent = 0;
            var eType = eventTypes[id];
            timer = alwaysUseMax ? nextEventTimeMax : Random.Range(nextEventTimeMin, nextEventTimeMax);
            if (fullGame && !fullGameStarted)
            {
                fullGameStarted = true;
                eType = "spawn";
                fullGameTimer = fullGameLength;
                nextEventIsGameStart = true;
                timer = startGameDelay;
            }
            else if (fullGame && nextEventIsGameStart)
            {
                eType = "gamestart";
                nextEventIsGameStart = false;
            }
            else if (fullGame && fullGameTimer <= 0f)
            {
                fullGameStarted = false;
                nextEventIsGameStart = false;
                eType = "victory";
                timer = endGameDelay;
            }
            var e = FakeEvent(eType);
            NetworkManager.FakeEvent(e);
            if(eType == "queenKill")
            {
                //do end game message right after
                //NetworkManager.FakeEvent(FakeEvent("victory"));
            }
            
        }
	}

    public static string FakeEvent(string type)
    {
        var randPlayer = UnityEngine.Random.Range(1, 11);
        var randTeam = randPlayer % 2;
        var randTeamName = randTeam == 0 ? "Blue" : "Red";
        var randTarget = 0;
        var randTargetType = "";
        var randCoord = new Vector2Int(Random.Range(0, 1920), Random.Range(0, 1080));
        string rawType = type.IndexOf("queenKill") > -1 ? "playerKill" : type;
        var ret = NetworkManager.localMode ? "![k[" + rawType + "],v["
            : "{\"event_type\":\"" + rawType + "\",\"values\":[";
        switch (type)
        {
            case "queenKill":
            case "queenKillBlue":
            case "queenKillGold": //hack to force a queen kill
            case GameEventType.PLAYER_KILL:
                do
                {
                    randTarget = UnityEngine.Random.Range(1, 10);
                } while ((randPlayer % 2) + (randTarget % 2) != 1);
                randTargetType = randTarget < 3 ? "Queen" : "Worker";
                if (type == "queenKill")
                {
                    randPlayer = randPlayer % 2 == 0 ? 1 : 2;
                    randTarget = 3 - randPlayer;
                    randTargetType = "Queen";
                } else if(type == "queenKillBlue")
                {
                    randPlayer = 2;
                    randTarget = 1;
                    randTargetType = "Queen";
                } else if(type == "queenKillGold")
                {
                    randPlayer = 1;
                    randTarget = 2;
                    randTargetType = "Queen";
                }
                ret += quote(randCoord.x) + "," + quote(randCoord.y) + "," + quote(randPlayer) + "," + quote(randTarget) + "," + quote(randTargetType);
                //ret += "\"1461\",\"496\",\"" + randTarget + "\",\"" + randPlayer + "\",\"" + randTargetType + "\"";
                break;
            default:
                break;
            case GameEventType.BERRY_SCORE:
                randPlayer = Random.Range(3, 10);
                ret += quote(randCoord.x) + "," + quote(randCoord.y) + "," + quote(randPlayer);
                //ret += "\"1461\",\"496\",\"" + randTarget + "\",\"" + randPlayer + "\",\"" + randTargetType + "\"";
                break;
            case GameEventType.SNAIL_START:
                ret += quote(Mathf.FloorToInt(SnailModel.currentPosition.property)) + "," + quote(11) + "," + quote(randPlayer);
                break;
            case GameEventType.SNAIL_END:
                ret += quote(Mathf.FloorToInt(SnailModel.currentPosition.property)) + "," + quote(11) + "," + quote(0) + "," + quote(randPlayer);
                break;
            case GameEventType.SNAIL_EAT:
                ret += quote(Mathf.FloorToInt(SnailModel.currentPosition.property)) + "," + quote(11) + "," + quote(0) + "," + quote(randPlayer);
                break;
            case GameEventType.GAME_START:
                ret += quote(MapDB.instance.allMaps[Random.Range(0, MapDB.instance.allMaps.Length)].name) + ","
                    + quote(instance.reversedSides ? "True" : "False") + "," + quote(0) + "," + quote("False") + "," + quote("16.9");
                break;
            case GameEventType.SPAWN:
                ret += quote(1) + "," + quote("False");
                break;
            case GameEventType.GAME_END_DETAIL:
                var endWin = Random.Range(0, 3);
                var endString = endWin == 0 ? "military" : endWin == 1 ? "economic" : "snail";
                ret += quote(randTeam == 0 ? "Blue" : "Red") + "," + quote(endString);
                break;
            case GameEventType.BOUNCE:
                do
                {
                    randTarget = Random.Range(1, 11);
                } while (randTarget == randPlayer);
                ret += quote(randCoord.x) + "," + quote(randCoord.y) + "," + quote(randPlayer) + "," + quote(randTarget);
                break;
            case GameEventType.GATE_TAG:
                ret += quote(100 * Random.Range(1, 3)) + "," + quote(100 * Random.Range(1,3)) + "," + quote(randTeamName);
                break;
            case GameEventType.GATE_USE_END:
                var randGate = Random.Range(0, 2) == 1 ? EventTargetType.GATE_SWORD : EventTargetType.GATE_SPEED;

                ret += quote(100 * Random.Range(1, 3)) + "," + quote(100 * Random.Range(1, 3)) + "," + quote(randGate) + "," + quote(randPlayer);
                break;


        }
        ret += NetworkManager.localMode ? "]]!"
            : "]}";
        return ret;
    }

    static string quote(int val)
    {
        if (NetworkManager.localMode) return val.ToString();

        return "\"" + val + "\"";
    }

    static string quote(string val)
    {
        if (NetworkManager.localMode) return val;

        return "\"" + val + "\"";
    }
}

