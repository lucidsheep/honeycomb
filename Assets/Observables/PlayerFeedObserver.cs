using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class PlayerFeedObserver : KQObserver
{
	public FeedCard[] cardTemplates;
	public int maxCards = 3;
	public float moveAnimTime = .5f;
	public float holdTime = 2f;
	public float fadeOutTime = .25f;
	public float startPos = -2f;
	public float decayRate = .1f;
	public bool observePadding = false;
	public bool fadeIn = false;
	public float yPosition = -7f;

	float padding { get { if (!observePadding) return 0f; return targetID == 0 ? ViewModel.leftSetPadding.property + ViewModel.leftHexPadding.property : ViewModel.rightSetPadding.property + ViewModel.rightHexPadding.property; } }
	protected List<FeedCard> cards = new List<FeedCard>();

	protected FeedCard PushNewCard()
    {
		foreach (var ca in cards)
			PushAnim(ca);
		var card = Instantiate(cardTemplates[targetID], ViewModel.stage);
		card.transform.localPosition = new Vector3(startPos * (targetID == 0 ? -1f : 1f) + padding, yPosition, 0f);
		card.SetTeam(targetID);
		cards.Add(card);
		PushAnim(card);
		return card;
	}
	void PushAnim(FeedCard card)
    {
		card.DOKill(false);
		card.SetPos(card.pos + 1, targetID == 1);
		var seq = DOTween.Sequence()
			.Append(card.transform.DOLocalMoveX((card.pos * card.cardWidth + startPos) * (targetID == 0 ? -1f : 1f) + padding, moveAnimTime).SetEase(Ease.OutCubic));
		if (card.pos == 1 && fadeIn)
			seq.Join(DOTween.To(() => card.alpha.alpha, x => card.alpha.alpha = x, 0f, moveAnimTime).From());
		seq.AppendInterval(holdTime - (card.pos * holdTime * decayRate))
			.OnComplete(() => FinishAnim(card));
    }

	protected void FinishAnim(FeedCard card)
    {
		cards.Remove(card);
		card.transform.DOLocalMoveX((card.pos * card.cardWidth + 5f) * (targetID == 0 ? -1f : 1f) + padding, fadeOutTime).SetEase(Ease.Linear).OnComplete(() => Destroy(card.gameObject));
		DOTween.To(() => card.alpha.alpha, x => card.alpha.alpha = x, 0f, fadeOutTime - .1f);
    }

}

