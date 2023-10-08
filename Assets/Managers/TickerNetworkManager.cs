using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using DG.Tweening;
using System.Reflection;


public class TickerNetworkManager : MonoBehaviour
{
    static TickerNetworkManager instance;

    public float cabinetSwapTime = 10f;
    public string[] debugCabsToWatch;

    public TextMeshPro cabNameTxt;
    public SpriteRenderer[] cabNameBG;
    public Color defaultColor, redCabColor, purpleCabColor, blueCabColor;

    public TickerLineItem lineItemTemplate;
    public TickerLineItem nextUpTemplate;

    TickerLineItem curLineItem;

    float nextSwapTime = -1f;
    int curCabIndex = 0;


    public class TickerCabinetTeamState
    {
        public string teamName;
        public int queenScore, berryScore, snailScore, seriesScore;

        public void CopyFrom(TickerCabinetTeamState other)
        {
            teamName = other.teamName;
            queenScore = other.queenScore;
            berryScore = other.berryScore;
            snailScore = other.snailScore;
            seriesScore = other.seriesScore;
        }
    }
    public class TickerCabinetState
    {
        public string cabName;
        public int cabID;
        public bool gameInProgress = true;
        public bool gameWinner; //false = blue, true = gold
        public string winType;
        public TickerCabinetTeamState blueTeam;
        public TickerCabinetTeamState goldTeam;
        public TickerCabinetTeamState blueTeamCached;
        public TickerCabinetTeamState goldTeamCached;
        public bool isTournamentPlay;
        public bool isTournamentSeriesFinished;
        public float gameTime;
        public string mapName;
        public bool isNextUpAvailable;
        public string nextUpBlueTeam;
        public string nextUpGoldTeam;
        public NetworkConnection cabEvents;

        public TickerCabinetState(string cName)
        {
            cabName = cName;
            blueTeam = new TickerCabinetTeamState();
            goldTeam = new TickerCabinetTeamState();
            blueTeamCached = new TickerCabinetTeamState();
            goldTeamCached = new TickerCabinetTeamState();
            GetCabData("", cabName, OnCabID);
        }

        public void OnCabID(int id)
        {
            cabID = id;
            cabEvents = new NetworkConnection("kqhivemind.com/ws/ingame_stats", 8080, true);
            cabEvents.OnConnectionEvent.AddListener(OnConnectState);
            cabEvents.OnNetworkEvent.AddListener(OnMessage);

            cabEvents.StartConnection();
        }

        void OnConnectState(bool state)
        {
            if(state)
            {
                var connectMessage = new HMInGameStats_ConnectSettings();
                connectMessage.type = "subscribe";
                connectMessage.cabinet_id = cabID;
                cabEvents.SendMessageToServer(JsonUtility.ToJson(connectMessage));
            }
        }

