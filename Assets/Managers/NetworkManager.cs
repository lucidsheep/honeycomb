using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using Unity.Jobs;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public static NetworkConnection cabinetEvents;
    public static NetworkConnection hivemindEvents;
    public static NetworkConnection tournamentEvents;
    public static NetworkConnection signInEvents;

    //static IWebSocketClient webSocketClient;
    //static IWebSocketClient hivemindSocketClient;

    public string debugURLVars = "";

    public bool connectToServer = true;
    public bool usePrivateRelay = false;
    public WebSocketSharp.LogLevel networkLogLevel;

    public string serverAddress;
    public int serverPort;

    public string hivemindAddress;
    public int hivemindPort;

    public string hivemindTournamentAddress;
    public int hivemindTournamentPort;

    public string hivemindSignInAddress;

    bool isConnected = false;
    bool beginNetworkingFlag = false;
    public string sceneName = "kqpdx";
    public string cabinetName = "groundkontrol";
    public string[] overlayURLs;

    int cabinetID = -1;
    public int tournamentID = -1; //todo - don't hardcode this

    bool useSecureSockets = true;
    bool setCompleteFlag = true;

    public static int currentGameID = 0;
    public static bool localMode = false;

    public GameEvent gameEventDispatcher = new GameEvent();
    public UnityEvent<HMMatchState> tournamentEventDispatcher = new UnityEvent<HMMatchState>();
    public UnityEvent<string, GameEventData> rawEventDispatcher = new UnityEvent<string, GameEventData>();
    public UnityEvent<int, int> onTournamentTeamIDs = new UnityEvent<int, int>();
    public UnityEvent<int> onTournamentTeamPlayers = new UnityEvent<int>();
    public UnityEvent<int, int, int> onTournamentTeamWinLossData = new UnityEvent<int, int, int>();
    public UnityEvent<int> onGameID = new UnityEvent<int>();
    public UnityEvent<TeamGameStats> onTeamGameData = new UnityEvent<TeamGameStats>();
    public UnityEvent<HMTournamentQueueData> onTournamentQueueData = new UnityEvent<HMTournamentQueueData>();
    public UnityEvent<int, string> onTournamentTeamName = new UnityEvent<int, string>();

    public Queue<string> LogQueue;

    private void Awake()
    {
        instance = this;
        LogQueue = new Queue<string>();

    }
    private void Start()
    {
        var url = debugURLVars != "" ? debugURLVars : Application.absoluteURL;
        if (url != null && url != "")
        {
            //https://kq.style/overlay/scene/cabinet/?theme=pdx&version=14

            //find cabinet and scene from slashes
            var urlSplit = url.Split('/');
            var tokenFindState = -1;
            for (int i = 0; i < urlSplit.Length; i++)
            {
                if (tokenFindState == -1)
                {
                    foreach (var overlayTag in overlayURLs)
                    {
                        if (overlayTag == urlSplit[i])
                        {
                            tokenFindState = 0; //next token is scene
                            break;
                        }
                    }
                }
                else if (urlSplit[i] != "" && tokenFindState == 0) { sceneName = urlSplit[i]; tokenFindState = 1; } //next token is cabinet
                else if (urlSplit[i] != "" && tokenFindState == 1) { cabinetName = urlSplit[i]; tokenFindState = 2; break; } //ignore the rest

            }
            //find vars
            urlSplit = url.Split('?');
            if(urlSplit.Length > 1)
            {
                urlSplit = urlSplit[1].Split('&');
                foreach(var param in urlSplit)
                {
                    var pSplit = param.Split('=');
                    if (pSplit.Length > 2) continue;
                    switch (pSplit[0])
                    {
                        case "version": Debug.Log("forcing version " + pSplit[1]); break;
                        case "theme": Debug.Log("setting theme to " + pSplit[1]); ViewModel.SetTheme(pSplit[1]); break;
                        case "noCameras":
                        case "hideCameras":
                            Debug.Log("Hiding cameras");
                            PlayerCameraObserver.ShowPlayerCams(false);
                            break;
                        case "adLength": Debug.Log("setting ad rotation interval to " + pSplit[1]); SlideshowObserver.SetInterval(int.Parse(pSplit[1])); break;
                        default: Debug.Log("unknown param: " + pSplit[1]); break;
                    }
                }
            }
        }
        //todo: private relay
        cabinetEvents = new NetworkConnection(serverAddress + sceneName + "/" + cabinetName, serverPort, useSecureSockets);
        hivemindEvents = new NetworkConnection(hivemindAddress, hivemindPort, useSecureSockets);

        cabinetEvents.OnNetworkEvent.AddListener(OnReceivedTextMessage);
        cabinetEvents.OnConnectionEvent.AddListener(OnConnectionEvent);
        cabinetEvents.OnNetworkErrorEvent.AddListener(OnReceivedError);
        hivemindEvents.OnNetworkEvent.AddListener(OnReceivedTextMessageHiveMind);
        hivemindEvents.OnConnectionEvent.AddListener(OnConnectionEvent_Hivemind);

        tournamentEvents = new NetworkConnection(hivemindTournamentAddress, hivemindTournamentPort, useSecureSockets);
        tournamentEvents.OnNetworkEvent.AddListener(ProcessTournamentEvent);
        tournamentEvents.OnConnectionEvent.AddListener(OnConnectionEvent_Tournaments);

        signInEvents = new NetworkConnection(hivemindSignInAddress, hivemindPort, useSecureSockets);
        signInEvents.OnConnectionEvent.AddListener(OnConnectionEvent_SignIn);
        signInEvents.OnNetworkEvent.AddListener(ProcessSignInEvent);

        //WebSocketSharpWebSocketClient.GlobalLogLevel = networkLogLevel;

        if (!SetupScreen.setupInProgress)
            BeginNetworking();

        LSConsole.AddCommandHook("signIn", "force a [userID] with [name] to sign in on both queens", ForceSigninCommand);
        if (ViewModel.instance.appView)
        {
            LSConsole.AddCommandHook("networkLogLevel", "set networking log level to [off] [error] [debug] [all]", NetworkLogLevel);
        }
    }

    string ForceSigninCommand(string[] options)
    {
        int id = 237;
        string name = "Kevin";
        if (options.Length > 0)
            id = int.Parse(options[0]);
        if (options.Length > 1)
            name = options[1];
        ForceSignIn(id, name);

        return "";
    }

    string NetworkLogLevel(string[] options)
    {
        //if (options.Length == 0)
        //    return "Current log level: " + (cabinetEvents.socketClient as WebSocketSharpWebSocketClient).logLevel.ToString();

        WebSocketSharp.LogLevel logLevel = WebSocketSharp.LogLevel.Error;
        switch(options[0])
        {
            case "error": logLevel = WebSocketSharp.LogLevel.Error; break;
            case "debug": logLevel = WebSocketSharp.LogLevel.Debug; break;
            case "all": logLevel = WebSocketSharp.LogLevel.Info; break;
            case "off": logLevel = WebSocketSharp.LogLevel.Fatal; break;
        }

        //WebSocketSharpWebSocketClient.GlobalLogLevel = logLevel;

        //(hivemindSocketClient as WebSocketSharpWebSocketClient).logLevel = logLevel;
        //(webSocketClient as WebSocketSharpWebSocketClient).logLevel = logLevel;

        return "";
    }
    public static void BeginNetworking(string localIP = "")
    {
        //        if (instance.usePrivateRelay)
        //          instance.serverAddress = "kq.style/listener";
        Debug.Log("begin networking");
        instance.beginNetworkingFlag = true;
        if(localIP != "")
        {
            Debug.Log("local mode");
            instance.serverAddress = localIP + ":12749";
            instance.serverPort = 12749;
            instance.useSecureSockets = false;
            localMode = true;
        }

    }
    static void _BeginNetworking()
    {
        //todo: private relay
        if (localMode)
        {
            cabinetEvents.serverAddress = instance.serverAddress;
            cabinetEvents.serverPort = instance.serverPort;
            cabinetEvents.useSecureSockets = instance.useSecureSockets;
            Debug.Log("connecting to " + cabinetEvents.serverAddress + ":" + cabinetEvents.serverPort);
        }
        else
        {
            cabinetEvents.serverAddress = instance.serverAddress + instance.sceneName + "/" + instance.cabinetName;
            hivemindEvents.StartConnection();
            signInEvents.StartConnection();
            GetCabData(instance.sceneName, instance.cabinetName);
        }
        cabinetEvents.StartConnection();

        instance.beginNetworkingFlag = false;
    }
    
    private void OnConnectionEvent_Hivemind(bool connected)
    {
        if (connected)
        {
            LogQueue.Enqueue("HiveMind Connected");
        }
        else
        {
            LogQueue.Enqueue("HiveMind Disconnected");
        }
    }

    private void OnConnectionEvent_Tournaments(bool connected)
    {
        if (connected)
        {
            LogQueue.Enqueue("Tournaments Connected");
        }
        else
        {
            LogQueue.Enqueue("Tournaments Disconnected");
        }
    }

    private void OnConnectionEvent_SignIn(bool connected)
    {
        if (connected)
        {
            LogQueue.Enqueue("Sign In Connected");
        }
        else
        {
            LogQueue.Enqueue("Sign In Disconnected");
        }
    }

    private void OnConnectionEvent(bool connected)
    {
        isConnected = connected;
        if (connected)
        {
            LogQueue.Enqueue("Connected");
           

            if (usePrivateRelay)
            {
                var overlayData = new OverlayConnectData();
                overlayData.type = "overlay";
                overlayData.scene = "kqpdx";
                overlayData.cab = "groundkontrol";
                cabinetEvents.SendMessageToServer(JsonUtility.ToJson(overlayData));
            }
        } else
        {
            LogQueue.Enqueue("Disconnected");
        }

    }

    private void OnReceivedTextMessage(string message)
    {
        ProcessEvent(message, false);
    }

    private void OnReceivedLogMessage(string message)
    {
        LogQueue.Enqueue("Event server log: " + message);

    }

    private void OnReceivedLogMessageHivemind(string message)
    {
        LogQueue.Enqueue("Gamestate server log: " + message);

    }

    private void OnReceivedTextMessageHiveMind(string message)
    {
        ProcessEvent(message, true);
    }
    private void OnReceivedError(string message)
    {
        Debug.Log(message);
    }


    private void ProcessTournamentID(int id)
    {
        if (instance.tournamentID <= 0 && id > 0)
        {
            Debug.Log("setting tournament queue to tournament ID " + id);
            instance.tournamentID = id;
            tournamentEvents.serverAddress = hivemindTournamentAddress + "/" + id;
            tournamentEvents.StartConnection();
        }
    }

    private void ProcessTournamentEvent(string json)
    {
        var jsonStepOne = JsonUtility.FromJson<HMTypeCheck>(json);
        switch(jsonStepOne.type)
        {
            case "queue":
                var queueData = JsonUtility.FromJson<HMTournamentQueue>(json);
                if(queueData.data.cabinet == cabinetID)
                    onTournamentQueueData.Invoke(queueData.data);
                break;
            default: break;
        }
    }

    private void ProcessSignInEvent(string json)
    {
        var data = JsonUtility.FromJson<HMSignedInUser>(json);

        if (cabinetID < 0 || cabinetID != data.cabinet_id) return;

        if (data.action == "sign_in")
            SignInPlayer(data);
        else
            SignOutPlayer(data.player_id);
    }

    void ProcessEvent(string json, bool isHiveMindMessage)
    {
        //send to javascript
#if UNITY_WEBGL && !UNITY_EDITOR
        GameEventBroadcaster.JSGameEvent(json);
#endif
        if (isHiveMindMessage)
        {
            Debug.Log(json);
            //first turn into generic object to check event type
            var jsonStepOne = JsonUtility.FromJson<HMTypeCheck>(json);
            if (jsonStepOne.cabinet_id != cabinetID) return; //only care about our cabinet

            switch (jsonStepOne.type)
            {
                case "gameend":
                    var gameEndData = JsonUtility.FromJson<HMGameEnd>(json);
                    gameEventDispatcher.Invoke("gameend", new GameEventData());
                    break;
                case "playernames":
                    break;
                case "gamestart":
                    var gameStartData = JsonUtility.FromJson<HMGameStart>(json);
                    if (gameStartData != null && gameStartData.game_id != null)
                    {
                        currentGameID = int.Parse(gameStartData.game_id);
                        onGameID.Invoke(currentGameID);
                    }
                    setCompleteFlag = false;
                    break;
                case "match":
                    var matchData = JsonUtility.FromJson<HMMatchState>(json);
                    if (matchData.current_match == null || matchData.current_match.blue_team == null)
                    {
                        Debug.Log("null game found");
                        GameModel.newSetTimeout = 25f;
                        GameModel.newSetTeamData = null;
                        setCompleteFlag = true;
                    }
                    else
                    {
                        Debug.Log("new match data found");
                        StartCoroutine(GetTeamIDs());
                        if (!setCompleteFlag)
                        {
                            //match data during a set means an adjustment, so fix immediately
                            GameModel.instance.setPoints.property = matchData.current_match.rounds_per_match != 0 ? matchData.current_match.rounds_per_match : matchData.current_match.wins_per_match;
                            GameModel.instance.teams[0].teamName.property = matchData.current_match.blue_team;
                            GameModel.instance.teams[0].setWins.property = matchData.current_match.blue_score;
                            GameModel.instance.teams[1].teamName.property = matchData.current_match.gold_team;
                            GameModel.instance.teams[1].setWins.property = matchData.current_match.gold_score;
                            if (matchData.current_match.is_warmup)
                                GameModel.instance.isWarmup.property = true;
                            tournamentEventDispatcher.Invoke(matchData);
                            if (GameModel.instance.isWarmup.property)
                            {
                                GameModel.newSetTimeout = 1f; //reset stats after warmup ends
                                GameModel.newSetTeamData = matchData;
                            }
                        }
                        else
                        {
                            //store next set data and start countdown for showing match preview
                            GameModel.newSetTeamData = matchData;
                            GameModel.newSetTimeout = 3f;
                            if (matchData.current_match.is_warmup)
                                GameModel.instance.isWarmup.property = true;
                        }

                    }

                    break;
                default:
                    Debug.Log("unmanaged HM event type " + jsonStepOne.type);
                    break;
            }
        }
        else
        {
            string rawEvent;
            GameEventJSON jsonData;
            string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff+00");
            if (localMode)
            {

                string eventType = json.Substring(4, json.IndexOf(']') - 4);
                //Debug.Log("event type is '" + eventType + "'");
                int lastInd = json.LastIndexOf('[') + 1;
                string args = json.Substring(lastInd, json.Length - 3 - lastInd);
                //Debug.Log("args: " + args);
                jsonData = new GameEventJSON();
                jsonData.event_type = eventType;
                jsonData.values = args.Split(',');
                if(jsonData.event_type == "alive")
                {
                    //cabinet alive message, send response
                    Debug.Log("cabinet isAlive");
                    cabinetEvents.SendMessageToServer("![k[im alive],v[null]]!");
                }
            }
            else
            {
                jsonData = JsonUtility.FromJson<GameEventJSON>(json);
            }

            //string datetime = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            rawEvent = "0," + datetime + "," + jsonData.event_type + ",{";
            for (int i = 0; i < jsonData.values.Length; i++)
            {
                rawEvent += jsonData.values[i];
                if (i + 1 < jsonData.values.Length) rawEvent += ";";
            }
            rawEvent += "}," + currentGameID;

            var gd = ParseEvent(jsonData);
            rawEventDispatcher.Invoke(rawEvent, gd);
            gameEventDispatcher.Invoke(jsonData.event_type, gd);
        }
    }
    GameEventData ParseEvent(GameEventJSON jsonData)
    {
        var gd = new GameEventData();
        gd.eventType = jsonData.event_type;
        switch (jsonData.event_type)
        {
            case GameEventType.SPAWN:
                gd.playerID = int.Parse(jsonData.values[0]);
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.BERRY_GRAB:
                gd.playerID = int.Parse(jsonData.values[0]);
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.BERRY_SCORE:
                gd.coordinates = new Vector2Int(int.Parse(jsonData.values[0]), int.Parse(jsonData.values[1]));
                gd.playerID = int.Parse(jsonData.values[2]);
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.BERRY_KICK:
                gd.coordinates = new Vector2Int(int.Parse(jsonData.values[0]), int.Parse(jsonData.values[1]));
                gd.playerID = int.Parse(jsonData.values[2]);
                //could be berry for the wrong team, so check X coord of berry to figure out which team it was for
                gd.targetID = gd.coordinates.x < 960 ? UIState.blue : UIState.gold;
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.BOUNCE:
                gd.playerID = int.Parse(jsonData.values[2]);
                gd.targetID = int.Parse(jsonData.values[3]);
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.GATE_TAG:
                gd.coordinates = new Vector2Int(int.Parse(jsonData.values[0]), int.Parse(jsonData.values[1]));
                gd.playerType = jsonData.values[2];
                gd.targetType = jsonData.values[2];
                gd.teamID = jsonData.values[2] == "Blue" ? 0 : 1;
                break;
            case GameEventType.GATE_USE_START:
                gd.coordinates = new Vector2Int(int.Parse(jsonData.values[0]), int.Parse(jsonData.values[1]));
                gd.playerID = int.Parse(jsonData.values[2]);
                break;
            case GameEventType.GATE_USE_END:
                gd.coordinates = new Vector2Int(int.Parse(jsonData.values[0]), int.Parse(jsonData.values[1]));
                gd.playerType = jsonData.values[2];
                gd.playerID = int.Parse(jsonData.values[3]);
                gd.targetType = jsonData.values[2];
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.PLAYER_KILL:
                gd.coordinates = new Vector2Int(int.Parse(jsonData.values[0]), int.Parse(jsonData.values[1]));
                gd.playerID = int.Parse(jsonData.values[2]);
                gd.targetID = int.Parse(jsonData.values[3]);
                gd.targetType = jsonData.values[4];
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.SNAIL_START:
                gd.coordinates = new Vector2Int(int.Parse(jsonData.values[0]), int.Parse(jsonData.values[1]));
                gd.playerID = int.Parse(jsonData.values[2]);
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.SNAIL_EAT:
                gd.coordinates = new Vector2Int(int.Parse(jsonData.values[0]), int.Parse(jsonData.values[1]));
                gd.targetID = int.Parse(jsonData.values[3]);
                gd.playerID = int.Parse(jsonData.values[2]);
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.SNAIL_END:
                gd.coordinates = new Vector2Int(int.Parse(jsonData.values[0]), int.Parse(jsonData.values[1]));
                //no one knows what [2] is for...maybe it's the eaten id?
                gd.playerID = int.Parse(jsonData.values[3]);
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.SNAIL_ESCAPE:
                gd.coordinates = new Vector2Int(int.Parse(jsonData.values[0]), int.Parse(jsonData.values[1]));
                gd.playerID = int.Parse(jsonData.values[2]);
                gd.teamID = gd.playerID % 2 == 0 ? 0 : 1;
                break;
            case GameEventType.GAME_START:
                //map_night,True,0,False
                //0 - map name
                //1 - gold on left
                Debug.Log("game start");
                gd.mapName = jsonData.values[0]; //map name
                gd.teamPositionsInverted = jsonData.values[1] == "True";

                break;
            case GameEventType.GAME_END: break;
            case GameEventType.GAME_END_DETAIL:
                gd.teamID = jsonData.values[0] == "Blue" ? 0 : 1;
                gd.targetType = jsonData.values[1];
                break;

        }
        gd.rawPlayerID = gd.playerID;
        gd.rawTargetID = gd.targetID;
        gd.playerID = TransposePlayerID(gd.playerID);
        if (jsonData.event_type != GameEventType.BERRY_KICK) //using targetID for berry's team id, so skip
            gd.targetID = TransposePlayerID(gd.targetID);
        return gd;
    }

    public int TransposePlayerID(int cabPlayerID)
    {
        //1-2 = 2
        //3-4 = 0
        //5-6 = 1
        //7-8 = 3
        //9-10= 4
        if (cabPlayerID <= 2) return 2;
        if (cabPlayerID <= 4) return 0;
        if (cabPlayerID <= 6) return 1;
        if (cabPlayerID <= 8) return 3;
        return 4;
    }

    private void Update()
    {
        if (beginNetworkingFlag)
            _BeginNetworking();
        while (LogQueue.Count > 0)
            Debug.Log(LogQueue.Dequeue());
    }

    public static void FakeEvent(string fEvent)
    {
        instance.OnReceivedTextMessage(fEvent);
    }

    
    public static void GetCabData(string scene, string cab)
    {
        instance.StartCoroutine(_GetCabData(scene, cab));

    }
    static IEnumerator _GetCabData(string scene, string cab)
    {
        //todo - find scene id and also filter by that to avoid dupe cab names
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/game/cabinet/?name=" + cab))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMCabinetResponse>(webRequest.downloadHandler.text);
                var test = new HMCabinetResponse();
                test.results = new HMCabinet[1];
                test.results[0] = new HMCabinet();
                var testJson = JsonUtility.ToJson(test);
                if (result.results.Length > 0)
                {
                    instance.cabinetID = result.results[0].id;
                    Debug.Log("got cab id " + instance.cabinetID);
                    GetSignedInPlayers();
                    GetTournamentState();
                }
            } else
            {
                Debug.Log("getcabdata fail reason: " + webRequest.result.ToString());
            }
        }

    }

    public static void GetSignedInPlayers()
    {
        if (instance.cabinetID < 0) return;

        instance.StartCoroutine(_GetSignedInPlayers());
    }

    static IEnumerator _GetSignedInPlayers()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/game/cabinet/" + instance.cabinetID + "/signin/"))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
            GameEventBroadcaster.JSGameEvent(webRequest.downloadHandler.text);
