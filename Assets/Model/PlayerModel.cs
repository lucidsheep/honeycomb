using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;



public class PlayerModel : IComparable
{
    public int CompareTo(object obj)
    {
        if (obj is PlayerModel)
            return this.gameStylePoints.CompareTo(((PlayerModel)obj).gameStylePoints);
        return 0;
    }
    public static StatValueType[] GetStatList()
    {
        return new StatValueType[] { StatValueType.KD, StatValueType.Berries, StatValueType.Snail, StatValueType.Gates, StatValueType.LongestLife, StatValueType.QueenKills, StatValueType.Pinces, StatValueType.ObjGuards, StatValueType.FormGuards, StatValueType.UpTime, StatValueType.BerryKicks, StatValueType.BumpAssists, StatValueType.SnailKills, StatValueType.SnailFeeds, StatValueType.Value, StatValueType.BerriesCombined, StatValueType.DroneKills, StatValueType.KDA, StatValueType.DroneKD };
    }

    public enum StatValueType { KD, Berries, Snail, Gates, LongestLife, QueenKills, Pinces, ObjGuards, FormGuards, UpTime, BerryKicks, BumpAssists, SnailKills, SnailFeeds, Value, BerriesCombined, DroneKills, KDA, DroneKD }
    public struct StatValue : System.IComparable
    {
        public int stylePoints;
        public int num1;
        public int num2;
        public StatValueType type;
        public string value;

        public int CompareTo(object obj)
        {
            if (obj is StatValue)
            {
                var ct = this.stylePoints.CompareTo(((StatValue)obj).stylePoints);
                return ct == 1 ? -1 : ct == -1 ? 1 : 0;
            }
            return 0;
        }
        public static StatValue operator +(StatValue lhs, StatValue rhs)
        {
            return new StatValue { num1 = lhs.num1 + rhs.num1, num2 = lhs.num2 + rhs.num2, stylePoints = lhs.stylePoints + rhs.stylePoints, type = lhs.type };
        }

        public static StatValue operator -(StatValue lhs, StatValue rhs)
        {
            return new StatValue { num1 = lhs.num1 - rhs.num1, num2 = lhs.num2 - rhs.num2, stylePoints = lhs.stylePoints - rhs.stylePoints, type = lhs.type };
        }
        public string label
        {
            get
            {
                bool plural = num1 != 1;
                switch (type)
                {
                    case StatValueType.KD: case StatValueType.KDA:  return "Military Kill" + (plural ? "s" : "");
                    case StatValueType.Berries: return "Berr" + (plural ? "ies" : "y") + " Run";
                    case StatValueType.BerryKicks: return "Kick-in" + (plural ? "s" : "");
                    case StatValueType.BumpAssists: return "Bump Assist" + (plural ? "s" : "");
                    case StatValueType.Snail: return "Snail Length" + (plural ? "s" : "");
                    case StatValueType.FormGuards: return "Gate Guard" + (plural ? "s" : "");
                    case StatValueType.Gates: return "Gate Control";
                    case StatValueType.LongestLife: return "Longest Life";
                    case StatValueType.ObjGuards: return "Obj. Guard" + (plural ? "s" : "");
                    case StatValueType.Pinces: return "Pince" + (plural ? "s" : "");
                    case StatValueType.QueenKills: return "";
                    case StatValueType.SnailKills: return "Snail Kill" + (plural ? "s" : "");
                    case StatValueType.UpTime: return "Mil. Uptime";
                    case StatValueType.SnailFeeds: return "Snail Feed" + (plural ? "s" : "");
                    case StatValueType.Value: return "Value Score™";
                    case StatValueType.DroneKills: case StatValueType.DroneKD: return "Drone Kill" + (plural ? "s" : "");
                    default: return "???";
                }
            }
        }
        public bool isMilitary { get {
                return type == StatValueType.KD || type == StatValueType.BerryKicks || type == StatValueType.FormGuards || type == StatValueType.Gates
|| type == StatValueType.LongestLife || type == StatValueType.ObjGuards || type == StatValueType.Pinces || type == StatValueType.QueenKills || type == StatValueType.UpTime || type == StatValueType.DroneKills; } }
        public bool isObjective { get { return !isMilitary; } }
        public string singleNumber
        { get
            {
                switch (type) {
                    case StatValueType.Snail: return Mathf.FloorToInt(num1 / SnailModel.SNAIL_METER).ToString();
                    case StatValueType.Gates: return num1 + "%";
                    case StatValueType.LongestLife: return Util.FormatTime(num1);
                    case StatValueType.UpTime: return num1 + "%";
                    default: return num1.ToString();
                }
            } }
        public string fullNumber
        {
            get
            {
                switch (type)
                {
                    case StatValueType.KD: return num1 + "-" + num2;
                    case StatValueType.Berries: return num1 + "/" + (num1 + num2);
                    case StatValueType.Snail: return Mathf.FloorToInt(num1 / SnailModel.SNAIL_METER) + "m";
                    case StatValueType.Gates: return num1 + "%";
                    case StatValueType.LongestLife: return Util.FormatTime(num1);
                    case StatValueType.UpTime: return num1 + "%";
                    case StatValueType.BerryKicks: return num1 + (num2 > 0 ? "(" + num2 + ")" : "");
                    case StatValueType.BerriesCombined: return num1 + (num2 > 0 ? "(" + num2 + ")" : "");
                    case StatValueType.DroneKD: return num1 + "|" + num2;
                    default: return num1.ToString();
                }
            }
        }
        public override string ToString()
        {
            return label + ":" + fullNumber + " sp:" + stylePoints;
        }
    }

