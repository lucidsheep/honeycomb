using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;

public class TournamentLeaderboardRiveObserver : KQObserver
{
    public RiveLeaderboardComponent riveComponent;
	public int numRows = 10;

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
		//leaderboardNameTxt.text = leaderboardPlayersTxt.text = leaderboardValuesTxt.text = "";
	}

	void OnLeaderboard(TournamentLeaderboard leaderboard)
    {
		cachedLeaderboard = leaderboard;
		dirty = true;
        //todo - fade out old leaderboard, wait, then show new one
    }

	int GetLBValue(string lbName, TournamentLeaderboardPlayer player)
    {
		switch(lbName)
        {
			case "kills_queen_aswarrior": return player.kills_queen_aswarrior;
			case "berries_kicked": return player.berries_kicked;
			case "warrior_ratio": return Mathf.RoundToInt(player.warrior_ratio);
			case "berries": return player.berries;
			case "warrior_deaths": return player.warrior_deaths;
			case "snail": return Mathf.FloorToInt(player.snail / SnailModel.SNAIL_METER);
			case "snail_deaths": return player.snail_deaths;
			case "deaths": return player.deaths;
			case "jason_points": return player.jason_points;
			case "kills_queen_asqueen": return player.kills_queen_asqueen;
			case "bump_assists": return player.bump_assists;
			case "warrior_life": return player.warrior_life;
			case "drone_kills_withberry": return player.drone_kills_withberry;
			case "???":
			default: return 0;
        }
    }

	void Update()
	{
		if(dirty)
        {
            //todo - some kind of cached leaderboard check for up arrow thing
			dirty = false;
			var leaderboard = cachedLeaderboard;
			var lbName = "???";
			if(leaderboard.leaderboardName == "jason_points" && ViewModel.currentTheme.leaderboardTargetName != "")
				lbName = ViewModel.currentTheme.leaderboardTargetName + " Points™";
			else if (leaderboardList.ContainsKey(leaderboard.leaderboardName))
				lbName = leaderboardList[leaderboard.leaderboardName];
            riveComponent.leaderboardCategory.property = pString(lbName);
			int limit = Mathf.Min(numRows, leaderboard.players.Length);
			for(int i = 0; i < limit; i++)
			{
				var player = leaderboard.players[i];
                riveComponent.SetLeaderboardEntry(i, Util.SmartTruncate(pString(player.name), maxNameLength), GetLBValue(leaderboard.leaderboardName, player), false);
			}
            riveComponent.leaderboardSwitch.property = true;
		}
	}
}

