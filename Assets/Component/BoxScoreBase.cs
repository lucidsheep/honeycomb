using UnityEngine;
using System.Collections;
using TMPro;

public class BoxScoreBase : MonoBehaviour
{
	public TextMeshPro blueName, goldName, queenCol, berryCol, snailCol;
	public SpriteRenderer highlightBox;
	public Sprite blueHighlight, goldHighlight;
	public Vector2 highlightStart = new Vector2(1.4f, .19f);
	public float highlightBerry = .5f, highlightSnail = .57f;

	public bool usePercent = true;

	virtual public void OnPostgame(int winningTeam, string winType)
	{
		blueName.text = GameModel.instance.teams[0].teamName.property;
		goldName.text = GameModel.instance.teams[1].teamName.property;

		highlightBox.sprite = winningTeam == 0 ? blueHighlight : goldHighlight;

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
			highlightStart.x + (highlightLoc > 2 ? highlightBerry : 0f) + (highlightSnail > 4 ? .57f : 0f),
			highlightStart.y * (highlightLoc % 2 == 1 ? 1f : -1f),
			1f);

		queenCol.text = blueQueens + "\n" + goldQueens;
		berryCol.text = blueBerries + "\n" + goldBerries;
		snailCol.text = (blueSnail == 0 ? "--" : (blueSnail.ToString() + (blueSnail >= 100 || !usePercent ? "" : "%"))) + "\n" + (goldSnail == 0 ? "--" : (goldSnail.ToString() + (goldSnail >= 100 || !usePercent ? "" : "%")));
	}
}