    public string GetKDA()
    {
        //need special function since it combines two different statValues
        var assists = (curGameDerivedStats[StatValueType.BumpAssists].num1 + curGameDerivedStats[StatValueType.Pinces].num1);

        if (assists <= 0) return curGameDerivedStats[StatValueType.KD].fullNumber;

        return curGameDerivedStats[StatValueType.KD].fullNumber + "-" + assists;
    }
    static string dashIfZero(int num) { return num == 0 ? "-" : num.ToString(); }
    public struct CombinedStylePoints : System.IComparable
    {
        public int totalMilitaryPoints;
        public int totalObjectivePoints;
        public int valuePoints;
        public int totalPoints { get { return totalMilitaryPoints + totalObjectivePoints + valuePoints; } }
        public int CompareTo(object obj)
        {
            if (obj is CombinedStylePoints)
            {
                int ct = 0;
                //uncomment to use only kquity points
                //if(KQuityManager.instance != null && KQuityManager.instance.useManager && KQuityManager.isConnected)
                //    ct = this.valuePoints.CompareTo(((CombinedStylePoints)obj).valuePoints);
                //else
                ct = this.totalPoints.CompareTo(((CombinedStylePoints)obj).totalPoints);
                return ct == 1 ? -1 : ct == -1 ? 1 : 0;
            }
            return 0;
        }

    }
    //static data
    public LSProperty<string> playerName = new LSProperty<string>("");
    public string positionName { get { return (teamID == 0 ? "Blue" : "Gold") + " " + defaultName; } }
    public string displayName { get { return playerName == "" ? positionName : playerName; } }
    public string displayNameWithoutTeam { get { return playerName == "" ? defaultName : playerName; } }
    public string defaultName;
    public int teamID;
    public int positionID;
    public int hivemindID = -1;
    public Sprite hat;

    //set stats
    public GameDataStructure curSetStats = new GameDataStructure();

    //accumulated stats
    public GameDataStructure curGameStats = new GameDataStructure();

    //current life stats
    public GameDataStructure curLifeStats = new GameDataStructure();

    //all stats since the overlay turned on, only count logged in users
    public GameDataStructure curDayStats = new GameDataStructure();

    //player career stats, counted when signed in to HM
    public GameDataStructure curCareerStats = new GameDataStructure();

    public struct BumpData
    {
        public DateTime bumpTime;
        public int bumperID;
    }
    public DateTime formTime;
    public List<BumpData> bumpLog;
    public bool isHoldingBerry;
    public int militarySeconds;
    public int warriorSeconds;
    public int jasonPoints; //the most important stat

    public LSProperty<bool> isOnFire = new LSProperty<bool>(false);
    float _fire = 0f;
    public float fireGauge { get { return _fire; } set
        {
            bool onFireNow = isOnFire.property;
            _fire = Mathf.Max(0f, Mathf.Min(500f, value));
            if (!onFireNow && _fire >= 300f)
            {
                isOnFire.property = true;
            }
            else if (onFireNow && _fire <= 0f)
            {
                isOnFire.property = false;
            }
        } }

