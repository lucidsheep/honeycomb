using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public class KQuityManager : MonoBehaviour
{
    public bool useManager = true;
	public int port;
    public float reconnectTime = 30f;

    float nextReconnect = 0f;
    float curProb = .5f;

    float snailDummyEventTimer = -1f;
    GameEventData snailData;

    public static LSProperty<bool> gameInProgress = new LSProperty<bool>(false);
    public Queue<GameEventData> queuedEvents = new Queue<GameEventData>();

    NetworkConnection wsConnection;
    public static bool isConnected;

    public static UnityEvent<float, GameEventData> onBlueWinProbability = new UnityEvent<float, GameEventData>();
    public static KQuityManager instance;

    public List<string> skippableEvents = new List<string>() {"gameend", "playernames",
                        "reserveMaiden", "unreserveMaiden",
                        "cabinetOnline", "cabinetOffline",
                        "bracket", "tstart", "tournamentValidation", "checkIfTournamentRunning"
                        };

    static string DummySnailEvent()
    {
        //send this event that has no bearing on gamestate, just to increment snail progress
        string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff+00");
        string rawEvent = "0," + datetime + "," + "glance" + ",{480;480;1;2;}," + NetworkManager.currentGameID;
        return rawEvent;
    }
    private void Awake()
    {
        instance = this;
    }
    // Use this for initialization
    void Start()
	{
        if (!useManager) return;

        wsConnection = new NetworkConnection("localhost:" + port, port, false);
        wsConnection.OnConnectionEvent.AddListener(OnConnected);
        wsConnection.OnNetworkEvent.AddListener(OnReceivedTextMessage);

        NetworkManager.instance.rawEventDispatcher.AddListener(OnRawEvent);

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        System.Diagnostics.Process.Start(Application.dataPath + "\\..\\" + "kquity.bat");
#endif
#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
        System.Diagnostics.Process.Start("/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal", Application.dataPath + "/kquity.sh");
#endif
        Connect();
	}

    private void Connect()
    {
        wsConnection.StartConnection();
    }
    void OnRawEvent(string rawEvent, GameEventData data)
    {
        if (!isConnected) return;
        if (data.eventType == GameEventType.GAME_START)
        {
            curProb = .5f;
            if ((data.mapName != "map_day") &&
                    (data.mapName != "map_night") &&
                    (data.mapName != "map_dusk") &&
                    (data.mapName != "map_twilight"))
            {
                gameInProgress.property = false;
                return; //don't send events if it's a non-standard map
            }
            gameInProgress.property = true;
            snailData = null;
            Debug.Log("queuedEvents " + queuedEvents.Count);
            queuedEvents.Clear();
        }
        if (gameInProgress.property && skippableEvents.FindIndex(x => x == data.eventType) < 0)
        {
            queuedEvents.Enqueue(data);
            SendMessageToServer(rawEvent);
            if(data.eventType == GameEventType.SNAIL_START)
            {
                snailData = data;
                snailDummyEventTimer = .25f;
            } else if(data.eventType == GameEventType.SNAIL_END)
            {
                snailData = null;
                snailDummyEventTimer = -1f;
            }
        }
        if(data.eventType == GameEventType.GAME_END_DETAIL)
        {
            gameInProgress.property = false;
            snailData = null;
        }
    }

    private void OnReceivedTextMessage(string message)
    {
        float blueOdds = -1f;
        float.TryParse(message, out blueOdds);
        if(blueOdds >= 0f)
        {
            var delta = blueOdds - curProb;
            curProb = blueOdds;
            var data = queuedEvents.Dequeue();
            onBlueWinProbability.Invoke(blueOdds, data);
            if (data.teamID > -1)
            {
                int score = Mathf.FloorToInt(delta * 1000f) * (data.teamID == 1 ? -1 : 1);
                //event subject
                if(data.playerID > -1)
                    GameModel.instance.teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.Value, score, 1);
                else
                {
                    //distribute score to all team members
                    foreach(var p in GameModel.instance.teams[data.teamID].players)
                        p.AddDerivedStat(PlayerModel.StatValueType.Value, score / 5, 1);
                }
                //event target
                score *= -1;
                if (data.targetID > -1)
                    GameModel.instance.teams[1 - data.teamID].players[data.targetID].AddDerivedStat(PlayerModel.StatValueType.Value, score, 1);
                else
                {
                    //distribute score to all team members
                    foreach (var p in GameModel.instance.teams[1 - data.teamID].players)
                        p.AddDerivedStat(PlayerModel.StatValueType.Value, score / 5, 1);
                }
            }
        } else
        {
            Debug.Log("KQuity message: " + message);
        }
    }

    private void OnConnected(bool connected)
    {
        isConnected = connected;
        //Debug.Log("kquity " + (connected ? "" : "dis") + "connected");
        gameInProgress.property = false;
    }

    private void SendMessageToServer(string message)
    {
        wsConnection.SendMessageToServer(message);
    }

    // Update is called once per frame
    void Update()
	{
        if(isConnected && snailDummyEventTimer > 0f)
        {
            snailDummyEventTimer -= Time.deltaTime;
            if(snailDummyEventTimer <= 0f && snailData != null)
            {
                //send dummy event
                var rawEvent = DummySnailEvent();
                OnRawEvent(rawEvent, snailData);
            }
        }
	}

    private void OnDestroy()
    {
        if (isConnected)
            SendMessageToServer("exit");
    }
}

