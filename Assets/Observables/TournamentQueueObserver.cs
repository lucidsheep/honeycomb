using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TournamentQueueObserver : KQObserver
{
    public TournamentQueueTeamName listTemplate, listBlueTemplate, listGoldTemplate;
	public int numQueueEntries = 3;
    public Vector3 listStartPosition;
    public float listIncrement;
    public List<TournamentQueueTeamName> teamNames = new List<TournamentQueueTeamName>();

    public override void Start()
    {
        base.Start();
        NetworkManager.instance.onTournamentQueueData.AddListener(OnQueueData);
        NetworkManager.instance.onTournamentTeamName.AddListener(OnTournamentTeamName);

        SetTeamList();
    }

    void SetTeamList()
    {
        while(teamNames.Count > 0)
        {
            var tn = teamNames[0];
            Destroy(tn.gameObject);
            teamNames.RemoveAt(0);
        }

        var numTeams = numQueueEntries * 2;
        var listY = listStartPosition.y;
        while(numTeams > 0)
        {
            var isBlue = numTeams % 2 == 0;
            var thisName = Instantiate((listBlueTemplate != null ? (isBlue ? listBlueTemplate : listGoldTemplate) :  listTemplate), this.transform);
            thisName.transform.localPosition = new Vector3(listStartPosition.x, listY, listStartPosition.z);
            thisName.text.text = "";
            thisName.isBlue = isBlue;
            listY += listIncrement;
            teamNames.Add(thisName);
            numTeams--;
        }
    }

    public void OnQueueData(HMCabinetQueue data)
    {
        foreach(var n in teamNames)
        {
            n.text.text = "";
            n.teamID = -1;
        }

        int curEntry = 0;
        foreach(var entry in data.matches)
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

    override public void OnParameters()
    {
		base.OnParameters();
        
        if(moduleParameters.ContainsKey("listStartX"))
		{
            listStartPosition.x = float.Parse(moduleParameters["listStartX"]);
		}

        if(moduleParameters.ContainsKey("listStartY"))
		{
            listStartPosition.y = float.Parse(moduleParameters["listStartY"]);
		}

        if(moduleParameters.ContainsKey("listIncrement"))
		{
            listIncrement = float.Parse(moduleParameters["listIncrement"]);
		}

        if(moduleParameters.ContainsKey("length"))
        {
            numQueueEntries = int.Parse(moduleParameters["length"]);
        }
        SetTeamList();
    }
}