    //milestones for calling out achievements
    public enum StatTimescale { Life, Game, Set, Tournament, Career }
    public struct Milestone
    {
        public StatTimescale timescale;
        public StatValueType statType;
        public string description;
        public int milestoneValue;

    }
    public DateTime lastMilestoneUsed = DateTime.MinValue;
    public Milestone bestUnusedMilestone;

    public Dictionary<StatValueType, StatValue> curGameDerivedStats = new Dictionary<StatValueType, StatValue>();
    public Dictionary<StatValueType, StatValue> curSetDerivedStats = new Dictionary<StatValueType, StatValue>();

    public UnityEvent<Milestone> onMilestoneAchieved = new UnityEvent<Milestone>();

    public void ResetLife(bool endOfGame = false)
    {
        if (curLifeStats.swordObtained.property > 0)
        {
            curLifeStats.longestLife.property = (int)(DateTime.Now - formTime).TotalMilliseconds;
            militarySeconds += (int)((DateTime.Now - formTime).TotalSeconds);
            if(positionID != 2)
                warriorSeconds += (int)((DateTime.Now - formTime).TotalSeconds); ;
        }
        curLifeStats.Reset();
        bumpLog = new List<BumpData>();

        isHoldingBerry = false;
        if (positionID == 2 && !endOfGame) //start mil life immediately if it's a queen
        {
            formTime = DateTime.Now;
            curLifeStats.swordObtained.property = 1;
        }
        if (endOfGame)
        {
            if (GameModel.instance.gameTime.property > 0f) //avoid / 0
                AddDerivedStat(StatValueType.UpTime, 0, Mathf.FloorToInt(((float)militarySeconds / GameModel.instance.gameTime.property) * 100f));
        }

        MaybeResetMilestone(StatTimescale.Life);
    }

    public void AddBump(int bumperID)
    {
        bumpLog.Add(new BumpData { bumperID = bumperID, bumpTime = DateTime.Now });

        //Debug.Log("ALL BUMPS: my id " + positionID);
        //foreach (var d in bumpLog)
        //    Debug.Log("  [" + (int)(DateTime.Now - d.bumpTime).TotalMilliseconds + "]" + d.bumperID);
    }
    public List<BumpData> GetRecentBumps(int msThreshold)
    {
        List<BumpData> data = bumpLog.FindAll(x => (int)(DateTime.Now - x.bumpTime).TotalMilliseconds <= msThreshold);
        return data;
    }
    public void ResetGame()
    {
        ResetLife();
        curGameStats.Reset();
        militarySeconds = 0;
        warriorSeconds = 0;
        fireGauge = 0f;
        jasonPoints = 0;
        foreach (var val in curGameDerivedStats)
        {
            curSetDerivedStats[val.Key] += val.Value;
        }
        foreach (var thisType in GetStatList())
            curGameDerivedStats[thisType] = new StatValue { type = thisType };
        MaybeResetMilestone(StatTimescale.Game);
    }

    public void ResetSet()
    {
        ResetGame();
        curSetStats.Reset();
        foreach (var val in GetStatList())
        {
            curSetDerivedStats[val] = new StatValue { type = val };
        }
        MaybeResetMilestone(StatTimescale.Set);
    }
    public void ResetAll()
    {
        ResetSet();
        playerName.property = "";
        hivemindID = -1;
        MaybeResetMilestone(StatTimescale.Career);
    }

    void MaybeResetMilestone(StatTimescale timescale)
    {
        if((int)timescale >= (int)bestUnusedMilestone.timescale)
        {
            bestUnusedMilestone = default(Milestone);
        }
    }

