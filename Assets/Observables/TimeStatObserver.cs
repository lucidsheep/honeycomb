using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class TimeStatObserver : KQObserver
{
	public TextMeshPro timeLabel;

	// Use this for initialization
	void Start()
	{
		GameModel.instance.gameTime.onChange.AddListener(OnGameTimeUpdate);
		//ViewModel.endGameAnimDelay.onChange.AddListener((b, a) => { if (b != a && a == false) SetState(false); });
	}

	void OnGameTimeUpdate(float before, float after)
    {
		timeLabel.text = Util.FormatTime(after);
    }

}

