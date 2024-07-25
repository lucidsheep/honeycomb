using UnityEngine;
using System.Collections;
using TMPro;

public class BoxScoreBase : MonoBehaviour
{
	public string moduleName;
	public TextMeshPro blueName, goldName, queenCol, berryCol, snailCol;
	public SpriteRenderer highlightBox;
	public Sprite blueHighlight, goldHighlight;
	public Vector2 highlightStart = new Vector2(1.4f, .19f);
	public float highlightBerry = .5f, highlightSnail = .57f, highlightGold = -.19f;

	public bool usePercent = true;

	virtual public void OnPostgame(int winningTeam, string winType)
	{
		blueName.text = GameModel.instance.teams[0].teamName.property;
		goldName.text = GameModel.instance.teams[1].teamName.property;

		int blueQueens, goldQueens, blueBerries, goldBerries, blueSnail, goldSnail;
		blueQueens = GameModel.instance.teams[1].players[2].curGameStats.deaths.property;
		goldQueens = GameModel.instance.teams[0].players[2].curGameStats.deaths.property;
		blueBerries = GameModel.instance.teams[0].GetBerryScore();
		goldBerries = GameModel.instance.teams[1].GetBerryScore();
		blueSnail = SnailModel.bluePercentage;
		goldSnail = SnailModel.goldPercentage;

		if(highlightBox != null)
		{
			highlightBox.sprite = winningTeam == 0 ? blueHighlight : goldHighlight;
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
				highlightStart.x + (highlightLoc > 2 ? highlightBerry : 0f) + (highlightLoc > 4 ? highlightSnail : 0f),
				highlightLoc % 2 == 1 ? highlightStart.y : highlightGold,
				1f);
		}
		queenCol.text = blueQueens + "\n" + goldQueens;
		berryCol.text = blueBerries + "\n" + goldBerries;

		//snails are complicated
		
		string snailTxt = "";
		if(blueSnail >= 100) snailTxt += "<size=66%>" + blueSnail.ToString() + "</size>\n";
		else if(blueSnail <= 0) snailTxt += "--\n";
		else snailTxt += blueSnail.ToString() + (usePercent ? "%" : "") + "\n";

		if(goldSnail >= 100) snailTxt += "<size=66%>" + goldSnail.ToString() + "</size>";
		else if(goldSnail <= 0) snailTxt += "--";
		else snailTxt += goldSnail.ToString() + (usePercent ? "%" : "");
		snailCol.text = snailTxt;
	}
}

