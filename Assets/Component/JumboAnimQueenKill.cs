using UnityEngine;
using System.Collections;
using DG.Tweening;

public class JumboAnimQueenKill : JumboAnim
{
	public Material[] bgMaterials;
	public SpriteRenderer player;
	public SpriteRenderer enemy;
	public SpriteRenderer killSprite;
	public SpriteRenderer hexMask;
	public SpriteRenderer[] bgs;

	static int testID = 0;

    virtual public void SetupAnim(int bgID, PlayerModel playerData)
	{
		//transform.localScale = Vector3.one * .1f;
		var playerName = playerData.displayName;
		var playerSprite = SpriteDB.allSprites[playerData.teamID].playerSprites[playerData.positionID].soldier_idle;
		var playerAttackSprite = SpriteDB.allSprites[playerData.teamID].playerSprites[playerData.positionID].soldier_attack;
		var totalKills = playerData.curSetStats.queenKills.property;
		var enemySprite = SpriteDB.allSprites[1 - playerData.teamID].playerSprites[2].soldier_idle;
		bg.material = bgMaterials[bgID];
		player.sprite = playerSprite;
		enemy.sprite = enemySprite;
		killSprite.transform.localPosition = new Vector3(0f, 1.5f, 0f);
		killSprite.enabled = false;
		hexMask.enabled = false;
		player.transform.localPosition = new Vector3(-20f, 0f, 0f);
		enemy.transform.localPosition = new Vector3(20f, 2.25f, 0f);
		//enemy.transform.localScale = new Vector3(-1f, 1f, 1f);
		titleTxt.text = "";
		subtitleTxt.text = "";
		float invert = (playerData.teamID == UIState.blue ? 1f : -1f);
		//delay postgame until after queen kill anim finishes
		ViewModel.endGameAnimDelay.property = true;

		foreach(var thisBG in bgs)
        {
			thisBG.color = sideID == UIState.blue ? ViewModel.currentTheme.blueTheme.pColor : ViewModel.currentTheme.goldTheme.pColor;
        }
		//player
		DOTween.Sequence()
			.Append(player.transform.DOLocalMoveY(4.5f, .5f).SetEase(Ease.OutQuad))
			.Append(player.transform.DOLocalMoveY(2.25f, .5f).SetEase(Ease.InQuad))
			.AppendCallback(() => player.sprite = playerAttackSprite)
			.AppendInterval(.5f)
			.Append(player.transform.DOLocalMoveX(sideID == 0 ? -11f : 9.5f, .5f).SetEase(Ease.OutQuad));

		player.transform.DOLocalMoveX(-2.5f, 1f).SetEase(Ease.Linear);

		//enemy
		DOTween.Sequence()
			.Append(enemy.transform.DOLocalMoveY(0.5f, .7f).SetEase(Ease.OutQuad))
			.Append(enemy.transform.DOLocalMoveY(1.5f, .3f).SetEase(Ease.InExpo))
			.AppendInterval(.5f)
			.Append(enemy.transform.DOLocalMoveX(sideID == 0 ? -8.5f : 12f, .5f).SetEase(Ease.OutQuad));

		enemy.transform.DOLocalMoveX(0f, 1f).SetEase(Ease.Linear);

		//booboo
		DOTween.Sequence()
			.AppendInterval(1f)
			.AppendCallback(() => killSprite.enabled = true)
			.AppendInterval(.5f)
			.Append(killSprite.transform.DOLocalMoveX(sideID == 0 ? -8.5f : 12f, .5f).SetEase(Ease.OutQuad));

		//text
		DOTween.Sequence()
			.AppendInterval(1.75f)
			.AppendCallback(() =>
			{
				titleTxt.text = "queen\nslain<size=10>\n\nby</size>";
				subtitleTxt.text = playerName.ToLower() + (GameModel.instance.setPoints.property > 0 ? " (" + totalKills + ")" : "");
				titleTxt.color = subtitleTxt.color = new Color(1f, 1f, 1f, 0f);
			})
			.Append(titleTxt.DOColor(Color.white, .5f))
			.Join(subtitleTxt.DOColor(Color.white, .5f));

		//mask
		DOTween.Sequence()
			.AppendInterval(1f)
			.AppendCallback(() => hexMask.enabled = true)
			.AppendInterval(.1f)
			.AppendCallback(() =>
			{
				hexMask.color = new Color(0f, 0f, 0f, 0f);
			})
			.Append(hexMask.DOColor(new Color(0f, 0f, 0f, .5f), .4f).SetEase(Ease.OutQuad))
			.Append(hexMask.transform.DOLocalMoveX(-10.3f * invert, .5f).SetEase(Ease.OutQuad));
	}

    private void OnDestroy()
    {
		ViewModel.endGameAnimDelay.property = false; //when anim ends, we can show end game
    }
}

