using UnityEngine;
using System.Collections;
using TMPro;

public class BoxScore : MonoBehaviour
{
	public TextMeshPro blueName, goldName, queenCol, berryCol, snailCol;
	public SpriteRenderer highlightBox;
	public SpriteRenderer[] blueBGs;
	public SpriteRenderer[] goldBGs;

	void SetColorPreserveAlpha(SpriteRenderer target, Color color)
    {
		target.color = new Color(color.r, color.g, color.b, target.color.a);
    }
	public void OnPostgame(int winningTeam, string winType)
    {
		blueName.text = GameModel.instance.teams[0].teamName.property;
		goldName.text = GameModel.instance.teams[1].teamName.property;

		foreach (var bg in blueBGs)
			SetColorPreserveAlpha(bg, ViewModel.currentTheme.blueTheme.pColor);
		foreach (var bg in goldBGs)
			SetColorPreserveAlpha(bg, ViewModel.currentTheme.goldTheme.pColor);
		int blueQueens, goldQueens, blueBerries, goldBerries, blueSnail, goldSnail;
		blueQueens = GameModel.instance.teams[1].players[2].curGameStats.deaths.property;
		goldQueens = GameModel.instance.teams[0].players[2].curGameStats.deaths.property;
		blueBerries = GameModel.instance.teams[0].GetBerryScore();
		goldBerries = GameModel.instance.teams[1].GetBerryScore();
		blueSnail = SnailModel.bluePercentage;
		goldSnail = SnailModel.goldPercentage;

		int highlightLoc;
		if (winType == "military")
			highlightLoc = 1;
		else if (winType == "economic")
			highlightLoc = 3;
		else
			highlightLoc = 5;
		if (winningTeam == 1)
			highlightLoc++;

		highlightBox.transform.localPosition = new Vector3(
			1.4f + (highlightLoc > 2 ? .5f : 0f) + (highlightLoc > 4 ? .57f : 0f),
			.19f * (highlightLoc % 2 == 1 ? 1f : -1f),
			1f);

		queenCol.text = blueQueens + "\n" + goldQueens;
		berryCol.text = blueBerries + "\n" + goldBerries;
		snailCol.text = (blueSnail == 0 ? "--" : (blueSnail.ToString() + (blueSnail >= 100 ? "" : "%"))) + "\n" + (goldSnail == 0 ? "--" : (goldSnail.ToString() + (goldSnail >= 100 ? "" : "%")));

		if(ViewModel.currentTheme.postgameHeaderFont != "")
        {
			blueName.font = goldName.font = FontDB.GetFont(ViewModel.currentTheme.postgameHeaderFont);
        }
	}
}

