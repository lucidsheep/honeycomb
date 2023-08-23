using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PlayerStaticData : MonoBehaviour
{
	static PlayerStaticData instance;

	public class PlayerData
    {
		public HMUserData data;
        public TournamentPlayer tournamentData;
        public int hivemindID;
		public Sprite profilePic;
        public int profilePicRotation;
        public GameDataStructure dayStatistics;
        public GameDataStructure careerStatistics;
        public string hatEmoji = "";

        public string pronouns { get {
                if (data != null && data.pronouns != "") return data.pronouns;
                if (tournamentData != null && tournamentData.pronouns != "") return tournamentData.pronouns;
                return "";
            } }

        public string name
        {
            get
            {
                var ret = "";
                if (data != null && data.name != "") ret = data.name;
                else if (tournamentData != null && tournamentData.playerName != "") ret = tournamentData.playerName;

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

    public Texture2D testSprite;
	Dictionary<int, PlayerData> _playerDB = new Dictionary<int, PlayerData>();
	public static Dictionary<int, PlayerData> playerDB { get { return instance._playerDB; } }

    public static UnityEvent<PlayerData> onPlayerData = new UnityEvent<PlayerData>();
    public static PlayerData GetPlayer(int playerID)
    {
        if (playerDB == null) return null;
        if (!playerDB.ContainsKey(playerID)) return null;
        return playerDB[playerID];
    }

    public static string GetPronouns(int playerID)
    {
        var p = GetPlayer(playerID);
        if (p == null) return "";
        return p.pronouns;
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
            foreach(var stat in data.stats)
            {
                if (stat == null) continue;
                switch(stat.name)
                {
                    case "military_kills": stats.militaryKills.property = stat.value; break;
                    case "queen_kills": stats.queenKills.property = stat.value; break;
                    case "snail_distance": stats.snailMoved.property = Mathf.FloorToInt(stat.value * SnailModel.SNAIL_METER); break;
                    case "berries": stats.berriesDeposited.property = stat.value; break;
                    default: break;
                }
            }
        }
        return stats;
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
        
        if (userData.image != null && userData.image != "")
            NetworkManager.GetUserProfilePic(playerID, userData.image);
        Debug.Log("user " + playerID + " added to DB");
        onPlayerData.Invoke(playerDB[playerID]);
    }

    public static void AddPlayer(int playerID, TournamentPlayer data)
    {
        if (!playerDB.ContainsKey(playerID))
        {
            PlayerData pd = new PlayerData();
            instance._playerDB.Add(playerID, pd);
            pd.tournamentData = data;
            pd.hivemindID = playerID;
            pd.dayStatistics = new GameDataStructure();
        }
        else if (playerDB[playerID].tournamentData == null)
            playerDB[playerID].tournamentData = data;

        if (data.profilePic != null)
        {
            playerDB[playerID].profilePic = data.profilePic;
            playerDB[playerID].profilePicRotation = 0;
        }

    }
    public static void OnPlayerProfilePic(int playerID, Texture2D texture, int rotationLevel = 0)
    {
        Texture2D test = texture;
        instance.testSprite = test;
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


}

