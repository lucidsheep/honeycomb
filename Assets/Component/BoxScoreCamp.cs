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

        crown.color = berry.color = snail.color = Color.white;

        SpriteRenderer icon = winType == "military" ? crown : winType == "snail" ? snail : berry;

        icon.color = winHeader.color = winningTeam == 0 ? ViewModel.currentTheme.blueTheme.pColor : ViewModel.currentTheme.goldTheme.pColor;

        winHeader.text = (winningTeam == 0 ? "Blue" : "Gold") + " Team Wins!";
    }
}

