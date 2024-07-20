using UnityEngine;
using System.Collections;
using TMPro;

public class PostgamePlayerCard : MonoBehaviour
{
	public string moduleName = "defualtCard";
	public SpriteRenderer positionIcon;
	public TextMeshPro playerName, leftCol, rightCol;
	public SpriteRenderer[] crowns;
	public SpriteRenderer[] primaryBGs;
	public SpriteRenderer[] secondaryBGs;
	public ProfilePicture profileFrame;
	public GameObject namePlate;
	public SpriteRenderer themeBG;
	public Sprite defaultCrown;

	Color fadedWhite = new Color(1f, 1f, 1f, 0f);

	void SetColorPreserveAlpha(SpriteRenderer target, Color color)
	{
		target.color = new Color(color.r, color.g, color.b, target.color.a);
	}

	public void OnPostgame(int teamID, int playerID)
    {
		var theme = ViewModel.currentTheme.GetTeamTheme(teamID);
		string suffix = teamID == 0 ? "blue" : "gold";
		themeBG.sprite = AppLoader.GetStreamingSprite("postgamePlayerCard_" + suffix);
		if(namePlate != null)
			namePlate.SetActive(themeBG.sprite == null);
		foreach (var bg in primaryBGs)
			SetColorPreserveAlpha(bg, theme.pColor);
		foreach (var bg in secondaryBGs)
			SetColorPreserveAlpha(bg, theme.sColor);
		var model = GameModel.GetPlayer(teamID, playerID);
		var (pic, rotation) = PlayerStaticData.GetProfilePic(model.hivemindID);
		playerName.text = model.displayName;
		var stats = model.GetBestStats(3);
		string leftTxt = "", rightTxt = "";
		for(var i = 0; i < 3; i++)
        {
			var stat = stats[i];
			if (stat.stylePoints == 0) // account for empty data
				break;
			leftTxt += stat.singleNumber + (i != 2 ? "\n" : "");
			rightTxt += stat.label + (i != 2 ? "\n" : "");
        }
		leftCol.text = leftTxt;
		rightCol.text = rightTxt;
		profileFrame.SetColor(profileFrame.mask.GetComponent<SpriteRenderer>().color, teamID); //for setting profile frame
		if (pic != null)
		{
			profileFrame.SetPicture(pic, rotation, .55f);
			positionIcon.sprite = SpriteDB.GetIcon(teamID, playerID);
		}
		else
		{
			var combinedPts = model.GetStylePoints();
			if (combinedPts.totalObjectivePoints >= combinedPts.totalMilitaryPoints)
			{
				profileFrame.SetPicture(SpriteDB.allSprites[teamID].playerSprites[playerID].drone_idle, 0, .5f);
			}
			else
			{
				profileFrame.SetPicture(SpriteDB.allSprites[teamID].playerSprites[playerID].soldier_attack, 0, .35f);
			}
			positionIcon.sprite = null;
		}
		var nQueenKills = model.curGameStats.queenKills.property;
		var customCrownEmpty = AppLoader.GetStreamingSprite("crownEmpty_" + suffix);
		var customCrownFull = AppLoader.GetStreamingSprite("crownFull_" + suffix);
		if(customCrownEmpty == null)
        {
			crowns[0].sprite = crowns[1].sprite = crowns[2].sprite = defaultCrown;
			SetColorPreserveAlpha(crowns[0], nQueenKills > 0 ? fadedWhite : theme.sColor);
			SetColorPreserveAlpha(crowns[1], nQueenKills > 1 ? fadedWhite : theme.sColor);
			SetColorPreserveAlpha(crowns[2], nQueenKills > 2 ? fadedWhite : theme.sColor);
		} else
        {
			crowns[0].color = crowns[1].color = crowns[2].color = new Color(1f,1f,1f, crowns[0].color.a);
			crowns[0].sprite = nQueenKills > 0 ? customCrownFull : customCrownEmpty;
			crowns[1].sprite = nQueenKills > 1 ? customCrownFull : customCrownEmpty;
			crowns[2].sprite = nQueenKills > 2 ? customCrownFull : customCrownEmpty;
		}

		if(ViewModel.currentTheme.postgameCardFont != "")
        {
			leftCol.font = rightCol.font = FontDB.GetFont(ViewModel.currentTheme.postgameCardFont);
        }

		if(ViewModel.currentTheme.playerCardStyle != null && ViewModel.currentTheme.playerCardStyle.scale > 0f)
        {
			leftCol.font = FontDB.GetFont(ViewModel.currentTheme.playerCardStyle.numberFont);
			rightCol.font = FontDB.GetFont(ViewModel.currentTheme.playerCardStyle.statFont);
			playerName.font = FontDB.GetFont(ViewModel.currentTheme.playerCardStyle.nameFont);
		}
	}
}

