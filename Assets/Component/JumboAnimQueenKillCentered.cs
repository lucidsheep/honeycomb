using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

public class JumboAnimQueenKillCentered : JumboAnimQueenKill
{
    public override void StartAnim()
    {
		//transform.localPosition = new Vector3(5.6f, -6.37f, 0f);
		//hack
		transform.localPosition = new Vector3(.79f, -.36f, 0f);
		transform.localScale = Vector3.one * .14f;
	}

    public override void SetupAnim(int bgID, PlayerModel playerData)
    {
		var playerName = playerData.displayName;
		var playerSprite = SpriteDB.allSprites[playerData.teamID].playerSprites[playerData.positionID].soldier_idle;
		var playerAttackSprite = SpriteDB.allSprites[playerData.teamID].playerSprites[playerData.positionID].soldier_attack;
		var totalKills = playerData.curSetStats.queenKills.property;
		var enemySprite = SpriteDB.allSprites[1 - playerData.teamID].playerSprites[2].soldier_idle;
		bool blueTeam = playerData.teamID == 0;
		bg.material = bgMaterials[bgID];
		player.sprite = playerSprite;
		enemy.sprite = enemySprite;
		killSprite.transform.localPosition = new Vector3(-3.44f, 0.5f, 0f);
		killSprite.enabled = false;
		hexMask.enabled = false;
		player.transform.localPosition = new Vector3(-20f, 0.0f, 0f);
		enemy.transform.localPosition = new Vector3(20f, 2.25f, 0f);
		//enemy.transform.localScale = new Vector3(-1f, 1f, 1f);
		titleTxt.text = "";
		subtitleTxt.text = "";
		float invert = (playerData.teamID == UIState.blue ? 1f : -1f);
		//delay postgame until after queen kill anim finishes
		ViewModel.endGameAnimDelay.property = true;

		//player
		DOTween.Sequence()
			.Append(player.transform.DOLocalMoveY(4.5f, .5f).SetEase(Ease.OutQuad))
			.Append(player.transform.DOLocalMoveY(2.25f, .5f).SetEase(Ease.InQuad))
			.AppendCallback(() => player.sprite = playerAttackSprite)
			.AppendInterval(.5f)
			.Append(player.transform.DOLocalMoveX(blueTeam ? -11.5f : -2.14f, .5f).SetEase(Ease.OutQuad));

		player.transform.DOLocalMoveX(-6.84f, 1f).SetEase(Ease.Linear);

		//enemy
		DOTween.Sequence()
			.Append(enemy.transform.DOLocalMoveY(0.5f, .7f).SetEase(Ease.OutQuad))
			.Append(enemy.transform.DOLocalMoveY(1.5f, .3f).SetEase(Ease.InExpo))
			.AppendInterval(.5f)
			.Append(enemy.transform.DOLocalMoveX(blueTeam ? -8.3f : 1.24f, .5f).SetEase(Ease.OutQuad));

		enemy.transform.DOLocalMoveX(-3.44f, 1f).SetEase(Ease.Linear);

		//booboo
		DOTween.Sequence()
			.AppendInterval(1f)
			.AppendCallback(() => killSprite.enabled = true)
			.AppendInterval(.5f)
			.Append(killSprite.transform.DOLocalMoveX(blueTeam ? -8.3f : 1.24f, .5f).SetEase(Ease.OutQuad));

		//text
		DOTween.Sequence()
			.AppendInterval(1.75f)
			.AppendCallback(() =>
			{
				titleTxt.text = "queen\nslain<size=8>\n\nby</size>";
				subtitleTxt.text = playerName.ToLower() + (GameModel.instance.setPoints.property > 0 ? " (" + totalKills + ")" : "");
				titleTxt.color = subtitleTxt.color = new Color(1f, 1f, 1f, 0f);
			})
			.Append(titleTxt.DOColor(Color.white, .5f))
			.Join(subtitleTxt.DOColor(Color.white, .5f));
		titleTxt.transform.localPosition = new Vector3((blueTeam ? -4.91f : -10.17f), 2.02f, 0f);
		subtitleTxt.transform.localPosition = new Vector3((blueTeam ? -5.05f : -10f), -2.88f, 0f);

		//mask
		DOTween.Sequence()
			.AppendInterval(1f)
			.AppendCallback(() => hexMask.enabled = true)
			.AppendInterval(.1f)
			.AppendCallback(() =>
			{
				hexMask.color = new Color(0f, 0f, 0f, 0f);
			})
			.Append(hexMask.DOColor(new Color(0f, 0f, 0f, 1f), .4f).SetEase(Ease.OutQuad))
			.Append(hexMask.transform.DOLocalMoveX(blueTeam ? -10.12f : -0.95f, .5f).SetEase(Ease.OutQuad));
	}
}

