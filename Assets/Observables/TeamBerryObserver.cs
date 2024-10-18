using UnityEngine;
using System.Collections;
using TMPro;

public class TeamBerryObserver : KQObserver
{
	public TextMeshPro berryLabel;


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
			berryLabel.text = berryCount.ToString() + "/" + MapDB.currentMap.property.berries_to_win;
			dirty = false;
		}
	}

    public void OnChange(int before, int after)
    {
		dirty = true;
		berryCount = GameModel.instance.teams[targetID].GetBerryScore();
	}
}

