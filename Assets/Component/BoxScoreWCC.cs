using UnityEngine;
using System.Collections;

public class BoxScoreWCC : BoxScoreBase
{
	public GameObject blueWinsBG, goldWinsBG;

    public override void OnPostgame(int winningTeam, string winType)
    {
        base.OnPostgame(winningTeam, winType);

        blueWinsBG.SetActive(winningTeam == 0);
        goldWinsBG.SetActive(winningTeam == 1);

        var txtToBold = winningTeam == 0 ? blueName : goldName;
        txtToBold.text = "<b>" + txtToBold.text + "</b>";
    }
}

