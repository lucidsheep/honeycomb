using UnityEngine;
using System.Collections;
using TMPro;

public class BoxScore : BoxScoreBase
{
	public SpriteRenderer[] blueBGs;
	public SpriteRenderer[] goldBGs;

	void SetColorPreserveAlpha(SpriteRenderer target, Color color)
    {
		target.color = new Color(color.r, color.g, color.b, target.color.a);
    }
	override public void OnPostgame(int winningTeam, string winType)
    {
		base.OnPostgame(winningTeam, winType);

		foreach (var bg in blueBGs)
			SetColorPreserveAlpha(bg, ViewModel.currentTheme.blueTheme.pColor);
		foreach (var bg in goldBGs)
			SetColorPreserveAlpha(bg, ViewModel.currentTheme.goldTheme.pColor);

		if(ViewModel.currentTheme.postgameHeaderFont != "")
        {
			blueName.font = goldName.font = FontDB.GetFont(ViewModel.currentTheme.postgameHeaderFont);
        }
	}
}

