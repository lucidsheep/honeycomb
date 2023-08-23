using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class ScoreboardSnailObserver : KQObserver
{
	public TextMeshPro centerText, leftText, rightText;
	public SpriteRenderer snailIcon;
	public SpriteRenderer blueBG, goldBG, neutralBG;
	public SpriteRenderer playPauseIcon;
	public Sprite playSprite, pauseSprite, ffSprite;

	int shownOwner = -1;
	bool takenFromNeutral = false;
	bool returnToNeutral = false;
	bool isSpeed = false;
	bool isMoving = false;
	float returnToNeutralTimeout = 0f;
	bool dirty = false;
	// Use this for initialization
	void Start()
	{
		SnailModel.blueSecondsLeft.onChange.AddListener(OnBlueSeconds);
		SnailModel.goldSecondsLeft.onChange.AddListener(OnGoldSeconds);
		SnailModel.riderTeam.onChange.AddListener(OnSnailTeamChange);
		SnailModel.isSpeedSnail.onChange.AddListener(OnSpeedChange);
		SnailModel.eatInProgress.onChange.AddListener(OnEatChange);
		GameModel.onGameEvent.AddListener(OnGameEvent);
		playPauseIcon.color = new Color(1f, 1f, 1f, 0);
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
	void OnGameEvent(string type, GameEventData data)
    {
		if (type == GameEventType.SPAWN && data.playerID == 2 && data.teamID == 1)
        {
			blueBG.color = goldBG.color = playPauseIcon.color = new Color(1f, 1f, 1f, 0f);
			shownOwner = -1;
			returnToNeutralTimeout = -1f;
			isSpeed = false;
			isMoving = false;
			dirty = true;
			returnToNeutral = true;
        }

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
			returnToNeutralTimeout = -1f;
			isMoving = true;
        } else if(after == -1)
        {
			returnToNeutralTimeout = 5f;
			isMoving = false;
			isSpeed = false;
        }
		dirty = true;
    }

	void SetTime()
    {
		if(takenFromNeutral)
        {
			takenFromNeutral = false;
			var snailX = shownOwner == UIState.blue ? -1f : 1f;
			snailIcon.DOKill();
			snailIcon.transform.DOLocalMove(new Vector3(snailX, -.75f, 0f), .25f).SetEase(Ease.InOutQuad);
			snailIcon.transform.DORotate(new Vector3(0f, snailX < 0f ? 180f : 0f, 0f), .25f).SetEase(Ease.Linear);

			leftText.DOColor(new Color(1f, 1f, 1f, 0f), .25f);
			rightText.DOColor(new Color(1f, 1f, 1f, 0f), .25f);
			centerText.DOColor(Color.white, .25f);
			playPauseIcon.DOColor(Color.white, .25f);

			var bgToUse = shownOwner == UIState.blue ? blueBG : goldBG;
			var bgToFade = shownOwner == UIState.blue ? goldBG : blueBG;

			bgToUse.DOKill();
			bgToFade.DOKill();
			bgToUse.DOColor(Color.white, .25f);
			bgToFade.DOColor(new Color(1f, 1f, 1f, 0f), .25f);

		} else if(returnToNeutral)
        {
			returnToNeutral = false;
			snailIcon.transform.DOLocalMove(new Vector3(0f, -1.38f, 0f), .25f).SetEase(Ease.InOutQuad);
			leftText.DOColor(new Color(1f, 1f, 1f, .5f), .25f);
			rightText.DOColor(new Color(1f, 1f, 1f, .5f), .25f);
			centerText.DOColor(new Color(1f,1f,1f,0f), .25f);

			var blueIsWinning = SnailModel.blueSecondsLeft.property < SnailModel.goldSecondsLeft.property;
			if (SnailModel.blueSecondsLeft.property == SnailModel.goldSecondsLeft.property)
				blueIsWinning = shownOwner == 0; //if we're tied, retain the last owner as the winner
			var bgToUse = blueIsWinning ? blueBG : goldBG;
			var bgToFade = blueIsWinning ? goldBG : blueBG;

			if(shownOwner > -1)
				bgToUse.DOColor(Color.white, .25f);

			bgToFade.DOColor(new Color(1f, 1f, 1f, 0f), .25f);
			playPauseIcon.DOColor(new Color(1f, 1f, 1f, 0f), .25f);
		}
		string blueText = Util.FormatTime((float)SnailModel.blueSecondsLeft.property);
		string goldText = Util.FormatTime((float)SnailModel.goldSecondsLeft.property);
		if (shownOwner == 0)
			centerText.text = blueText;
		else if (shownOwner == 1)
			centerText.text = goldText;

		leftText.text = UIState.inverted ? goldText : blueText;
		rightText.text = UIState.inverted ? blueText : goldText;
		playPauseIcon.sprite = isMoving ? (isSpeed ? ffSprite : playSprite) : pauseSprite;
	}
	// Update is called once per frame
	void Update()
	{
		if(dirty)
        {
			dirty = false;
			SetTime();
        } else if(returnToNeutralTimeout > 0f)
        {
			returnToNeutralTimeout -= Time.deltaTime;
			if(returnToNeutralTimeout <= 0f)
            {
				returnToNeutral = true;
				SetTime();
            }
        }
	}
}