#endif
                var result = JsonUtility.FromJson<HMSignedInResponse>(webRequest.downloadHandler.text);
                instance.ProcessSignedInPlayers(result);

            }
        }
    }

    void ProcessSignedInPlayers(HMSignedInResponse result)
    {
        for (int i = 1; i <= 10; i++)
        { 
            SignOutPlayer(i);
        }
        if (result.signed_in.Length > 0)
        {
            foreach (var player in result.signed_in)
            {
                SignInPlayer(player);
            }
        }
    }

    void SignInPlayer(HMSignedInUser player)
    {
        var team = player.player_id % 2;
        var position = instance.TransposePlayerID(player.player_id);
        GameModel.instance.teams[team].players[position].playerName.property = player.user_name;
        GameModel.instance.teams[team].players[position].hivemindID = player.user_id;

        //if we don't have additional user data for a user, grab it

        if (!PlayerStaticData.HasHivemindData(player.user_id))
        {
            Debug.Log("getting data for " + player.user_id);
            instance.StartCoroutine(GetUserData(player.user_id, team, position));
        }
        else
        {
            Debug.Log("userid " + player.user_id + " already in DB");
            GameModel.instance.teams[team].players[position].OnPlayerSignIn(PlayerStaticData.GetPlayer(player.user_id));
        }
    }

    void SignOutPlayer(int playerID)
    {
        var team = playerID % 2;
        var position = instance.TransposePlayerID(playerID);
        var player = GameModel.instance.teams[team].players[position];

        player.playerName.property = "";
        player.hivemindID = -1;
        player.OnPlayerSignOut();
    }
    void ForceSignIn(int forcedID, string name = "forceName")
    {
        var response = new HMSignedInResponse();
        response.signed_in = new HMSignedInUser[2];
        for (int i = 0; i < 2; i++)
        {
            var user = new HMSignedInUser();
            user.player_id = i;
            user.user_id = forcedID;
            user.user_name = name;
            response.signed_in[i] = user;
        }
        ProcessSignedInPlayers(response);
            
    }
    static IEnumerator GetUserData(int userID, int team = -1, int position = -1)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/user/user/" + userID + "/"))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMUserData>(webRequest.downloadHandler.text);
                Debug.Log("user data for " + userID);
                PlayerStaticData.AddPlayer(userID, result);
                if(team >= 0)
                    GameModel.instance.teams[team].players[position].OnPlayerSignIn(PlayerStaticData.GetPlayer(userID));
            }
            else
            {
                Debug.Log("user data failure");
            }
        }
    }

    public static void GetUserProfilePic(int userID, string url)
    {
        instance.StartCoroutine(_GetUserProfilePic(userID, url));
    }

    static IEnumerator _GetUserProfilePic(int userID, string url)
    {
        using (var webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                int rotation = 0;
                //read exif metadata to detect custom image rotation
                if (url.Contains(".jpg") || url.Contains(".jpeg"))
                {
                    var exifData = ExifLib.ExifReader.ReadJpeg(webRequest.downloadHandler.data, "avatar");
                    rotation = (int)exifData.Orientation;
                }
                var result = DownloadHandlerTexture.GetContent(webRequest);
                PlayerStaticData.OnPlayerProfilePic(userID, result, rotation);
            } else
            {
                Debug.Log("Error retrieving profile pic: http error " + webRequest.responseCode + ":" + webRequest.result + ", " + webRequest.error);
                Debug.Log(webRequest.downloadHandler.text);
            }
        }
    }

    public static void GetTournamentState()
    {
        instance.StartCoroutine(_GetTournamentState());
    }

    static IEnumerator _GetTournamentState()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/tournament/match/?active_cabinet_id=" + instance.cabinetID))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMTournamentResponse>(webRequest.downloadHandler.text);
                if (result.results.Length > 0)
                {
                    if (result.results[0].tournament != null)
                        GameModel.currentTournamentID = result.results[0].tournament.id;
                    else
                        GameModel.currentTournamentID = 0;
                    Debug.Log("found active tournament game at this cab, ID=" + GameModel.currentTournamentID);
                    GetTournamentTeamNames(result.results[0].blue_team, result.results[0].gold_team);
                    GetBracketData(result.results[0].bracket, result.results[0].blue_score, result.results[0].gold_score);
                    GameModel.instance.isWarmup.property = result.results[0].is_warmup;
                    GameModel.inTournamentMode = true;
                    instance.setCompleteFlag = false;
                    GetTournamentQueue(GameModel.currentTournamentID);
                } else
                {
                    Debug.Log("no tournament games found");
                    GameModel.currentTournamentID = 0;
                }
            }
        }
    }

    public static void GetTournamentQueue(int tournamentID)
    {
        instance.StartCoroutine(_GetTournamentQueue(tournamentID));
    }
    static IEnumerator _GetTournamentQueue(int tournamentID)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/tournament/queue/?tournament_id=" + tournamentID))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMTournamentQueueList>(webRequest.downloadHandler.text);
                if (result.results.Length > 0)
                {
                    foreach(var queue in result.results)
                    {
                        if (queue.cabinet != instance.cabinetID) continue;

                        instance.onTournamentQueueData.Invoke(queue);
                        Debug.Log("found active tournament queue");
                        break;
                    }
                }
            }
        }
    }

    public static void GetTournamentTeamName(int id)
    {
        instance.StartCoroutine(_GetTournamentTeamName(-1, id));
    }
    public static void GetTournamentTeamNames(int blueID, int goldID)
    {
        instance.StartCoroutine(_GetTournamentTeamName(0, blueID));
        instance.StartCoroutine(_GetTournamentTeamName(1, goldID));
    }

    static IEnumerator _GetTournamentTeamName(int internalTeamID, int tournamentTeamID)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/tournament/team/" + tournamentTeamID + "/"))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMTournamentTeam>(webRequest.downloadHandler.text);
                if (result.name != "" && internalTeamID >= 0)
                {
                    Debug.Log("teamID " + internalTeamID + " name is " + result.name);
                    GameModel.instance.teams[internalTeamID].teamName.property = result.name;
                }
                instance.onTournamentTeamName.Invoke(tournamentTeamID, result.name);
            }
        }
    }

    public static void GetBracketData(int bracketID, int blueScore, int goldScore)
    {
        instance.StartCoroutine(_GetBracketData(bracketID, blueScore, goldScore));
    }
    static IEnumerator _GetBracketData(int bracketID, int blueScore, int goldScore)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/tournament/bracket/" + bracketID + "/"))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMTournamentBracket>(webRequest.downloadHandler.text);
                Debug.Log("tournament bracket data received");
                GameModel.instance.setPoints.property = result.rounds_per_match != 0 ? result.rounds_per_match : result.wins_per_match;
                //need to update wins AFTER max score is set to properly render points
                GameModel.instance.teams[0].setWins.property = blueScore;
                GameModel.instance.teams[1].setWins.property = goldScore;
                HMMatchState match = new HMMatchState();
                match.current_match = new HMCurrentMatch();
                match.current_match.blue_team = "blue";
                match.current_match.gold_team = "gold";
                match.current_match.blue_score = blueScore;
                match.current_match.gold_score = goldScore;

                instance.tournamentEventDispatcher.Invoke(match);
            }
        }
    }
    static IEnumerator GetTeamIDs()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/tournament/match/?active_cabinet_id=" + instance.cabinetID))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMTournamentResponse>(webRequest.downloadHandler.text);
                if (result.results.Length > 0)
                {
                    instance.ProcessTournamentID(result.results[0].tournament.id);
                    GetTournamentPlayerData(result.results[0].blue_team, result.results[0].gold_team);
                    GetTournamentTeamData(result.results[0].blue_team, result.results[0].gold_team);
                    GetTournamentQueue(result.results[0].tournament.id);
                    instance.onTournamentTeamIDs.Invoke(result.results[0].blue_team, result.results[0].gold_team);
                }
                else
                {
                    Debug.Log("no tournament games found");
                }
            } else
            {
                Debug.Log("request failed reason: " + webRequest.result.ToString());
            }
        }
    }

    static void GetTournamentPlayerData(int blueTeam, int goldTeam)
    {
        instance.StartCoroutine(GetTournamentTeamPlayers(blueTeam));
        instance.StartCoroutine(GetTournamentTeamPlayers(goldTeam));
    }

    //we need a fake HM ID for tournament users that lack a normal HM account, since we index on that
    static int fakeID = 1000000;

    static IEnumerator GetTournamentTeamPlayers(int teamID)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/tournament/player/?team_id=" + teamID))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMTournamentPlayerList>(webRequest.downloadHandler.text);
                Debug.Log("found " + result.count + " tournament players for team " + teamID);
                foreach(var playerData in result.results)
                {
                    if (playerData == null) continue;
                    
                    if(playerData.user <= 0)
                    {
                        //tournament user without a HM account, needs a fake ID
                        playerData.user = fakeID;
                        fakeID++;
                    }
                    else if(!PlayerStaticData.HasHivemindData(playerData.user))
                    {
                        //if this player hasn't signed in yet today, get their normal player data as well
                        Debug.Log("also getting user data for " + playerData.user);
                        instance.StartCoroutine(GetUserData(playerData.user));
                    }
                    PlayerStaticData.AddPlayer(playerData.user, playerData);
                }
                instance.onTournamentTeamPlayers.Invoke(teamID);
            }
            else
            {
                Debug.Log("request failed reason: " + webRequest.result.ToString());
            }
        }
    }

    static void GetTournamentTeamData(int blueTeam, int goldTeam)
    {
        //need to clear fake players, since they will get duplicated by new calls to HM team lists
        PlayerStaticData.ClearFakePlayers();
        instance.StartCoroutine(GetTournamentTeamWinLoss(blueTeam));
        instance.StartCoroutine(GetTournamentTeamWinLoss(goldTeam));
    }

    static IEnumerator GetTournamentTeamWinLoss(int teamID)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/tournament/match/?team_id=" + teamID))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMTournamentResponse>(webRequest.downloadHandler.text);
                Debug.Log("found " + result.count + " sets for team " + teamID);
                int wins = 0, losses = 0;
                bool isBlue = false;
                foreach (var matchData in result.results)
                {
                    if (matchData == null) continue;

                    if(matchData.blue_team == teamID)
                    {
                        wins += matchData.blue_score;
                        losses += matchData.gold_score;
                        isBlue = true;
                    } else
                    {
                        wins += matchData.gold_score;
                        losses += matchData.blue_score;
                    }
                    instance.StartCoroutine(GetTournamentTeamMatchDataP1(teamID, matchData.id, isBlue));
                }
                instance.onTournamentTeamWinLossData.Invoke(teamID, wins, losses);
            }
            else
            {
                Debug.Log("request failed reason: " + webRequest.result.ToString());
            }
        }
    }
    
    static IEnumerator GetTournamentTeamMatchDataP1(int teamID, int setID, bool isBlue)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/game/game/?tournament_match_id=" + setID))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMGameResponse>(webRequest.downloadHandler.text);
                Debug.Log("found " + result.count + " games for team " + teamID + " set " + setID);
                foreach (var gameData in result.results)
                {
                    if (gameData == null) continue;
                    instance.StartCoroutine(GetTournamentTeamMatchDataP2(teamID, gameData.id, isBlue));
                }
            }
            else
            {
                Debug.Log("request failed reason: " + webRequest.result.ToString());
            }
        }
    }

    static IEnumerator GetTournamentTeamMatchDataP2(int teamID, int gameID, bool isBlue)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/game/game/" + gameID + "/stats/"))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMGameStats>(webRequest.downloadHandler.text);
                Debug.Log("game ID " + gameID + " data found");
                int mil_kills = 0, mil_deaths = 0, snail = 0;
                string mapName = result.map;
                string winCondition = result.win_condition;
                bool didWin = isBlue ? result.winning_team == "blue" : result.winning_team == "gold";

                int berries = isBlue ? result.berries.blue : result.berries.gold;
                HMPlayerStat military_kills = HMPlayerStat.CreateHMStatArray(webRequest.downloadHandler.text, "military_kills");
                HMPlayerStat military_deaths = HMPlayerStat.CreateHMStatArray(webRequest.downloadHandler.text, "military_deaths");
                HMPlayerStat snail_meters = HMPlayerStat.CreateHMStatArray(webRequest.downloadHandler.text, "snail_meters");

                int i = isBlue ? 2 : 1;
                while(i <= 10)
                {
                    var s = i.ToString();
                    mil_kills += military_kills[s];
                    mil_deaths += military_deaths[s];
                    snail += snail_meters[s];

                    i += 2;
                }

                var data = new TeamGameStats();
                data.didWin = didWin;
                data.teamID = teamID;
                data.gameID = gameID;
                data.winCondition = winCondition;
                data.mapName = mapName;
                data.militaryKills = mil_kills;
                data.militaryDeaths = mil_deaths;
                data.berries = berries;
                data.snailLengths = snail;
                data.totalSeconds = Mathf.FloorToInt(result.length_sec);

                instance.onTeamGameData.Invoke(data);
            }
            else
            {
                Debug.Log("request failed reason: " + webRequest.result.ToString());
            }
        }
    }

}