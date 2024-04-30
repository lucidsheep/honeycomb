using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PlayerStaticData : MonoBehaviour
{
	static PlayerStaticData instance;

	public class PlayerData
    {
		public HMUserData data;
        public TournamentPlayer presetData;
        public HMTournamentPlayer tournamentData;
        public int hivemindID;
		public Sprite profilePic;
        public int profilePicRotation;
        public string profilePicURL = "";
        public GameDataStructure dayStatistics;
        public GameDataStructure careerStatistics;
        public string sceneTag { get
            {
                if (tournamentData != null && tournamentData.scene != "") return tournamentData.scene;
                if (data != null && data.scene != "") return data.scene;
                return "";
            } }
        public string sceneColor;
        public string hatEmoji = "";

        public string pronouns { get {
                if (tournamentData != null && tournamentData.pronouns != "") return tournamentData.pronouns;
                if (data != null && data.pronouns != "") return data.pronouns;
                if (presetData != null && presetData.pronouns != "") return presetData.pronouns;
                return "";
            } }

        public string name
        {
            get
            {
                var ret = "";
                if (tournamentData != null && tournamentData.name != "") return tournamentData.name;
                else if (data != null && data.name != "") ret = data.name;
                else if (presetData != null && presetData.playerName != "") ret = presetData.playerName;

                /*
                 * uncomment this to remove hat emoji from user names
                if(hatEmoji != "")
                {
                    var texEnum = System.Globalization.StringInfo.GetTextElementEnumerator(ret);
                    string final = "";
                    while(texEnum.MoveNext())
                    {
                        var elem = texEnum.GetTextElement();
                        if (elem != hatEmoji)
                            final += elem;
                    }
                    return final;
                }

                */

                return ret;
                
            }
        }
    }

	Dictionary<int, PlayerData> _playerDB = new Dictionary<int, PlayerData>();
	public static Dictionary<int, PlayerData> playerDB { get { return instance._playerDB; } }

    public static UnityEvent<PlayerData> onPlayerData = new UnityEvent<PlayerData>();
    public static PlayerData GetPlayer(int playerID)
    {
        if (playerDB == null) return null;
        if (!playerDB.ContainsKey(playerID)) return null;
        return playerDB[playerID];
    }
    public static List<PlayerData> GetTournamentPlayers(int teamID)
    {
        var ret = new List<PlayerData>();
        if (playerDB == null) return ret;

        foreach(var pd in playerDB.Values)
        {
            if (pd.tournamentData != null && pd.tournamentData.team == teamID)
            {
                ret.Add(pd);
                Debug.Log(pd.name);
            }
        }

        /*
         * todo - figure out a better merging function
        Debug.Log("Found " + ret.Count + " team players total");

        //remove fake HM entries that have a similar signed in entry
        //also removed logged in entries that can't be matched to a fake entry
        var loggedIn = ret.FindAll(x => x.hivemindID < 1000000);
        var tempUsers = ret.FindAll(x => x.hivemindID >= 1000000);
        foreach(var check in loggedIn)
        {
            var temp = tempUsers.Find(x => x.name.Contains(check.name));
            if (temp != null)
            {
                Debug.Log(temp.name + " contains " + check.name);
                ret.Remove(temp);
            }
            else
            {
                Debug.Log("no name contains " + check.name);
                ret.Remove(check);
            }
        }
        */
        return ret;
    }
    public static string GetPronouns(int playerID)
    {
        var p = GetPlayer(playerID);
        if (p == null) return "";
        return p.pronouns;
    }

    public static string GetSceneTag(int playerID)
    {
        var p = GetPlayer(playerID);
        if (p == null) return "";
        return p.sceneTag;
    }

    public static (Sprite, int) GetProfilePic(int playerID)
    {
        var p = GetPlayer(playerID);
        if (p == null) return (null, 0);
        return (p.profilePic, p.profilePicRotation);
    }
    // Use this for initialization

    private void Awake()
    {
		instance = this;
    }
    void Start()
	{

	}

    static GameDataStructure SetCareerStats(HMUserData data)
    {
        var stats = new GameDataStructure();
        if(data.detail == null || data.detail == "")
        {
            //profile is public
            StatsFromHMArray(stats, data.stats);
        }
        return stats;
    }

    static GameDataStructure SetTournamentStats(HMTournamentPlayer data)
    {
        var stats = new GameDataStructure();
        StatsFromHMArray(stats, data.stats);
        return stats;
    }

    static void StatsFromHMArray(GameDataStructure hcData, HMUserStat[] hmData)
    {
        foreach (var stat in hmData)
        {
            if (stat == null) continue;
            switch (stat.name)
            {
                case "military_kills": hcData.militaryKills.property = stat.value; break;
                case "queen_kills": hcData.queenKills.property = stat.value; break;
                case "snail_distance": hcData.snailMoved.property = Mathf.FloorToInt(stat.value * SnailModel.SNAIL_METER); break;
                case "berries": hcData.berriesDeposited.property = stat.value; break;
                default: break;
            }
        }
    }
    public static void AddPlayer(int playerID, HMUserData userData)
    {
        if (!playerDB.ContainsKey(playerID))
        {
            PlayerData pd = new PlayerData();
            instance._playerDB.Add(playerID, pd);
            pd.data = userData;
            pd.hivemindID = playerID;
            pd.dayStatistics = new GameDataStructure();
            pd.careerStatistics = SetCareerStats(userData);
        }
        else if (playerDB[playerID].data == null)
            playerDB[playerID].data = userData;

        if (userData.image != null && userData.image != "" && playerDB[playerID].profilePicURL == "")
        {
            playerDB[playerID].profilePicURL = userData.image;
            NetworkManager.GetUserProfilePic(playerID, userData.image);
        }
        Debug.Log("user " + playerID + " added to DB");
        onPlayerData.Invoke(playerDB[playerID]);
    }

    public static void AddPlayer(int playerID, TournamentPlayer data)
    {
        if (!playerDB.ContainsKey(playerID))
        {
            PlayerData pd = new PlayerData();
            instance._playerDB.Add(playerID, pd);
            pd.presetData = data;
            pd.hivemindID = playerID;
            pd.dayStatistics = new GameDataStructure();
        }
        else if (playerDB[playerID].presetData == null)
            playerDB[playerID].presetData = data;

        if (data.profilePic != null)
        {
            playerDB[playerID].profilePic = data.profilePic;
            playerDB[playerID].profilePicRotation = 0;
            playerDB[playerID].profilePicURL = "preset";
        }

    }

    public static void AddPlayer(int playerID, HMTournamentPlayer data)
    {
        if (!playerDB.ContainsKey(playerID))
        {
            PlayerData pd = new PlayerData();
            instance._playerDB.Add(playerID, pd);
            pd.tournamentData = data;
            pd.hivemindID = playerID;
            pd.dayStatistics = SetTournamentStats(data);
            
        }
        else if (playerDB[playerID].tournamentData == null)
            playerDB[playerID].tournamentData = data;

        if (data.image != null && data.image != "" && playerDB[playerID].profilePicURL == "")
        {
            playerDB[playerID].profilePicURL = data.image;
            NetworkManager.GetUserProfilePic(playerID, data.image);
        }

    }
    public static void OnPlayerProfilePic(int playerID, Texture2D texture, int rotationLevel = 0)
    {
        Texture2D test = texture;
        if (texture.height != texture.width)
        {
            //need to crop into a square
            int padding = 0;
            int smallerDim = Mathf.Min(texture.height, texture.width);
            Texture2D dest = new Texture2D(smallerDim, smallerDim, texture.format, texture.mipmapCount, false );
            if(texture.width > texture.height)
            {
                padding = (texture.width - texture.height) / 2;

                dest.SetPixels(texture.GetPixels(padding, 0, smallerDim, smallerDim));
            } else
            {
                padding = (texture.height - texture.width) / 2;
                dest.SetPixels(texture.GetPixels(0, padding, smallerDim, smallerDim));
            }
            //Destroy(texture); //release old texture from memory
            texture = dest;
            texture.Apply();
        }
        if (texture.height != 512) //resize
        {
            RenderTexture dest = new RenderTexture(512, 512, 24); //, UnityEngine.Experimental.Rendering.GraphicsFormatUtility.GetGraphicsFormat(texture.format, false));
            Graphics.Blit(texture, dest);
            texture.Reinitialize(512, 512);
            RenderTexture.active = dest;
            texture.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
            RenderTexture.active = null;
            texture.Apply();            
        }
        
        playerDB[playerID].profilePic = Sprite.Create(texture, new Rect(0f, 0f, 512, 512), new Vector2(.5f, .5f), 140f);
        playerDB[playerID].profilePicRotation = rotationLevel;
        Debug.Log("user " + playerID + " pic added");
        
        onPlayerData.Invoke(playerDB[playerID]);
    }

    public static bool HasHivemindData(int playerID)
    {
        if (!playerDB.ContainsKey(playerID)) return false;
        return playerDB[playerID].data != null;
    }
    public static bool HasTournamentData(int playerID)
    {
        if (!playerDB.ContainsKey(playerID)) return false;
        return playerDB[playerID].tournamentData != null;
    }

    public static void ClearFakePlayers()
    {
        List<int> idsToRemove = new List<int>();
        foreach(var player in playerDB)
        {
            if (player.Key >= 1000000)
                idsToRemove.Add(player.Key);
        }
        foreach (var id in idsToRemove)
            playerDB.Remove(id);
    }

}

