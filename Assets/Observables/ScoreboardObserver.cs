using UnityEngine;
using System.Collections;
using TMPro;

public class ScoreboardObserver : KQObserver
{
	public LSProperty<int> queenCount = new LSProperty<int>(0);
	public LSProperty<int> berryCount = new LSProperty<int>(0);
	public LSProperty<int> snailCount = new LSProperty<int>(0);

	bool dirty = false;
	public TextMeshPro txt;
	public bool usePercent = true;
	string snailText = "--";

	// Use this for initialization
	void Start()
	{
		for (var i = 0; i < 2; i++)
		{
			foreach (var p in GameModel.instance.teams[i].players)
			{
				var copiedIndex = i; //this is needed to capture i and pass into funcs as value and not reference
				p.curGameStats.berriesDeposited.onChange.AddListener((b, a) => { if (team == copiedIndex) OnBerry(b, a); });
				p.curGameStats.berriesKicked.onChange.AddListener((b, a) => { if (team == copiedIndex) OnBerry(b, a); });
				p.curGameStats.berriesKicked_OtherTeam.onChange.AddListener((b, a) => { if (team != copiedIndex) OnBerry(b, a); });
				p.curGameStats.queenKills.onChange.AddListener((b, a) => { if (team == copiedIndex) OnQueenKill(b, a); });
			}
		}
		SnailModel.bluePercentage.onChange.AddListener((b, a) => { if (team == 0) OnSnailPercentage(b, a); });
		SnailModel.goldPercentage.onChange.AddListener((b, a) => { if (team == 1) OnSnailPercentage(b, a); });
	}

	void OnBerry(int before, int after)
    {
		berryCount.property = berryCount.property + (after - before);
		UpdateLabel();
    }

	void OnQueenKill(int before, int after)
    {
		queenCount.property = queenCount.property + (after - before);
		UpdateLabel();
    }
	void OnSnailPercentage(int before, int after)
    {
		snailCount.property = after;
		snailText = after <= 0 ? "--" : usePercent ? (after + "%") : after.ToString();
		UpdateLabel();
    }
	void UpdateLabel()
    {
		dirty = true;
    }

    private void Update()
    {
        if(dirty)
        {
			txt.SetText(snailText +"\n" + berryCount.property.ToString());
			dirty = false;
		}
    }
}

