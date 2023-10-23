using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using DG.Tweening;

public class JumboAnimQueenKillWCC : JumboAnimQueenKill
{
	public VideoClip[] clips;
	public VideoPlayer animPlayer;

	override public void SetupAnim(int bgID, PlayerModel playerData)
	{
		var isBlue = sideID == 0;
		var killerName = Util.SmartTruncate(playerData.displayName.ToUpper(), 12);
		var killedName = Util.SmartTruncate(GameModel.GetPlayer(1 - playerData.teamID, 2).displayName.ToUpper(), 12);
		if (isBlue)
        {
			titleTxt.text = "<color=#00befe>" + killerName + "</color>\nKILLS\n<color=#f3bd00>" + killedName;
        } else
        {
			titleTxt.text = "<color=#f3bd00>" + killerName + "</color>\nKILLS\n<color=#00befe>" + killedName;
		}
		animPlayer.clip = clips[playerData.positionID];
		animPlayer.time = 0f;
		animPlayer.Play();

	}

	override public void StartAnim()
	{
		var actualAnimTime = animTime - 1f;
		transform.localScale = Vector3.one * .15f;
		transform.localPosition = new Vector3(0f, -0.4f, 0f);
		var xMove = 1.3f;
		DOTween.Sequence()
			.Append(transform.DOLocalMoveX(sideID == 0 ? -xMove : xMove, .5f).SetEase(Ease.OutBack))
			//.Join(DOTween.To(() => alpha.alpha, x => alpha.alpha = x, 0f, .25f).From())
			.AppendInterval(actualAnimTime)
			.Append(transform.DOLocalMoveX(0f, .5f).SetEase(Ease.InBack));
			//.Join(DOTween.To(() => alpha.alpha, x => alpha.alpha = x, 0f, .25f))
		//.AppendCallback(() => Destroy(this.gameObject));

	}

	private void OnDestroy()
	{
		ViewModel.endGameAnimDelay.property = false; //when anim ends, we can show end game
	}
}

