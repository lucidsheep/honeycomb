using UnityEngine;
using System.Collections;
using TMPro;

public class PostgamePlayerLine : MonoBehaviour
{
	public TextMeshPro playerName;
	public SpriteRenderer[] crowns;
	public SpriteRenderer bg, profileBG;
	public ProfilePicture profile;
	public bool isQueen = false;

	void SetColorPreserveAlpha(SpriteRenderer target, Color color)
	{
		target.color = new Color(color.r, color.g, color.b, target.color.a);
	}

	public void OnPostgame(PlayerModel player)
    {
		profile.SetColor(isQueen ? ViewModel.currentTheme.GetTeamTheme(player.teamID).sColor : ViewModel.currentTheme.GetTeamTheme(player.teamID).pColor, player.teamID);

		playerName.text = player.displayNameWithoutTeam;
		var (pic, rotation) = PlayerStaticData.GetProfilePic(player.hivemindID);
		if(pic != null)
        {
			profile.SetPicture(pic, rotation, .55f);
        } else
        {
			profile.SetPicture(SpriteDB.GetIcon(player.teamID, player.positionID), 0, .25f);

		}

		if (bg != null)
			SetColorPreserveAlpha(bg, ViewModel.currentTheme.GetTeamTheme(player.teamID).pColor);
		//if(profileBG != null)
		//	SetColorPreserveAlpha(profileBG, ViewModel.currentTheme.GetTeamTheme(player.teamID).primaryColor);
		for (int i = 0; i < 3; i++)
        {
			crowns[i].enabled = player.curGameStats.queenKills.property > i;
        }
    }
}

