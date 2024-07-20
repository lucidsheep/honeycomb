using UnityEngine;
using System.Collections;
using TMPro;

public class BoxScoreCamp : BoxScoreBase
{
	public SpriteRenderer crown, berry, snail;
    public TextMeshPro winHeader;

    public override void OnPostgame(int winningTeam, string winType)
    {
        base.OnPostgame(winningTeam, winType);

        crown.color = berry.color = snail.color = blueName.color = goldName.color = Color.white;

        SpriteRenderer icon = winType == "military" ? crown : winType == "snail" ? snail : berry;
        
        var colToUse = winningTeam == 0 ? ViewModel.currentTheme.blueTheme.pColor : ViewModel.currentTheme.goldTheme.pColor;
        icon.color = colToUse;

        if(winningTeam == 0)
            blueName.color = colToUse;
        else
            goldName.color = colToUse;

        var tmp = winType == "military" ? queenCol : winType == "snail" ? snailCol : berryCol;
        var txtToChange = tmp.text;

        int firstIndex = winningTeam == 0 ? 0 : txtToChange.IndexOf('\n') + 1;
        int secondIndex = winningTeam == 0 ? txtToChange.IndexOf('\n') : txtToChange.Length;

        txtToChange = txtToChange.Insert(secondIndex, "</color>");
        txtToChange = txtToChange.Insert(firstIndex, "<color=#" + ColorUtility.ToHtmlStringRGB(colToUse) + ">");

        tmp.text = txtToChange;
        winHeader.text = (winningTeam == 0 ? "Blue" : "Gold") + " Team Wins!";

        //this is a war crime
        this.gameObject.GetComponentInParent<GlobalFade>().ForceUpdate(this.gameObject);
    }
}

