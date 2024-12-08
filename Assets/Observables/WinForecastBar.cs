using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;
using System;

public class WinForecastBar : KQObserver
{

	public TextMeshPro text;

	public SpriteRenderer centerPoint;

    public float range;
	public bool showSnail = true;
	public SpriteRenderer snailSprite;

    int curWinner = -1;
	bool dirty = true;
	float pct = .5f;
	float prevPct = .5f;

    float rawPct = .5f;
	bool iv = false;
	bool isActive = true;
	bool startAnim = false;
	int forceLit = 2;
	bool forceLitBright = false;

	bool dirtySnail = false;
	bool snailFacingRight = true;
	float snailPos = 0f;

	Tweener winPctAnim;

	void OnWinPercentage(float blueWinPct, GameEventData eventData)
    {
        rawPct = blueWinPct;
		curWinner = Mathf.FloorToInt(blueWinPct * 100f) == 50 ? -1 : (blueWinPct > .5f ? 0 : 1);
		pct = curWinner == -1 ? .5f : curWinner == 0 ? blueWinPct : (1f - blueWinPct);
		if(isActive)
			dirty = true;
    }
	// Use this for initialization
	public override void Start()
	{
		base.Start();
		KQuityManager.onBlueWinProbability.AddListener(OnWinPercentage);
		//KQuityManager.gameInProgress.onChange.AddListener(OnGameStart);
		GameModel.onGameEvent.AddListener(OnGameEvent);
		GameModel.onGameStart.AddListener(OnGameStart);
		SnailModel.onSnailMoveEstimate.AddListener(OnSnailUpdate);

		if(!showSnail && snailSprite != null)
			snailSprite.enabled = false;
		dirty = true;
	}

    private void OnSnailUpdate(float pos)
    {
		var blueWinning = SnailModel.bluePercentage > SnailModel.goldPercentage;
		var pct = blueWinning ? SnailModel.bluePercentage : SnailModel.goldPercentage;
		var dist = range * pct *.01f;
		var prevPos = snailPos;
        snailPos = dist * (blueWinning ? -1f : 1f);
		snailFacingRight = prevPos == snailPos ? snailFacingRight : prevPos < snailPos;
		dirtySnail = true;
    }

    protected override void OnThemeChange()
    {
        base.OnThemeChange();
    }
    public override void OnInvert(bool inverted)
    {
        base.OnInvert(inverted);
		dirty = true;
    }

	void OnGameStart()
    {
		//if (before == after || !after) return;

		if(KQuityManager.instance != null && KQuityManager.instance.useManager)
        {	
			prevPct = pct = .5f;
			curWinner = -1;
        }
		snailPos = 0f;
		dirtySnail = true;
    }

	void OnGameEvent(string type, GameEventData data)
	{

	}
	// Update is called once per frame
	void Update()
	{
		if (dirty)
		{
            var newX = -range + (range * 2f * rawPct);
			var distance = Mathf.Abs(centerPoint.transform.localPosition.x - -newX);
			if(winPctAnim != null && !winPctAnim.IsComplete())
				winPctAnim.Kill();
			winPctAnim = centerPoint.transform.DOLocalMoveX(-newX, distance * (.5f / 2.7f)).SetEase(Ease.OutBack);
			dirty = false;
        }

		if(dirtySnail) //:O
		{
			snailSprite.transform.localPosition = new Vector3(snailPos, .02f, 0f);
			snailSprite.transform.localScale = new Vector3(.75f * (snailFacingRight ? 1f : -1f), 1f, 1f);
			dirtySnail = false; 
		}
	}
}

