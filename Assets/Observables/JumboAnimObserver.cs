using UnityEngine;
using System.Collections;

public class JumboAnimObserver : KQObserver
{
	public JumboAnimQueenKill queenKillAnim;

	JumboAnim curAnim;
	float animTimeLeft = 0f;

	// Use this for initialization
	void Start()
	{
		GameModel.onGameEvent.AddListener(OnGameEvent);
	}


	void OnGameEvent(string type, GameEventData data)
    {
		if (GameModel.instance.isWarmup.property) return;

		if (type == GameEventType.PLAYER_KILL && data.targetID == 2 && (targetID == data.teamID || targetID == -1))
        {
			var anim = StartAnim(queenKillAnim);
			if (anim == null) return; //should not play anim
			int mapID = 0;
			switch(MapDB.currentMap.property.name)
            {
				case "map_day": mapID = 0; break;
				case "map_night": mapID = 1; break;
				case "map_dusk": mapID = 2; break;
				case "map_twilight": case "map_twilight2": mapID = 3; break;
            }
			anim.SetupAnim(mapID, GameModel.instance.teams[data.teamID].players[data.playerID]);
			anim.StartAnim();
        }
    }

	T StartAnim<T>(T animTemplate) where T : JumboAnim
    {
		if (curAnim != null && curAnim.priority < animTemplate.priority) return null; //only overwrite anim if higher priority
		if(curAnim != null)
        {
			Destroy(curAnim.gameObject);
        }
		var newAnim = Instantiate(animTemplate, ViewModel.stage);
		newAnim.sideID = targetID;
		curAnim = newAnim;
		animTimeLeft = newAnim.animTime;
		//ViewModel.hideSetPoints[targetID].property += 1;
		return newAnim;
    }

    private void Update()
    {
        if(animTimeLeft > 0f)
        {
			animTimeLeft -= Time.deltaTime;
			if(animTimeLeft <= 0f && curAnim != null)
            {
				Destroy(curAnim.gameObject);
				curAnim = null;
				//ViewModel.hideSetPoints[targetID].property -= 1;
            }
        }
    }
}

