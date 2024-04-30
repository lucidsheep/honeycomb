using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ScoreboardQueenObserver : MonoBehaviour
{
	public Crown crownTemplate;
	public Color blueFaded, goldFaded, centerFaded, blueFilled, goldFilled;
	public Crown[] staticCrowns;

	float[] outerPointPositions = new float[]{ 5.71f, 3.23f };
	int[] queenPoints = new int[] { 0, 0 };
	// Use this for initialization
	void Start()
	{
		for (var i = 0; i < 2; i++)
		{
			foreach (var p in GameModel.instance.teams[i].players)
			{
				var copiedIndex = i; //this is needed to capture i and pass into funcs as value and not reference
				p.curGameStats.queenKills.onChange.AddListener((b, a) => { OnQueenKill(copiedIndex, a - b); });
			}
		}
		GameModel.onGameStart.AddListener(OnGameStart);
		OnGameStart();
	}

	void OnGameStart()
    {
		Color blue = UIState.inverted ? goldFaded : blueFaded;
		Color gold = UIState.inverted ? blueFaded : goldFaded;
		queenPoints = new int[] { 0, 0 };
		for (int i = 0; i < 5; i++)
			staticCrowns[i].SetCrown(i < 2 ? UIState.blue : i > 2 ? UIState.gold : -1, false, i < 2 ? blue : i == 2 ? centerFaded : gold);
    }
	void OnQueenKill(int teamID, int score)
	{
		queenPoints[teamID] += score;
		if (score <= 0 || queenPoints[teamID] > 3) return;

		var staticCrown = teamID == UIState.blue ? staticCrowns[queenPoints[teamID] - 1] : staticCrowns[5 - queenPoints[teamID]];
		if (staticCrown == null || staticCrown.main == null)
			return; //todo - why
		var isBlue = teamID == UIState.blue;
		var isCenter = queenPoints[teamID] == 3;
		Color blue = UIState.inverted ? goldFaded : blueFaded;
		Color gold = UIState.inverted ? blueFaded : goldFaded;
		if (!ViewModel.currentTheme.showCrownAnimation)
		{
			staticCrown.SetCrown(teamID, true, isBlue ? blueFilled : goldFilled);
			//staticCrown.main.color = isCenter ? (isBlue ? blue : gold) : (isBlue ? blueFilled : goldFilled);
			return;
		}
		var anim = Instantiate(crownTemplate, ViewModel.stage);
		var xPos = 0f;
		var finalScale = .61f;
		var finalScaleStatic = .26f;
		var finalY = -1.37f;

		
		if (queenPoints[teamID] < 3 && queenPoints[teamID] > 0)
			xPos = outerPointPositions[queenPoints[teamID] - 1] * (teamID == 0 ? -1f : 1f) * UIState.invertf;
		if (isCenter)
		{
			finalScale = .9f;
			finalY = .9f;
			finalScaleStatic = .35f;
		}
		anim.main.color = anim.outline.color = new Color(1f, 1f, 1f, 0f); // isBlue ? gold : blue;
		anim.transform.localPosition = new Vector3(13f * UIState.invertf * (teamID == 0 ? -1f : 1f), -1.0f, 0f);
		anim.transform.localScale = Vector3.one * .61f;

		DOTween.Sequence()
			.AppendInterval(1f)
			.AppendCallback(() => { anim.main.color = anim.outline.color = Color.white; })
			.AppendInterval(.5f)
			.Append(anim.transform.DOLocalMoveX(xPos + ((anim.transform.localPosition.x - xPos) / 2f), .5f).SetEase(Ease.OutQuad))
			//.Join(anim.transform.DOLocalMoveY(4f, .5f).SetEase(Ease.OutCirc))
			.Join(anim.transform.DOScale(2.5f, .5f).SetEase(Ease.OutQuad))
			.Append(anim.transform.DOLocalMoveX(xPos, .5f).SetEase(Ease.InQuad))
			.Join(anim.transform.DOLocalMoveY(finalY, .5f).SetEase(Ease.InQuad))
			.Join(anim.transform.DOScale(finalScale, .5f).SetEase(Ease.InQuad))
			.Join(anim.main.DOColor(isCenter ? (isBlue ? blue : gold) : Color.white, .5f))
			//.Join(anim.outline.DOColor(isBlue ? blue : gold, .5f))
			.AppendCallback(() => { staticCrown.main.color = isCenter ? (isBlue ? blue : gold) : (isBlue ? blueFilled : goldFilled); Destroy(anim.gameObject); })
			.Append(staticCrown.transform.DOScale(finalScaleStatic * .5f, .25f).SetEase(Ease.OutQuad))
			.Append(staticCrown.transform.DOScale(finalScaleStatic * 1.25f, .4f).SetEase(Ease.InOutQuad))
			.Append(staticCrown.transform.DOScale(finalScaleStatic * 1f, .25f).SetEase(Ease.InQuad));


	}
	// Update is called once per frame
	void Update()
	{
			
	}
}

