using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TournamentQueueObserver : KQObserver
{
    public TournamentQueueTeamName listTemplate;
	public int numQueueEntries = 3;
    public Vector3 listStartPosition;
    public float listIncrement;
    public List<TournamentQueueTeamName> teamNames = new List<TournamentQueueTeamName>();

    public override void Start()
    {
        base.Start();
        NetworkManager.instance.onTournamentQueueData.AddListener(OnQueueData);
        NetworkManager.instance.onTournamentTeamName.AddListener(OnTournamentTeamName);

        var numTeams = numQueueEntries * 2;
        while(numTeams > 0)
        {
            var thisName = Instantiate(listTemplate, this.transform);
            thisName.transform.localPosition = listStartPosition;
            thisName.text.text = "";
            thisName.isBlue = numTeams % 2 == 1;
            listStartPosition.y += listIncrement;
            teamNames.Add(thisName);
            numTeams--;
        }
    }

    public void OnQueueData(HMTournamentQueueData data)
    {
        foreach(var n in teamNames)
        {
            n.text.text = "";
            n.teamID = -1;
        }

        int curEntry = 0;
        foreach(var entry in data.match_list)
        {
            teamNames[curEntry * 2].teamID = entry.blue_team;
            teamNames[curEntry * 2 + 1].teamID = entry.gold_team;

            teamNames[curEntry * 2].text.text = teamNames[curEntry * 2 + 1].text.text = "<i>TBD</i>";

            NetworkManager.GetTournamentTeamName(entry.blue_team);
            NetworkManager.GetTournamentTeamName(entry.gold_team);

            curEntry++;
            if (curEntry >= numQueueEntries)
                break;
        }
    }

    public void OnTournamentTeamName(int teamID, string teamName)
    {
        foreach(var tName in teamNames)
        {
            if(tName.teamID == teamID)
            {
                tName.text.text = teamName;
            }
        }
    }
}

