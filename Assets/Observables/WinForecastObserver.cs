using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class WinForecastObserver : KQObserver
{
	public TextMeshPro text;
	public SpriteRenderer[] blueArrows;
	public SpriteRenderer[] goldArrows;
	public SpriteRenderer logo;

	bool dirty = true;
	float pct = .5f;
	float prevPct = .5f;
	int prevWinner = -1; //-1 = no one, 0 = blue, 1 = gold
	int curWinner = -1;
	bool iv = false;
	bool isActive = false;
	bool startAnim = false;
	int forceLit = 2;
	bool forceLitBright = false;

	void OnWinPercentage(float blueWinPct, GameEventData eventData)
    {
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
		var maybeLogo = AppLoader.GetStreamingSprite("centerLogo");
		if (maybeLogo != null)
			logo.sprite = maybeLogo;
    }
    public override void OnInvert(bool inverted)
    {
        base.OnInvert(inverted);
		dirty = true;
		if (iv != inverted && prevWinner != -1)
			prevWinner = 1 - prevWinner;
		iv = inverted;
    }

	void OnGameStart()
    {
		//if (before == after || !after) return;

		if(KQuityManager.instance != null && KQuityManager.instance.useManager)
        {	
			prevPct = pct = .5f;
			prevWinner = curWinner = -1;
			forceLit = 2;
			forceLitBright = true;
			dirty = true;
			DOTween.Sequence()
				.AppendInterval(5.8f)
				.AppendCallback(() => { forceLit = 1; dirty = true; })
				.AppendInterval(1f)
				.AppendCallback(() => { forceLit = 0; dirty = true; })
				.AppendInterval(1f)
				.AppendCallback(() => { forceLit = -1; dirty = true; })
				.AppendInterval(1f)
				.Append(AnimateSR(logo, Color.black))
				.AppendCallback(() => isActive = dirty = true);

		}
    }

	void OnGameEvent(string type, GameEventData data)
	{
		if (type == GameEventType.GAME_END_DETAIL)
		{
			//blueWinning = data.teamID == 0;
			isActive = false;
			AnimateSR(logo, Color.white);
			dirty = true;
		}
	}
	// Update is called once per frame
	void Update()
	{
		if (dirty)
		{
			var blueTheme = UIState.inverted ? ViewModel.currentTheme.goldTheme : ViewModel.currentTheme.blueTheme;
			var goldTheme = UIState.inverted ? ViewModel.currentTheme.blueTheme : ViewModel.currentTheme.goldTheme;
			bool isLeftWinning = !UIState.inverted ? curWinner == 0 : curWinner == 1;
			bool wasLeftWinning = !UIState.inverted ? prevWinner == 0 : prevWinner == 1;
			bool isRightWinning = !UIState.inverted ? curWinner == 1 : curWinner == 0;
			bool wasRightWinning = !UIState.inverted ? prevWinner == 1 : prevWinner == 0;

			for (int side = 0; side < 2; side++)
			{
				var theme = side == 0 ? blueTheme : goldTheme;
				bool isWinning = side == 0 ? isLeftWinning : isRightWinning;
				bool wasWinning = side == 0 ? wasLeftWinning : wasRightWinning;
				var arrows = side == 0 ? blueArrows : goldArrows;
				for (int i = 0; i < blueArrows.Length; i++)
				{
					if (forceLit >= i)
						AnimateSR(arrows[i], forceLitBright ? theme.iColor : theme.pColor);
					else if (!isWinning)
						AnimateSR(arrows[i], theme.sColor);
					else
					{
						bool startedWinning = !wasWinning && isWinning;
						if (i == 0 && startedWinning)
							AnimateSR(arrows[i], theme.iColor);
						else if(i != 0)
						{
							float threshold = i == 1 ? .65f : .8f;
							if ((prevPct < threshold && pct >= threshold) || (startedWinning && pct >= threshold))
							{
								AnimateSR(arrows[i], theme.iColor);
							}
							else if (prevPct >= threshold && pct < threshold)
							{
								AnimateSR(arrows[i], theme.sColor);
							}

						}

					}
				}
			}
			text.text = Mathf.FloorToInt(pct * 100f) + "%";
			if (!isActive)
			{
				text.text = "";
			}
			prevPct = pct;
			prevWinner = curWinner;
			dirty = false;
        }
	}

	Tweener AnimateSR(SpriteRenderer sr, Color destColor)
    {
		sr.DOComplete();
		return sr.DOColor(destColor, .5f).SetEase(Ease.OutBack);
    }
}

