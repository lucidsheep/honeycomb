using UnityEngine;
using System.Collections;

public class TeamModel
{
    public LSProperty<string> teamName = new LSProperty<string>("");
    public LSProperty<int> setWins = new LSProperty<int>(0);

	public PlayerModel[] players;

    public int teamID;

    string[] defaultNames = { "Stripes", "Abs", "Queen", "Skulls", "Chex" };
    string[] defaultTeamNames = { "Blue Team", "Gold Team" };

    public TeamModel(int tid)
    {
        teamName.property = defaultTeamNames[tid];
        players = new PlayerModel[5];
        teamID = tid;
        for(int i = 0; i < 5; i ++)
        {
            var p = new PlayerModel(tid, i, defaultNames[i]);
            players[i] = p;
        }
    }

    public void EndGame()
    {
        foreach (var p in players)
            p.ResetLife(true);
    }
    public void ResetGame()
    {
        foreach (var p in players)
            p.ResetGame();
    }

    public void ResetSet()
    {
        foreach (var p in players)
            p.ResetSet();
        setWins.property = 0;
    }

    public int GetBerryScore()
    {
        var ret = 0;
        foreach (var p in players)
            ret += p.curGameStats.berriesDeposited.property + p.curGameStats.berriesKicked.property;
        foreach (var p in GameModel.instance.teams[1 - teamID].players)
            ret += p.curGameStats.berriesKicked_OtherTeam.property;
        return ret;
    }
}