    public void OnPlayerSignIn(PlayerStaticData.PlayerData data)
    {
        if(data.dayStatistics != null)
        {
            curDayStats = data.dayStatistics;
            curDayStats.berriesDeposited.onChange.AddListener((b, a) => { CheckMilestone(StatValueType.Berries, StatTimescale.Tournament, b, a); });
            curDayStats.snailMoved.onChange.AddListener((b, a) => { CheckMilestone(StatValueType.Snail, StatTimescale.Tournament, b, a); });
            curDayStats.militaryKills.onChange.AddListener((b, a) => { CheckMilestone(StatValueType.KD, StatTimescale.Tournament, b, a); });
            curDayStats.queenKills.onChange.AddListener((b, a) => { CheckMilestone(StatValueType.QueenKills, StatTimescale.Tournament, b, a); });
        }
        if(data.careerStatistics != null)
        {
            curCareerStats = data.careerStatistics;
            curCareerStats.berriesDeposited.onChange.AddListener((b, a) => { CheckMilestone(StatValueType.Berries, StatTimescale.Career, b, a); });
            curCareerStats.snailMoved.onChange.AddListener((b, a) => { CheckMilestone(StatValueType.Snail, StatTimescale.Career, b, a); });
            curCareerStats.militaryKills.onChange.AddListener((b, a) => { CheckMilestone(StatValueType.KD, StatTimescale.Career, b, a); });
            curCareerStats.queenKills.onChange.AddListener((b, a) => { CheckMilestone(StatValueType.QueenKills, StatTimescale.Career, b, a); });
            Debug.Log(data.hivemindID + " career stats: qkills/kills: " + curCareerStats.queenKills + "/" + curCareerStats.militaryKills + " berries " + curCareerStats.berriesDeposited + " snail " + curCareerStats.snailMoved);
        }
        bestUnusedMilestone = default(Milestone);

        hat = null;

        var playerIDHat = SpriteDB.hatDatabase.Find(x => x.hivemindID == data.hivemindID);
        if (playerIDHat != null)
            hat = playerIDHat.hat;
        else if (data.hatEmoji != "")
            hat = SpriteDB.hatDatabase.Find(x => x.validEmoji.Contains(data.hatEmoji)).hat;
        else
        {
            var texEnum = System.Globalization.StringInfo.GetTextElementEnumerator(data.name);
            while(texEnum.MoveNext())
            {
                var elem = texEnum.GetTextElement();
                playerIDHat = SpriteDB.hatDatabase.Find(x => x.validEmoji.Contains(elem));
                if (playerIDHat != null)
                {
                    //remove special hat emoji from name
                    data.hatEmoji = elem;
                    break;
                }
            }
            if (playerIDHat != null)
                hat = playerIDHat.hat;
        }
        if (hat != null)
            Debug.Log("hat found");
    }

    public void OnPlayerSignOut()
    {
        if (curDayStats != null)
        {
            curDayStats.berriesDeposited.onChange.RemoveAllListeners();
            curDayStats.snailMoved.onChange.RemoveAllListeners();
            curDayStats.militaryKills.onChange.RemoveAllListeners();
            curDayStats.queenKills.onChange.RemoveAllListeners();
            curDayStats = null;
        }
        if (curCareerStats != null)
        {
            curCareerStats.berriesDeposited.onChange.RemoveAllListeners();
            curCareerStats.snailMoved.onChange.RemoveAllListeners();
            curCareerStats.militaryKills.onChange.RemoveAllListeners();
            curCareerStats.queenKills.onChange.RemoveAllListeners();
            curCareerStats = null;
        }
        bestUnusedMilestone = default(Milestone);
        hat = null;
    }

    void MaybeSyncCareerStats(StatValueType type, int b, int a)
    {
        if (curDayStats == null) return;
        if (b >= a) return;
        int delta = a - b;
        switch(type)
        {
            case StatValueType.KD:
                curDayStats.militaryKills.property += delta;
                if (curCareerStats != null) curCareerStats.militaryKills.property += delta;
                break;
            case StatValueType.Berries: curDayStats.berriesDeposited.property += delta;
                if (curCareerStats != null) curCareerStats.berriesDeposited.property += delta;
                break;
            case StatValueType.Snail: curDayStats.snailMoved.property += delta;
                if (curCareerStats != null) curCareerStats.snailMoved.property += delta;
                break;
            case StatValueType.QueenKills: curDayStats.queenKills.property += delta;
                if (curCareerStats != null) curCareerStats.queenKills.property += delta;
                break;
            default: break;
        }
    }
    public void AddDerivedStat(StatValueType type, int stylePoints, int num1, int num2 = 0)
    {
        curGameDerivedStats[type] += new StatValue { num1 = num1, num2 = num2, stylePoints = stylePoints, type = type };
        fireGauge += stylePoints;
    }
    public void SyncPlayerStat(LSProperty<int> source, params LSProperty<int>[] dests)
    {
        foreach(var dest in dests)
            source.onChange.AddListener((b, a) => { if (a > b) dest.property += a - b; });
    }

    public void SyncPlayerTopStat(LSProperty<int> source, params LSProperty<int>[] dests)
    {
        foreach (var dest in dests)
            source.onChange.AddListener((b, a) => { if (a > dest.property) dest.property = a; });
    }

