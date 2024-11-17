using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class WinForecastBar : KQObserver
{

	public TextMeshPro text;

	public SpriteRenderer centerPoint;

    public float range;

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

		dirty = true;
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
            centerPoint.transform.localPosition = new Vector3(-newX, 0.02f, 0f);
			dirty = false;
        }
	}
}

