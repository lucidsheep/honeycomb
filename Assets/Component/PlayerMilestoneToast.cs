using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class PlayerMilestoneToast : MonoBehaviour
{
	public TextMeshPro titleTxt, descriptionTxt, timeTxt;
	public SpriteRenderer icon;

	GlobalFade alpha;

	public void Init(PlayerModel player, PlayerModel.Milestone milestone, int side)
    {
		alpha = GetComponent<GlobalFade>();
		bool isQueenMS = milestone.statType == PlayerModel.StatValueType.QueenKills; //stay for extra time to wait for queen kill anim to finish
		transform.localPosition = new Vector3(9.5f * side, -4.222f, 0f);
		titleTxt.text = player.displayName;
		descriptionTxt.text = milestone.description;
		timeTxt.text = TimeScaleToText(milestone.timescale);
		icon.sprite = SpriteDB.allSprites[player.teamID].playerSprites[player.positionID].icon;
		DOTween.Sequence()
			.Append(transform.DOLocalMoveX(20f * side, .5f))
			.Join(DOTween.To(() => alpha.alpha, x => alpha.alpha = x, 0f, .25f).From())
			.AppendInterval(isQueenMS ? 10f : 5f)
			.Append(transform.DOLocalMoveX(9.5f * side, .5f))
			.Join(DOTween.To(() => alpha.alpha, x => alpha.alpha = x, 0f, .25f))
			.OnComplete(() => Destroy(this.gameObject));
    }

	static string TimeScaleToText(PlayerModel.StatTimescale timescale)
    {
		switch(timescale)
        {
			case PlayerModel.StatTimescale.Life: return "this life";
			case PlayerModel.StatTimescale.Game: return "this game";
			case PlayerModel.StatTimescale.Set: return "this set";
			case PlayerModel.StatTimescale.Tournament: return "today";
			case PlayerModel.StatTimescale.Career: return "career milestone";
			default: return "";
        }
    }
}