        HMPlayerStat CreateHMStatArray(string message, string arrayName)
        {
            var ret = new HMPlayerStat();
            int startIndex = message.IndexOf(arrayName) + arrayName.Length + 2;
            int endIndex = message.IndexOf('}', startIndex + 1);
            if (startIndex > -1 && endIndex - startIndex > 5) //watch for empty array case
            {
                var substring = message.Substring(startIndex, endIndex - startIndex);
                //do conversion thingy and pray
                ret.ConvertJson(substring);
            }
            return ret;
        }
        void OnMessage(string message)
        {
            var data = JsonUtility.FromJson<HMInGameStats>(message);
            gameTime = data.game_time;
            if(!isTournamentPlay)
            {
                blueTeam.teamName = "Blue Team";
                goldTeam.teamName = "Gold Team";
            }
            float snailTrackLength = 900f;
            if (!MapDB.maps.ContainsKey(data.map_name)) //null or unknown map
            {
                mapName = "";
            }
            else
            {
                mapName = data.map_name == "" ? "???" : MapDB.maps[data.map_name].display_name;
                snailTrackLength = MapDB.maps[data.map_name].snail_track_width;
            }

            blueTeam.berryScore = goldTeam.berryScore = blueTeam.queenScore = goldTeam.queenScore = blueTeam.snailScore = goldTeam.snailScore = 0;

            //todo - berries sometimes don't add up to 12. possibly some kick ins arn't being counted? also snail does not always add up to 100%
            data.total_berries = CreateHMStatArray(message, "total_berries");
            data.queen_kills = CreateHMStatArray(message, "queen_kills");
            data.snail_distance = CreateHMStatArray(message, "snail_distance");

            //Debug.Log("ticker data: " + data.total_berries.ToString() + "," + data.kills.Length);
            for(int i = 1; i < 11; i++)
            {
                var teamToUse = i % 2 == 1 ? goldTeam : blueTeam;
                //Debug.Log(data.total_berries[i.ToString()]);
                teamToUse.berryScore += data.total_berries[i.ToString()];
                teamToUse.queenScore += data.queen_kills[i.ToString()];
                teamToUse.snailScore += data.snail_distance[i.ToString()];
            }

            bool neutralSnail = blueTeam.snailScore - goldTeam.snailScore == 0;
            bool blueWinningSnail = blueTeam.snailScore > goldTeam.snailScore;

            if (neutralSnail)
                blueTeam.snailScore = goldTeam.snailScore = 0;
            else if(blueWinningSnail)
            {

                blueTeam.snailScore = Mathf.FloorToInt(((blueTeam.snailScore - goldTeam.snailScore) * 100.0f) / snailTrackLength);
                goldTeam.snailScore = 0;
            } else
            {
                goldTeam.snailScore = Mathf.FloorToInt(((goldTeam.snailScore - blueTeam.snailScore) * 100.0f) / snailTrackLength);
                blueTeam.snailScore = 0;
            }



        }

        public void OnTournamentState(HMTournamentMatch match)
        {

        }
    }

    static List<TickerCabinetState> subscribedCabinets = new List<TickerCabinetState>();

    private void Awake()
    {
        instance = this;
        ViewModel.onThemeChange.AddListener(OnThemeChange);
    }

    void OnThemeChange()
    {
        transform.position = new Vector3(0f, -4.86f + (ViewModel.currentTheme.showTicker ? 0f : -2f), 0f);
    }
    public static void Init()
    {
        Init(instance.debugCabsToWatch);
    }
    public static void Init(params string[] cabsToWatch)
    {
        NetworkManager.hivemindEvents.OnNetworkEvent.AddListener(OnHivemindMessage);

        foreach(var cab in cabsToWatch)
        {
            Debug.Log("adding cab '" + cab + "'");
            var cabState = new TickerCabinetState(cab);
            subscribedCabinets.Add(cabState);
        }
        //add our own cab for Next Up
        var thisCab = new TickerCabinetState(NetworkManager.instance.cabinetName);
        subscribedCabinets.Add(thisCab);
        instance.nextSwapTime = instance.cabinetSwapTime;
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(nextSwapTime > 0f)
        {
            nextSwapTime -= Time.deltaTime;
            if(nextSwapTime <= 0f)
            {
                nextSwapTime = cabinetSwapTime;
                UpdateDisplay();
            }
        }
    }