    public void SyncPlayerMilestone(LSProperty<int> lifeSource, LSProperty<int> gameSource, LSProperty<int> setSource, LSProperty<int> tournamentSource, StatValueType statType)
    {
        lifeSource.onChange.AddListener((b, a) => { CheckMilestone(statType, StatTimescale.Life, b, a); });
        gameSource.onChange.AddListener((b, a) => { CheckMilestone(statType, StatTimescale.Game, b, a); });
        setSource.onChange.AddListener((b, a) => { CheckMilestone(statType, StatTimescale.Set, b, a); });
    }

    void CheckMilestone(StatValueType statType, StatTimescale timescale, int before, int after)
    {
        if (timescale == StatTimescale.Set && !GameModel.setScoresEnabled)
            return; //ignore set milestones if we are not in a set

        int value = 1, mult = 1;
        string desc = "";
        switch(statType)
        {
            case StatValueType.KD:
                mult = timescale == StatTimescale.Life ? 3 : timescale == StatTimescale.Career ? 100 : 5;
                value = 2;
                desc = "Kills";
                break;
            case StatValueType.Berries:
                mult = timescale == StatTimescale.Life ? 6 : timescale == StatTimescale.Career ? 250 : 10;
                value = 1;
                desc = "Berries Run";
                break;
            case StatValueType.QueenKills:
                mult = timescale == StatTimescale.Career ? 100 : 5;
                value = 5;
                desc = "Queens Slain";
                break;
            case StatValueType.Snail:
                mult = timescale == StatTimescale.Life ? 50 : timescale == StatTimescale.Career ? 1000 : 100;
                mult = Mathf.FloorToInt(mult * SnailModel.SNAIL_METER); //snail meters
                value = 1;
                desc = "Snail Meters";
                break;
            default: break;
        }

        int threshold = ((before / mult) + 1) * mult;

        if (statType == StatValueType.Snail)
            desc = Mathf.RoundToInt(threshold / SnailModel.SNAIL_METER) + " " + desc;
        else
            desc = threshold + " " + desc;

        value *=
            timescale == StatTimescale.Life ? 2 :
            timescale == StatTimescale.Game ? 1 :
            timescale == StatTimescale.Set ? 3 :
            timescale == StatTimescale.Tournament ? 5 :
            10; //career
        value *= (threshold / mult);
        if(before < threshold && after >= threshold)
        {
            Milestone milestone = new Milestone(){ statType= statType, description = desc, milestoneValue = value, timescale = timescale};
            if(bestUnusedMilestone.milestoneValue < milestone.milestoneValue)
            {
                bestUnusedMilestone = milestone;
            }
            onMilestoneAchieved.Invoke(milestone);
        }
    }

    public void UseMilestone()
    {
        lastMilestoneUsed = DateTime.Now;
        bestUnusedMilestone = default(Milestone);
    }

