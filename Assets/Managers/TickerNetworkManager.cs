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

    public GoogleSheetsDB googleSheetsDB;
    GoogleSheet txtSheet;

    TickerLineItem curLineItem;

    float nextSwapTime = -1f;
    int curItemIndex = 0;

    public class TickerItem
    {
        public enum Type { CABINET, NEXT_UP, MESSAGE };
        public Type type;
        public string title, message;
        public float duration;
        public Color color;
        public TickerCabinetState cabState;
    }

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

    static List<TickerItem> tickerItems = new List<TickerItem>();
    private void Awake()
    {
        instance = this;
        ViewModel.onThemeChange.AddListener(OnThemeChange);
        if(googleSheetsDB != null)
            googleSheetsDB.OnDownloadComplete += OnReceivedNewsEntries;
    }

    void OnThemeChange()
    {
        transform.position = new Vector3(0f, -4.89f + (ViewModel.currentTheme.showTicker ? 0f : -2f), 0f);
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
            if (cab == "") continue;

            Debug.Log("adding cab '" + cab + "'");
            var cabState = new TickerCabinetState(cab);
            var item = new TickerItem();
            item.cabState = cabState;
            item.title = cab;
            item.duration = instance.cabinetSwapTime;
            item.type = TickerItem.Type.CABINET;
            item.color = instance.defaultColor;
            tickerItems.Add(item);
        }
        //add our own cab for Next Up
        var thisCab = new TickerCabinetState(NetworkManager.instance.cabinetName);
        var thisItem = new TickerItem();
        thisItem.cabState = thisCab;
        thisItem.title = "Next Up";
        thisItem.duration = instance.cabinetSwapTime;
        thisItem.type = TickerItem.Type.NEXT_UP;
        thisItem.color = instance.defaultColor;
        tickerItems.Add(thisItem);
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


    void UpdateDisplay()
    {
        var curItem = tickerItems[curItemIndex];
        curItemIndex = curItemIndex + 1 >= tickerItems.Count ? 0 : curItemIndex + 1;

        //next up isn't available, skip this line
        if(curItem.type == TickerItem.Type.NEXT_UP && !curItem.cabState.isNextUpAvailable)
        {
            nextSwapTime = .1f;
            return;
        }
        if (curItem.duration > 0f)
            nextSwapTime = curItem.duration;

        DOTween.Sequence()
            .AppendCallback(() =>
            {
                if (curLineItem != null)
                    curLineItem.EndLine();
                foreach (var sr in cabNameBG)
                {
                    sr.DOColor(curItem.color, .5f);
                }
            })
            .AppendInterval(.5f)
            .AppendCallback(() =>
            {
                cabNameTxt.text = curItem.title;
            }).AppendInterval(.25f)
            .AppendCallback(() =>
            {
                var templateToUse = TypeToLineItem(curItem.type);
                curLineItem = Instantiate(templateToUse, transform);
                if(curItem.type == TickerItem.Type.CABINET)
                    (curLineItem as TickerLineGameStatus).Init(curItem.cabState);
                else if(curItem.type == TickerItem.Type.NEXT_UP)
                    (curLineItem as TickerLineNextUp).Init(curItem.cabState);
                else //message
                    (curLineItem as TickerLineNextUp).Init(curItem.message);
                curLineItem.StartLine();
            });
        

        
    }

    TickerLineItem TypeToLineItem(TickerItem.Type type)
    {
        switch(type)
        {
            case TickerItem.Type.CABINET: return lineItemTemplate;
            case TickerItem.Type.NEXT_UP: return nextUpTemplate;
            case TickerItem.Type.MESSAGE: default: return nextUpTemplate;
        }
    }
    static void OnHivemindMessage(string message)
    {
        var jsonStepOne = JsonUtility.FromJson<HMTypeCheck>(message);
        int id = jsonStepOne.cabinet_id;
        TickerCabinetState targetCab = null;
        foreach(var item in tickerItems)
        {
            if (item.cabState == null)
                continue;

            var cab = item.cabState;
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


    public void OnReceivedNewsEntries()
    {
        int txtSheetIndex = googleSheetsDB.sheetTabNames.IndexOf("ticker");

        txtSheet = googleSheetsDB.dataSheets[txtSheetIndex];
        for(int i = 0; i < txtSheet.AvailableRows.Count; i++)
        {
            txtSheet.CurrentRow = txtSheet.GetRowID(i);
            var title = txtSheet.GetString("Title");

            if (title == "") continue;

            var duration = txtSheet.GetFloat("Duration");
            var message = txtSheet.GetString("Message");
            var color = txtSheet.GetString("Title Color");
            var item = new TickerItem();
            item.title = title;
            item.message = message;
            item.duration = duration;
            item.color = color == "" ? defaultColor : Util.HexToColor(color);
            item.type = TickerItem.Type.MESSAGE;
            tickerItems.Add(item);
        }
    }
}