    (string, Color) GetCabNameAndColor(string cabName)
    {
        //beautifully hardcoded ©2023
        switch(cabName)
        {
            case "groundkontrol": return ("Ground Kontrol", blueCabColor);
            case "pdxprivate": return ("Private Cab", purpleCabColor);
            case "goingaming": return ("Goin Gaming", redCabColor);
            default: return (cabName, defaultColor);
        }
    }
    void UpdateDisplay()
    {
        var curCab = subscribedCabinets[curCabIndex];
        var nameAndColor = GetCabNameAndColor(curCab.cabName);
        bool nextUp = (curCab.cabName == NetworkManager.instance.cabinetName);
        curCabIndex = curCabIndex + 1 >= subscribedCabinets.Count ? 0 : curCabIndex + 1;

        //next up isn't available, skip this line
        if(nextUp && !curCab.isNextUpAvailable)
        {
            nextSwapTime = .1f;
            return;
        }
        if (nextUp)
            nameAndColor.Item1 = "Next Up";
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                if (curLineItem != null)
                    curLineItem.EndLine();
                foreach (var sr in cabNameBG)
                {
                    sr.DOColor(nameAndColor.Item2, .5f);
                }
            })
            .AppendInterval(.5f)
            .AppendCallback(() =>
            {
                cabNameTxt.text = nameAndColor.Item1;
            }).AppendInterval(.25f)
            .AppendCallback(() =>
            {
                var templateToUse = nextUp ? nextUpTemplate : lineItemTemplate;
                curLineItem = Instantiate(templateToUse, transform);
                if(!nextUp)
                    (curLineItem as TickerLineGameStatus).Init(curCab);
                else
                    (curLineItem as TickerLineNextUp).Init(curCab);
                curLineItem.StartLine();
            });
        

        
    }
    static void OnHivemindMessage(string message)
    {
        var jsonStepOne = JsonUtility.FromJson<HMTypeCheck>(message);
        int id = jsonStepOne.cabinet_id;
        TickerCabinetState targetCab = null;
        foreach(var cab in subscribedCabinets)
        {
            if(cab.cabID == id)
            {
                targetCab = cab;
            }
        }
        if (targetCab == null) return;

        switch (jsonStepOne.type)
        {
            case "gameend":
                var gameEndData = JsonUtility.FromJson<HMGameEnd>(message);

                targetCab.winType = gameEndData.win_condition;
                targetCab.gameWinner = gameEndData.winning_team != "blue";
                targetCab.gameInProgress = false;
                if (!targetCab.gameWinner)
                    targetCab.blueTeam.seriesScore++;
                else
                    targetCab.goldTeam.seriesScore++;
                //cache results until next game starts
                targetCab.blueTeamCached.CopyFrom(targetCab.blueTeam);
                targetCab.goldTeamCached.CopyFrom(targetCab.goldTeam);

                break;
            case "playernames":
                break;
            case "gamestart":
                targetCab.gameInProgress = true;
                targetCab.isTournamentSeriesFinished = false;
                if(!targetCab.isTournamentPlay)
                {
                    targetCab.blueTeam.teamName = "Blue Team";
                    targetCab.goldTeam.teamName = "Gold Team";
                    targetCab.blueTeam.seriesScore = targetCab.goldTeam.seriesScore = 0;
                }
                //var gameStartData = JsonUtility.FromJson<HMGameStart>(message);
                break;
            case "match":
                var matchData = JsonUtility.FromJson<HMMatchState>(message);
                if (matchData.current_match == null || matchData.current_match.blue_team == null)
                {
                    //tournament ended
                    targetCab.isTournamentSeriesFinished = targetCab.isTournamentPlay;
                    targetCab.isTournamentPlay = false;
                }
                else
                {
                    //tournament game found
                    targetCab.isTournamentPlay = true;
                    targetCab.isTournamentSeriesFinished = false;

                    targetCab.blueTeam.teamName = matchData.current_match.blue_team;
                    targetCab.goldTeam.teamName = matchData.current_match.gold_team;

                    targetCab.blueTeam.seriesScore = matchData.current_match.blue_score;
                    targetCab.goldTeam.seriesScore = matchData.current_match.gold_score;

                }
                //next up
                if (matchData.on_deck != null && matchData.on_deck.blue_team != null)
                {
                    targetCab.isNextUpAvailable = true;
                    targetCab.nextUpBlueTeam = matchData.on_deck.blue_team;
                    targetCab.nextUpGoldTeam = matchData.on_deck.gold_team;
                } else
                {
                    targetCab.isNextUpAvailable = false;
                }
                break;
            default:
                break;
        }
    }
    delegate void CabIdDelegate(int id);

    static void GetCabData(string scene, string cab, CabIdDelegate onID)
    {
        instance.StartCoroutine(_GetCabData(scene, cab, onID));

    }
    static IEnumerator _GetCabData(string scene, string cab, CabIdDelegate onID)
    {
        //todo - find scene id and also filter by that to avoid dupe cab names
        //hack
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://kqhivemind.com/api/game/cabinet/?name=" + cab))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<HMCabinetResponse>(webRequest.downloadHandler.text);
                if (result.results.Length > 0)
                {
                    onID.Invoke(result.results[0].id);
                }
            }
            else
            {
                Debug.Log("getcabdata fail reason: " + webRequest.result.ToString());
            }
        }

    }
}

