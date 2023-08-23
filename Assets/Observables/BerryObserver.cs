using UnityEngine;
using System.Collections;
using TMPro;

public class BerryObserver : KQObserver
{
	public SpriteRenderer berryIcon;
	public TextMeshPro berryLabel;
	public Color berryWarningColor = Color.red;

	bool dirty = false;
	int berryCount = 0;

	// Use this for initialization
	override public void Start()
	{
		base.Start();
		GameModel.instance.berriesLeft.onChange.AddListener(OnChange);
	}

	// Update is called once per frame
	void Update()
	{
		if(dirty)
        {
			berryLabel.text = berryCount.ToString();
			berryLabel.color = berryIcon.color = berryCount < 10 && GameModel.instance.gameIsRunning.property ? berryWarningColor : Color.white;
			dirty = false;
		}
	}

    public void OnChange(int before, int after)
    {
		dirty = true;
		berryCount = after;
	}
}

