using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class ScoreboardSnailPercentObserver : KQObserver
{
	public TextMeshPro centerText;
	public SpriteRenderer snailIcon;
	public SpriteRenderer blueBG, goldBG, neutralBG;

	int shownOwner = -1;
	bool takenFromNeutral = false;
	bool returnToNeutral = false;
	bool isSpeed = false;
	bool isMoving = false;
	bool dirty = false;
	// Use this for initialization
	void Start()
	{
		SnailModel.bluePercentage.onChange.AddListener(OnBlueSeconds);
		SnailModel.goldPercentage.onChange.AddListener(OnGoldSeconds);
		SnailModel.riderTeam.onChange.AddListener(OnSnailTeamChange);
		SnailModel.isSpeedSnail.onChange.AddListener(OnSpeedChange);
		SnailModel.eatInProgress.onChange.AddListener(OnEatChange);
		GameModel.onGameStart.AddListener(OnGameStart);
	}

	void OnSpeedChange(bool before, bool after)
	{
		if (before == after) return;
		isSpeed = after;
		dirty = true;
	}
	void OnEatChange(bool before, bool after)
	{
		if (before == after) return;

		isMoving = after ? false : shownOwner > -1 ? true : false;
		dirty = true;
	}
	void OnGameStart()
	{
		var c = blueBG.color;
		c.a = 0f;
		blueBG.color = c;
		c = goldBG.color;
		c.a = 0f;
		goldBG.color = c;
		shownOwner = -1;
		isSpeed = false;
		isMoving = false;
		dirty = true;
		returnToNeutral = true;
	}

	void OnBlueSeconds(int before, int after)
	{
		dirty = true;
	}

	void OnGoldSeconds(int before, int after)
	{
		dirty = true;
	}

	void OnSnailTeamChange(int before, int after)
	{
		if (after != -1 && before == -1)
		{
			shownOwner = after;
			takenFromNeutral = true;
			isMoving = true;
		}
		else if (after == -1)
		{
			isMoving = false;
			isSpeed = false;
		}
		dirty = true;
	}

	void SetPercent()
	{
		var bluePercent = SnailModel.bluePercentage;
		var goldPercent = SnailModel.goldPercentage;
		var winningTeam = bluePercent == goldPercent ? -1 : bluePercent > goldPercent ? 0 : 1;

		var bgToUse = winningTeam == 0 ? blueBG : goldBG;
		if(winningTeam > -1 && bgToUse.color.a == 0f)
        {
			var bgToFade = winningTeam == 0 ? goldBG : blueBG;
			bgToUse.DOKill();
			bgToFade.DOKill();
			bgToUse.DOColor(new Color(bgToUse.color.r, bgToUse.color.g, bgToUse.color.b, 1f), .25f);
			bgToFade.DOColor(new Color(bgToFade.color.r, bgToFade.color.g, bgToFade.color.b, 0f), .25f);
		}
		if (takenFromNeutral)
		{
			takenFromNeutral = false;
			var snailX = shownOwner == UIState.blue ? -1f : 1f;
			snailIcon.DOKill();
			snailIcon.transform.DOLocalMove(new Vector3(snailX, -.75f, 0f), .25f).SetEase(Ease.InOutQuad);
			snailIcon.transform.DORotate(new Vector3(0f, snailX < 0f ? 180f : 0f, 0f), .25f).SetEase(Ease.Linear);

			centerText.DOColor(Color.white, .25f);

		}
		else if (returnToNeutral)
		{
			returnToNeutral = false;
			snailIcon.transform.DOLocalMove(new Vector3(0f, -1.38f, 0f), .25f).SetEase(Ease.InOutQuad);
			centerText.DOColor(new Color(1f, 1f, 1f, 0f), .25f);

			blueBG.DOColor(new Color(blueBG.color.r, blueBG.color.g, blueBG.color.b, 0f), .25f);
			goldBG.DOColor(new Color(goldBG.color.r, goldBG.color.g, goldBG.color.b, 0f), .25f);

		}
		centerText.text = winningTeam == 0 ? bluePercent + "%" : goldPercent + "%";
	}
	// Update is called once per frame
	void Update()
	{
		if (dirty)
		{
			dirty = false;
			SetPercent();
		}
	}
}

