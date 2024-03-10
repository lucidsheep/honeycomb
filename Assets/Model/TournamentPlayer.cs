using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class TournamentPlayer : ScriptableObject
{
	public string playerName;
	public string pronouns;
	public Sprite profilePic;
	public int hivemindID;
	public int tournamentID;
}
[System.Serializable]
public class TournamentLeaderboardPlayer
{
	public int id;
	public string name;
	public int kills_military;
	public int kills_queen;
	public int kills_queen_aswarrior;
	public int berries;
	public int snail;
	public int berries_kicked;
	public int deaths;
	public int warrior_uptime;
	public int kills_all;
	public float warrior_ratio;
	public int warrior_deaths;
	public int snail_deaths;
	public int jason_points;

	public static TournamentLeaderboardPlayer PlayerModelToLeaderboardRow(PlayerModel player, TournamentLeaderboardPlayer curValues = null, bool sendAllStats = true)
	{
		var ret = curValues == null ?  new TournamentLeaderboardPlayer() : curValues;

		ret.id = player.hivemindID;
		ret.name = player.displayName;

		if (sendAllStats)
		{
			ret.berries += player.curGameStats.berriesDeposited.property + player.curGameStats.berriesKicked.property;
			ret.berries_kicked += player.curGameStats.berriesKicked.property;
			ret.deaths += player.curGameStats.deaths.property;
			ret.kills_military += player.curGameStats.militaryKills.property;
			ret.kills_queen += player.curGameStats.queenKills.property;
			if (player.positionID == 2) //stat is for non-queen players only
			{
				ret.kills_queen_aswarrior += 0;
				ret.warrior_deaths += 0;
				ret.kills_all += 0; //used for space cadet
			}
			else
			{
				ret.kills_queen_aswarrior += ret.kills_queen;
				ret.warrior_deaths += player.curGameStats.militaryDeaths.property;
				ret.kills_all += player.curGameStats.kills.property;
			}
			ret.snail += player.curGameStats.snailMoved.property;
			ret.snail_deaths = player.curGameStats.snailDeaths.property;
			ret.warrior_uptime = player.warriorSeconds;
			//warrior_ratio is determined by server by combining uptime and kills
		}
		//jason points are always sent, because jason
		ret.jason_points += player.jasonPoints;
		return ret;
	}
}
[System.Serializable]
public class TournamentLeaderboard
{
	public string leaderboardName;
	public TournamentLeaderboardPlayer[] players;
}

[System.Serializable]
public class TournamentLeaderboardSubmission
{
	public string scene, type, leaderboard;
	public TournamentLeaderboardPlayer[] players;
}


