using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class GameModel : MonoBehaviour
{
    
    public class Gate
    {
        public Vector2Int position;
        public int currentTeam = -1;
        public int blueTime = 0;
        public int goldTime = 0;
        public DateTime lastTagTime;
        public string type = "unknown";
        public void TagGate(int teamID)
        {
            var dt = (DateTime.Now - lastTagTime).Milliseconds;
            if (currentTeam == 0) blueTime += dt;
            else goldTime += dt;
            currentTeam = teamID;
            lastTagTime = DateTime.Now;
        }
        public void EndGame()
        {
            var dt = (DateTime.Now - lastTagTime).Milliseconds;
            if (currentTeam == 0) blueTime += dt;
            else goldTime += dt;
        }
        public Gate(Vector2Int coords, int initTeam)
        {
            position = coords;
            lastTagTime = DateTime.Now;
            currentTeam = initTeam;
        }
        
    }

    public static List<int> GetGateControlPercetages()
    {
        List<int> ret = new List<int>(){ 0, 0};
        foreach(var g in instance.allGates)
        {
            ret[0] += g.blueTime;
            ret[1] += g.goldTime;
        }
        if (ret[0] + ret[1] <= 0) ret[0] = 50;
        else ret[0] = Mathf.FloorToInt(100f * ((float)ret[0] / (float)(ret[0] + ret[1])));

        ret[1] = 100 - ret[0];
        return ret;
    }
    public static GameModel instance;

    public List<Gate> allGates = new List<Gate>();

    public static UnityEvent<int, string> onGameModelComplete = new UnityEvent<int,string>();
    public TeamModel[] teams = new TeamModel[2];
    public static bool setScoresEnabled { get { return instance.setPoints.property > 0; } }
    public LSProperty<int> setPoints = new LSProperty<int>(0);
    public static bool inTournamentMode = false;

    //current game state
    public LSProperty<int> berriesLeft = new LSProperty<int>(0);
    public LSProperty<float> famineTimer = new LSProperty<float>(0f);
    public LSProperty<int> snailPosition = new LSProperty<int>(0);
    public LSProperty<float> gameTime = new LSProperty<float>(0f);
    public LSProperty<bool> gameIsRunning = new LSProperty<bool>(false);

    //tournament game state
    public LSProperty<bool> isWarmup = new LSProperty<bool>(false);
    public static float newSetTimeout = -1f;

    public static GameEvent onGameEvent = new GameEvent();
    public static UnityEvent onGameStart = new UnityEvent();
    public static UnityEvent onGamePointForcedUpdate = new UnityEvent();
    public static UnityEvent<HMMatchState> onDelayedTournamentData = new UnityEvent<HMMatchState>();
    public static int currentTournamentID = 0;

    public static HMMatchState newSetTeamData = null;
    private void Awake()
    {
        instance = this;
        for(int i = 0; i < 2; i++)
        {
            teams[i] = new TeamModel(i);
        }

        //debug
        isWarmup.onChange.AddListener((before, after) =>
        {
            Debug.Log("isWarmup " + after);
        });
    }

    private void Start()
    {
        NetworkManager.instance.gameEventDispatcher.AddListener(OnGameEvent);
        berriesLeft.onChange.AddListener(CheckForFamine);
        if (!ViewModel.instance.appView)
            Application.targetFrameRate = 60;
        //Discord.Send("<@&1027310722872512643> is now playing on Cab 1! (Not really, just testing stuff :slight_smile: )");
    }

    void CheckForFamine(int before, int after)
    {
        if(after == 0 && gameIsRunning.property)
        {
            famineTimer.property = 90f;
        }
    }
    void ResetGame()
    {
        foreach (var t in teams)
            t.ResetGame();
        SnailModel.ResetModel();
        allGates = new List<Gate>();
        gameTime.property = 0f;
    }

    public void ResetSet()
    {
        foreach (var t in teams)
            t.ResetSet();
        SnailModel.ResetModel();
        allGates = new List<Gate>();
        gameTime.property = 0f;
    }

    void NewSetAfterDelay()
    {
        ResetSet();
        newSetTimeout = -1f;
        if (newSetTeamData != null)
        {
            //set team data
            Debug.Log("resetting set with stored team data");
            GameModel.instance.setPoints.property = newSetTeamData.current_match.rounds_per_match != 0 ? newSetTeamData.current_match.rounds_per_match : newSetTeamData.current_match.wins_per_match;
            GameModel.instance.teams[0].teamName.property = newSetTeamData.current_match.blue_team;
            GameModel.instance.teams[0].setWins.property = newSetTeamData.current_match.blue_score;
            GameModel.instance.teams[1].teamName.property = newSetTeamData.current_match.gold_team;
            GameModel.instance.teams[1].setWins.property = newSetTeamData.current_match.gold_score;
            GameModel.instance.isWarmup.property = newSetTeamData.current_match.is_warmup;
            onDelayedTournamentData.Invoke(newSetTeamData);
            newSetTeamData = null;
            inTournamentMode = true;
            //NetworkManager.GetSignedInPlayers();
        }
        else
        {
            //tournament mode over
            Debug.Log("Tournament ended");
            var tempMatchData = new HMMatchState { blue_score = 0, gold_score = 0, current_match = new HMCurrentMatch { blue_score = 0, gold_score = 0, gold_team = "Gold Team", blue_team = "Blue Team", is_warmup = false, rounds_per_match = 0, wins_per_match = 0, id = 0} };
            GameModel.instance.setPoints.property = 0;
            GameModel.instance.teams[0].teamName.property = "Blue Team";
            GameModel.instance.teams[0].setWins.property = 0;
            GameModel.instance.teams[1].teamName.property = "Gold Team";
            GameModel.instance.teams[1].setWins.property = 0;
            GameModel.instance.isWarmup.property = false;
            onDelayedTournamentData.Invoke(tempMatchData);
            TournamentPresetData.ClearPresetData();
            inTournamentMode = false;
        }
    }
    void OnGameEvent(string eType ,GameEventData data)
    {

        switch(eType)
        {
            case GameEventType.GAME_START:
                Debug.Log("inverted " + data.teamPositionsInverted);
                MapDB.SetMap(data.mapName);
                GameModel.instance.berriesLeft.property = MapDB.currentMap.property.total_berries;
                instance.famineTimer.property = 0f;
                UIState.inverted = data.teamPositionsInverted;
                ResetGame();
                gameIsRunning.property = true;
                break;
            case GameEventType.SPAWN:
                if(data.playerID == 2 && data.teamID == 1) //gold queen spawn is the first trigger at game start
                {
                    if (newSetTimeout > 0f)
                    {
                        NewSetAfterDelay();   
                    }
                    else
                    {
                        ResetGame();
                    }
                    //NetworkManager.GetSignedInPlayers();
                    NetworkManager.GetTournamentState();
                    gameTime.property = 0f;
                    onGameStart.Invoke();
                }
                break;
            case GameEventType.BERRY_SCORE:
                teams[data.teamID].players[data.playerID].curLifeStats.berriesDeposited.property++;
                int sp = 5 * (int)GetObjPressureRating(data.teamID, ObjType.Berries);
                teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.Berries, 5 * (int)GetObjPressureRating(data.teamID, ObjType.Berries), 1);
                teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.BerriesCombined, 0, 1);
                berriesLeft.property--;
                break;
            case GameEventType.BERRY_KICK:
                if (data.teamID != data.targetID) //wrong goal!
                {
                    teams[data.teamID].players[data.playerID].curLifeStats.berriesKicked_OtherTeam.property++;
                    teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.BerryKicks, 0, 0, 1);
                    teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.BerriesCombined, 0, 0, 1);
                }
                else
                {
                    teams[data.teamID].players[data.playerID].curLifeStats.berriesKicked.property++;
                    teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.BerryKicks, (int)GetObjPressureRating(data.teamID, ObjType.Berries) * 50, 1);
                    teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.BerriesCombined, 0, 1);
                }
                berriesLeft.property--;
                break;
            case GameEventType.BERRY_GRAB:
                teams[data.teamID].players[data.playerID].curLifeStats.berriesGrabbed.property++;
                teams[data.teamID].players[data.playerID].isHoldingBerry = true;
                break;
            case GameEventType.BOUNCE:
                teams[data.teamID].players[data.playerID].AddBump(data.targetID);
                teams[1 - data.teamID].players[data.targetID].AddBump(data.playerID);
                break;
            case GameEventType.PLAYER_KILL:
                teams[data.teamID].players[data.playerID].curLifeStats.kills.property++;
                int killValue = 0;
                if(data.targetType == EventTargetType.WARRIOR)
                {
                    killValue = teams[1 - data.teamID].players[data.targetID].curLifeStats.speedObtained.property > 0 ? 250 : 100;
                } else if(data.targetType == EventTargetType.QUEEN)
                {
                    killValue = (int)GetObjPressureRating(1 - data.teamID, ObjType.Queen) * 100;
                }
                if (data.targetType != EventTargetType.DRONE)
                {
                    teams[data.teamID].players[data.playerID].curLifeStats.militaryKills.property++;
                    teams[1 - data.teamID].players[data.targetID].curLifeStats.militaryDeaths.property++;
                    teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.KD, killValue, 1);
                    teams[1 - data.teamID].players[data.targetID].AddDerivedStat(PlayerModel.StatValueType.KD, 0, 0, 1);
                    //award bump assist if one exists
                    var bumperList = teams[1 - data.teamID].players[data.targetID].GetRecentBumps(2000);
                    foreach(var bumper in bumperList)
                    {
                        teams[data.teamID].players[bumper.bumperID].curLifeStats.bumpAssists.property++;
                        teams[data.teamID].players[bumper.bumperID].AddDerivedStat(
                            teams[data.teamID].players[bumper.bumperID].curLifeStats.swordObtained.property > 0
                            ? PlayerModel.StatValueType.Pinces
                            : PlayerModel.StatValueType.BumpAssists
                        , killValue / 2, 1);
                    }
                }
                else
                {
                    //always award drone kill and death
                    teams[data.teamID].players[data.playerID].curLifeStats.droneKills.property++;
                    teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.DroneKills, 10, 1);
                    teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.DroneKD, 0, 1, 0);
                    teams[1 - data.teamID].players[data.targetID].AddDerivedStat(PlayerModel.StatValueType.DroneKD, 0, 0, 1);

                    //try to determine if the drone kill was noteworthy (stopped a form or objective)
                    //todo - need to consider wraps - make duplicate gates and snail for ones close to the edge
                    bool hasBerry = teams[1 - data.teamID].players[data.targetID].isHoldingBerry;
                    var nearestGate = MapDB.currentMap.property.GetNearestFeature(data.coordinates, MapFeature.Type.SwordGate);
                    var nearestHive = MapDB.currentMap.property.GetNearestFeature(data.coordinates, data.teamID == 0 ? MapFeature.Type.BlueHive : MapFeature.Type.GoldHive);
                    float snailDist = Vector2Int.Distance(data.coordinates, new Vector2Int(Mathf.FloorToInt(SnailModel.currentPosition.property), MapDB.currentMap.property.snail_Y));
                    bool maybeForm = hasBerry && nearestGate.Item2 <= 150f;
                    bool maybeHive = hasBerry && nearestHive.Item2 <= 300f;
                    bool maybeSnail = snailDist <= 200f;
                    if(maybeForm && (maybeHive || maybeSnail))
                    {
                        //if we think the player is obj, eliminate forms as a possibility
                        if (teams[1 - data.teamID].players[data.targetID].isLikelyObjective)
                            maybeForm = false;
                    }
                    if (maybeForm && maybeHive)
                    {
                        
                        //scale hive value by how good enemy berries are doing (smaller is better)
                        var adjustedHive = nearestHive.Item2 * Util.AdjustRange(12 - teams[1 - data.teamID].GetBerryScore(), new Vector2(0, 12), new Vector2(.5f, 1.5f));
                        maybeHive = adjustedHive < nearestGate.Item2;
                        maybeForm = !maybeHive;
                    }
                    if (maybeForm && maybeSnail)
                    {
                        //scale snail distance by how good snail is doing (smaller is better)
                        snailDist *= Util.AdjustRange(100 - Mathf.Max(SnailModel.bluePercentage.property, SnailModel.goldPercentage.property), new Vector2(0f, 100f), new Vector2(.5f, 1.5f));
                        maybeSnail = snailDist < nearestGate.Item2;
                        maybeForm = !maybeSnail;
                    }
                    if (maybeForm) //probably trying to form
                    {
                        teams[data.teamID].players[data.playerID].curLifeStats.formGuards.property++;
                        teams[1 - data.teamID].players[data.targetID].curLifeStats.formFails.property++;
                        teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.FormGuards,
                            teams[1 - data.teamID].players[data.targetID].curLifeStats.speedObtained.property > 0 ? 250 : 100, 1);
                    }
                    else if (maybeHive) //probably trying to berry
                    {
                        teams[data.teamID].players[data.playerID].curLifeStats.ledgeGuards.property++;
                        teams[1 - data.teamID].players[data.targetID].curLifeStats.berryFails.property++;
                        teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.ObjGuards, (int)GetObjPressureRating(1 - data.teamID, ObjType.Berries) * 5, 1);
                        teams[1 - data.teamID].players[data.targetID].AddDerivedStat(PlayerModel.StatValueType.Berries, 0, 0, 1);
                    }
                    else if (maybeSnail)
                    {
                        teams[data.teamID].players[data.playerID].curLifeStats.snailGuards.property++;
                        teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.ObjGuards, (int)GetObjPressureRating(1 - data.teamID, ObjType.Snail) * 5, 1);
                    }
                }
                if (data.targetType == EventTargetType.QUEEN)
                    teams[data.teamID].players[data.playerID].curLifeStats.queenKills.property++;

                if(teams[1 - data.teamID].players[data.targetID].hivemindID == 166) //jason
                {
                    teams[data.teamID].players[data.playerID].jasonPoints += 1;
                }
                teams[1 - data.teamID].players[data.targetID].curLifeStats.deaths.property++;
                teams[1 - data.teamID].players[data.targetID].ResetLife();
                break;
            case GameEventType.GATE_USE_END:
                if (data.targetType == EventTargetType.GATE_SWORD)
                {
                    teams[data.teamID].players[data.playerID].curLifeStats.swordObtained.property++;
                    teams[data.teamID].players[data.playerID].formTime = System.DateTime.Now;
                }
                if (data.targetType == EventTargetType.GATE_SPEED)
                    teams[data.teamID].players[data.playerID].curLifeStats.speedObtained.property++;
                berriesLeft.property--;
                break;
            case GameEventType.SNAIL_START:
                SnailModel.OnSnailStart(data.coordinates.x, data.teamID, data.playerID, teams[data.teamID].players[data.playerID].curLifeStats.speedObtained > 0);
                break;
            case GameEventType.SNAIL_EAT:
                SnailModel.OnSnailEat(data.coordinates.x, data.targetID);
                break;
            case GameEventType.SNAIL_END:
                SnailModel.OnSnailEnd(data.coordinates.x);
                break;
            case GameEventType.GAME_END_DETAIL:

                //if in tournament mode, let HM do the score keeping
                if (!isWarmup.property && !inTournamentMode)
                    teams[data.teamID].setWins.property++;

                SnailModel.instance.OnGameEnd(data.targetType == "snail");
                foreach (var g in allGates)
                    g.EndGame();
                var gatePercents = GetGateControlPercetages();
                //todo - add style points for gate control?
                teams[0].players[2].AddDerivedStat(PlayerModel.StatValueType.Gates, 0, gatePercents[0]);
                teams[1].players[2].AddDerivedStat(PlayerModel.StatValueType.Gates, 0, gatePercents[1]);
                foreach (var team in teams)
                    team.EndGame();
                
                gameIsRunning.property = false;
                //new LSTimer(10f, () => NetworkManager.GetSignedInPlayers());
                break;
            case GameEventType.GATE_TAG:
                var gate = allGates.Find(x => x.position == data.coordinates);
                if(gate == null)
                {
                    gate = new Gate(data.coordinates, data.teamID);
                    allGates.Add(gate);
                }
                else
                {
                    gate.TagGate(data.teamID);
                }
                break;
        }
        //re-propagate the event now that the game model is updated
        onGameEvent.Invoke(eType, data);
        if (eType == GameEventType.GAME_END_DETAIL)
        {
            onGameModelComplete.Invoke(data.teamID, data.targetType);
            isWarmup.property = false;
        }
    }

    public enum ObjType { Berries, Snail, Queen };
    public float GetObjPressureRating(int teamID, ObjType type)
    {
        switch(type)
        {
            case ObjType.Queen:
                int qDeaths = teams[teamID].players[2].curGameStats.deaths.property;
                return qDeaths == 0 ? 3f : qDeaths == 1 ? 6f : 10f;
            case ObjType.Berries:
                return Mathf.Min(10f, teams[teamID].GetBerryScore() + 1f);
            case ObjType.Snail:
                return 100f / (110f - Mathf.Max((float)SnailModel.bluePercentage, (float)SnailModel.goldPercentage));
            default: return 0f;
        }
    }
    private void Update()
    {
        if (gameIsRunning.property)
        {
            gameTime.property += Time.deltaTime;
            for (int i = 0; i < 2; i++)
            {
                foreach (var p in teams[i].players)
                {
                    p.fireGauge -= Time.deltaTime * 10f;
                }
            }
        } else if(newSetTimeout > 0f)
        {
            newSetTimeout -= Time.deltaTime;
            if (newSetTimeout <= 0f)
                NewSetAfterDelay();
        }
        if(famineTimer.property > 0f)
        {
            famineTimer.property = Mathf.Max(0f, famineTimer.property - Time.deltaTime);
            if(famineTimer == 0f)
            {
                berriesLeft.property = MapDB.currentMap.property.total_berries;
            }
        }
    }

    public static PlayerModel GetPlayer(int t, int p)
    {
        return instance.teams[t].players[p];
    }

    public PlayerModel[] GetBestPlayers(int num)
    {
        var allPlayers = new List<PlayerModel>();
        foreach(var t in teams)
        {
            foreach(var p in t.players)
            allPlayers.Add(p);
        }
        allPlayers.Sort();
        if (allPlayers.Count > num)
            return allPlayers.GetRange(0, num).ToArray();
        return allPlayers.ToArray();
    }
}