    public PlayerModel(int tid, int pid, string dn)
    {
        defaultName = dn;
        positionID = pid;
        teamID = tid;
        hivemindID = -1;
        ResetAll();
        
        SyncPlayerStat(curLifeStats.berriesDeposited, curGameStats.berriesDeposited, curSetStats.berriesDeposited);
        SyncPlayerStat(curLifeStats.berriesGrabbed, curGameStats.berriesGrabbed, curSetStats.berriesGrabbed);
        SyncPlayerStat(curLifeStats.berriesKicked, curGameStats.berriesKicked, curSetStats.berriesKicked);
        SyncPlayerStat(curLifeStats.deaths, curGameStats.deaths, curSetStats.deaths);
        SyncPlayerStat(curLifeStats.kills, curGameStats.kills, curSetStats.kills);
        SyncPlayerStat(curLifeStats.snailKills, curGameStats.snailKills, curSetStats.snailKills);
        SyncPlayerStat(curLifeStats.formFails, curGameStats.formFails, curSetStats.formFails);
        SyncPlayerStat(curLifeStats.berryFails, curGameStats.berryFails, curSetStats.berryFails);
        SyncPlayerStat(curLifeStats.militaryKills, curGameStats.militaryKills, curSetStats.militaryKills);
        SyncPlayerStat(curLifeStats.queenKills, curGameStats.queenKills, curSetStats.queenKills);
        SyncPlayerStat(curLifeStats.snailMoved, curGameStats.snailMoved, curSetStats.snailMoved);
        SyncPlayerStat(curLifeStats.speedObtained, curGameStats.speedObtained, curSetStats.speedObtained);
        SyncPlayerStat(curLifeStats.swordObtained, curGameStats.swordObtained, curSetStats.swordObtained);
        SyncPlayerStat(curLifeStats.berriesKicked_OtherTeam, curGameStats.berriesKicked_OtherTeam, curSetStats.berriesKicked_OtherTeam);
        SyncPlayerStat(curLifeStats.militaryDeaths, curGameStats.militaryDeaths, curSetStats.militaryDeaths);
        SyncPlayerStat(curLifeStats.formGuards, curGameStats.formGuards, curSetStats.formGuards);
        SyncPlayerStat(curLifeStats.ledgeGuards, curGameStats.ledgeGuards, curSetStats.ledgeGuards);
        SyncPlayerStat(curLifeStats.snailGuards, curGameStats.snailGuards, curSetStats.snailGuards);

        SyncPlayerTopStat(curLifeStats.longestLife, curGameStats.longestLife, curSetStats.longestLife);

        SyncPlayerMilestone(curLifeStats.militaryKills, curGameStats.militaryKills, curSetStats.militaryKills, null, StatValueType.KD);
        SyncPlayerMilestone(curLifeStats.queenKills, curGameStats.queenKills, curSetStats.queenKills, null, StatValueType.QueenKills);
        SyncPlayerMilestone(curLifeStats.berriesDeposited, curGameStats.berriesDeposited, curSetStats.berriesDeposited, null, StatValueType.Berries);
        SyncPlayerMilestone(curLifeStats.snailMoved, curGameStats.snailMoved, curSetStats.snailMoved, null, StatValueType.Snail);

        curLifeStats.berriesDeposited.onChange.AddListener((b, a) => MaybeSyncCareerStats(StatValueType.Berries, b, a));
        curLifeStats.snailMoved.onChange.AddListener((b, a) => MaybeSyncCareerStats(StatValueType.Snail, b, a));
        curLifeStats.militaryKills.onChange.AddListener((b, a) => MaybeSyncCareerStats(StatValueType.KD, b, a));
        curLifeStats.queenKills.onChange.AddListener((b, a) => MaybeSyncCareerStats(StatValueType.QueenKills, b, a));
        
        curGameDerivedStats = new Dictionary<StatValueType, StatValue>();
        curSetDerivedStats = new Dictionary<StatValueType, StatValue>();
        foreach (var type in GetStatList())
        {
            var stat = new StatValue();
            stat.type = type;
            curGameDerivedStats[type] = stat;
            curSetDerivedStats[type] = stat;
        }
        PlayerStaticData.onPlayerData.AddListener(OnStaticData);
    }

    void OnStaticData(PlayerStaticData.PlayerData data)
    {
        if (data.hivemindID == hivemindID)
            playerName.property = data.name;
    }
    public StatValue[] GetBestStats(int numStats, bool forSet = false)
    {
        List<StatValue> stats = new List<StatValue>();
        foreach(var stat in (forSet ? curSetDerivedStats : curGameDerivedStats))
        {
            if(stat.Key != StatValueType.Value) //don't show value stat
                stats.Add(stat.Value);
        }
        stats.Sort();
        if (stats.Count > numStats)
            return stats.GetRange(0, numStats).ToArray();
        return stats.ToArray();
    }

    public CombinedStylePoints GetStylePoints(bool forSet = false)
    {
        var ret = new CombinedStylePoints();
        foreach (var stat in (forSet ? curSetDerivedStats : curGameDerivedStats))
        {
            if (stat.Value.isMilitary)
                ret.totalMilitaryPoints += stat.Value.stylePoints;
            else if (stat.Key == StatValueType.Value)
                ret.valuePoints += stat.Value.stylePoints;
            else
                ret.totalObjectivePoints += stat.Value.stylePoints;
        }
        return ret;
    }

    public CombinedStylePoints gameStylePoints { get { return GetStylePoints(false); } }
    public CombinedStylePoints setStylePoints { get { return GetStylePoints(true); } }
    public bool isLikelyMilitary { get { var sp = GetStylePoints(false); return sp.totalMilitaryPoints > sp.totalObjectivePoints; } }
    public bool isLikelyObjective { get { var sp = GetStylePoints(false); return sp.totalMilitaryPoints <= sp.totalObjectivePoints; } }
}

