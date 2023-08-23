using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class SecondaryStatsObserver : KQObserver
{
	public TextMeshPro warriorText, speedText, speedWarriorText;

	int curSpeed = 0;
	int curWarriors = 0;
	int curSpeedWarriors = 0;

	bool[] speedPowerups = new bool[] { false, false, false, false, false };
	bool[] swordPowerups = new bool[] { false, false, false, false, false };
	Sequence delayReopenAnim;

	// Use this for initialization
	void Start()
	{
		GameModel.onGameEvent.AddListener(OnGameEvent);
		GameModel.onGameModelComplete.AddListener(OnGameEnd);
		//ViewModel.endGameAnimDelay.onChange.AddListener((b, a) => { if (b != a && a == false) SetState(false); });

		for(int i = 0; i < 2; i++)
        {
			int thisTeam = i;
			foreach(var p in GameModel.instance.teams[i].players)
            {
				if (p.positionID != 2) //do not count "swords" from queen
				{
					p.curLifeStats.swordObtained.onChange.AddListener((b, a) =>
					{
						if (thisTeam == team && b != a)
						{
							swordPowerups[p.positionID] = a >= 1;
							UpdateCounts();
						}
					});
					p.curLifeStats.speedObtained.onChange.AddListener((b, a) => {
						if (thisTeam == team && b != a)
						{
							speedPowerups[p.positionID] = a >= 1;
							UpdateCounts();
						}
					});
				}
            }
        }
	}

	void OnGameEnd(int _, string __)
    {
		/*
		readyToClose = true;
		if (delayReopenAnim != null && !delayReopenAnim.IsComplete())
			delayReopenAnim.Kill();
		if (ViewModel.endGameAnimDelay.property == false)
			SetState(false);
		*/
    }

	void UpdateCounts()
        {
			curSpeed = curWarriors = curSpeedWarriors = 0;
			for(int i = 0; i < 5; i++)
			{
				if (swordPowerups[i] == speedPowerups[i] && speedPowerups[i] == true)
					curSpeedWarriors++;
				else if (swordPowerups[i] == true)
					curWarriors++;
				else if (speedPowerups[i] == true)
					curSpeed++;
            }
		if(speedText != null)
			speedText.text = curSpeed.ToString();
		if(warriorText != null)
			warriorText.text = curWarriors.ToString();
		if(speedWarriorText != null)
			speedWarriorText.text = curSpeedWarriors.ToString();
        }

	void OnGameEvent(string type, GameEventData data)
    {
		if (type == GameEventType.SPAWN && data.playerID == 2 && data.teamID == 1)
		{
			speedPowerups = new bool[] { false, false, false, false, false };
			swordPowerups = new bool[] { false, false, false, false, false };
			UpdateCounts();
		} 
    }

	void SetState(bool toOpen)
    {
		/*
		if (toOpen == isOpen) return;
		if (!toOpen && !readyToClose) return;
		isOpen = toOpen;
		readyToClose = false;

		if (isOpen) SetVisible(true);
		if(!isOpen)
        {
			curWarriors = 0;
			gateText.text = warriorText.text = "0";
        }
		transform.DOKill();
		transform.DOLocalMoveX(toOpen ? 0f : (3.5f * (targetID == 0 ? 1f : -1f)), (toOpen ? 1f : .25f)).SetEase(toOpen ? Ease.OutBack : Ease.Linear)
			.OnComplete(() => SetVisible(isOpen));
		LSProperty<float> padding = targetID == 0 ? ViewModel.leftHexPadding : ViewModel.rightHexPadding;
		DOTween.To(() => padding.property, x => padding.property = x, !toOpen ? 0f : (targetID == 0 ? -3.5f : 3.5f), (toOpen ? 1f : .25f)).SetEase(toOpen ? Ease.OutBack : Ease.Linear);
		*/
	}

	void SetVisible(bool val)
    {
		GetComponent<GlobalFade>().alpha = val ? 1f : 0f;
    }
	// Update is called once per frame
	void Update()
	{
			
	}
}

