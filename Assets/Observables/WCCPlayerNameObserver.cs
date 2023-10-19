using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class WCCPlayerNameObserver : KQObserver
{
	public WCCPlayerNameDisplay[] playerSlots;

	TournamentTeamData presetTeamData;
	bool dirty = false;
	bool presetMode = false;

	bool onOff = false;
	// Use this for initialization
	public override void Start()
	{
		base.Start();
		for (int p = 0; p < 5; p++)
		{
			GameModel.instance.teams[targetID].players[p].playerName.onChange.AddListener((b, a) => dirty = true);
		}
		PlayerStaticData.onPlayerData.AddListener(_ => dirty = true);
		dirty = true;
	}

	// Update is called once per frame
	void Update()
	{
		if (dirty)
		{
			dirty = false;
			for (int j = 0; j < 5; j++)
			{
				//force queen to display first
				int i = j;
				if (j == 0) i = 2;
				else if (j <= 2) i = j - 1;

				var id = GameModel.instance.teams[targetID].players[i].hivemindID;
				playerSlots[j].SetDisplay(GameModel.instance.teams[targetID].players[i].playerName.property, PlayerStaticData.GetSceneTag(id), PlayerStaticData.GetPronouns(id));
			}
		}
	}
}

