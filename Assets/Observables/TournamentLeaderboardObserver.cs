using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TournamentLeaderboardObserver : KQObserver
{
	//const leaderboardList = ["kills_queen_aswarrior", "berries_kicked", "warrior_ratio", "berries",
	//"warrior_deaths", "snail", "snail_deaths"];

	public int numRows = 10;
	public TextMeshPro leaderboardNameTxt, leaderboardPlayersTxt, leaderboardValuesTxt, leaderboardTitleTxt;
	public SpriteRenderer frame;

	public GameObject fontContainer;
	public int maxNameLength = 13;
	bool dirty = false;
	TournamentLeaderboard cachedLeaderboard;

	static Dictionary<string, string> leaderboardList = new Dictionary<string, string> {
		{ "kills_queen_aswarrior", "Usurper"},
		{ "berries_kicked", "Marksman"},
		{ "warrior_ratio", "Space Cadet"},
		{ "berries", "Top Shareholder"},
		{ "warrior_deaths", "Targeted"},
		{ "snail", "Trailblazer"},
		{ "snail_deaths", "Tastiest Treat"},
		{ "deaths", "Most Generous"},
		{ "jason_points", "Jason Points™"},
		{ "kills_queen_asqueen", "Regicide"},
		{ "warrior_life", "Immortal"},
		{ "bump_assists", "Bump Ninja"},
		{ "drone_kills_withberry", "Impassable"}
	};
	// Use this for initialization
	override public void Start()
	{
		base.Start();
		TournamentLeaderboardManager.OnLeaderboardReceived.AddListener(OnLeaderboard);
		leaderboardNameTxt.text = leaderboardPlayersTxt.text = leaderboardValuesTxt.text = "";
	}

	void OnLeaderboard(TournamentLeaderboard leaderboard)
    {
		cachedLeaderboard = leaderboard;
		dirty = true;
    }

	string GetLBValue(string lbName, TournamentLeaderboardPlayer player)
    {
		switch(lbName)
        {
			case "kills_queen_aswarrior": return player.kills_queen_aswarrior.ToString();
			case "berries_kicked": return player.berries_kicked.ToString();
			case "warrior_ratio": return System.Math.Round(player.warrior_ratio, 3).ToString();
			case "berries": return player.berries.ToString();
			case "warrior_deaths": return player.warrior_deaths.ToString();
			case "snail": return Mathf.FloorToInt(player.snail / SnailModel.SNAIL_METER).ToString();
			case "snail_deaths": return player.snail_deaths.ToString();
			case "deaths": return player.deaths.ToString();
			case "jason_points": return player.jason_points.ToString();
			case "kills_queen_asqueen": return player.kills_queen_asqueen.ToString();
			case "bump_assists": return player.bump_assists.ToString();
			case "warrior_life": return Util.FormatTime(player.warrior_life);
			case "drone_kills_withberry": return player.drone_kills_withberry.ToString();
			case "???":
			default: return "0";
        }
    }

	void Update()
	{
		if(dirty)
        {
			dirty = false;
			var leaderboard = cachedLeaderboard;
			var lbName = "???";
			var lbPlayers = "";
			var lbValues = "";
			if(leaderboard.leaderboardName == "jason_points" && ViewModel.currentTheme.leaderboardTargetName != "")
				lbName = ViewModel.currentTheme.leaderboardTargetName + " Points™";
			else if (leaderboardList.ContainsKey(leaderboard.leaderboardName))
				lbName = leaderboardList[leaderboard.leaderboardName];
			leaderboardNameTxt.text = "<b>" + pString(lbName) + "</b>";
			int limit = Mathf.Min(numRows, leaderboard.players.Length);
			for(int i = 0; i < limit; i++)
			{
				var player = leaderboard.players[i];
				lbPlayers += Util.SmartTruncate(pString(player.name), maxNameLength) + "\n";
				lbValues += GetLBValue(leaderboard.leaderboardName, player) + "\n";
			}
			leaderboardPlayersTxt.text = lbPlayers;
			leaderboardValuesTxt.text = lbValues;
		}
	}

    protected override void OnThemeChange()
    {
        base.OnThemeChange();
		if(frame != null)
			frame.gameObject.SetActive(bgContainer.sprite == null);
    }

	override public void OnParameters()
    {
		base.OnParameters();
		if(moduleParameters.ContainsKey("hideTitle"))
        {
			leaderboardTitleTxt.text = "";
        }
		if(moduleParameters.ContainsKey("fontScale"))
		{
			fontContainer.transform.localScale = Vector3.one * float.Parse(moduleParameters["fontScale"]);
		}
		if(moduleParameters.ContainsKey("fontSize"))
		{
			leaderboardPlayersTxt.fontSize = leaderboardValuesTxt.fontSize = float.Parse(moduleParameters["fontSize"]);
		}
		if(moduleParameters.ContainsKey("fontSpacing"))
		{
			leaderboardPlayersTxt.lineSpacing = float.Parse(moduleParameters["fontSpacing"]);
		}
		if(moduleParameters.ContainsKey("detailFont"))
        {
			var font = FontDB.GetFont(moduleParameters["detailFont"]);
			if (font != null)
			{
				leaderboardValuesTxt.font = font;
			}
        }
		if(moduleParameters.ContainsKey("detailFontSpacing"))
		{
			leaderboardValuesTxt.lineSpacing = float.Parse(moduleParameters["detailFontSpacing"]);
		}

		if(moduleParameters.ContainsKey("nameLength"))
		{
			maxNameLength = int.Parse(moduleParameters["nameLength"]);
		}
	}
}

