using UnityEngine;
using System.Collections.Generic;

public class TournamentPresetData : MonoBehaviour
{
	public List<TournamentTeamData> teams;

	static TournamentPresetData instance;

	public static LSProperty<TournamentTeamData> blueTeam = new LSProperty<TournamentTeamData>();
	public static LSProperty<TournamentTeamData> goldTeam = new LSProperty<TournamentTeamData>();
    private void Awake()
    {
		instance = this;
		
    }

    private void Start()
    {
        //fill all static data in player DB
		foreach(var t in teams)
        {
			foreach(var p in t.players)
            {
				PlayerStaticData.AddPlayer(p.hivemindID, p);
            }
        }
    }
    public static void OnTournamentData(string blueID, string goldID)
    {
		Debug.Log("TTD " + blueID + " " + goldID);
        if(instance == null)
            return;
		blueTeam.property = instance.teams.Find(x => x.hivemindID == blueID);
		goldTeam.property = instance.teams.Find(x => x.hivemindID == goldID);
    }

	public static void ClearPresetData()
    {
		blueTeam.property = goldTeam.property = null;
    }


	public static int GetQueenPresetID(int teamID)
    {
		if (teamID == 0 && blueTeam.property != null) return blueTeam.property.players[0].hivemindID;
		if (teamID == 1 && goldTeam.property != null) return goldTeam.property.players[0].hivemindID;
		return -1;
    }

	public static TournamentTeamData GetTeam(string teamID)
    {
        
		return instance.teams.Find(x => x.hivemindID == teamID);;
    }
}

