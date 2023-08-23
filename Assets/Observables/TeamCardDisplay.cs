using UnityEngine;
using System.Collections;

public class TeamCardDisplay : PlayerFeedObserver
{
	// Use this for initialization
	virtual protected void Start()
	{
		NetworkManager.instance.gameEventDispatcher.AddListener(OnGameEvent);
		for(int t = 0; t < 2; t++)
        {
			for(int p = 0; p < 5; p++)
            {
				var varTeam = t;
				var varPlayer = p;
				var cardID = targetID == 0 ? 4 - p : p;
				GameModel.instance.teams[t].players[p].playerName.onChange.AddListener((b, a) =>
				{
					if (varTeam == team)
						UpdateName(GameModel.instance.teams[varTeam].players[varPlayer].displayNameWithoutTeam, cardID, a != "");
				});
            }
        }
	}

	virtual protected void UpdateName(string newName, int id, bool signedIn)
    {
		if (cards == null || cards.Count <= id) return;

		if(cards[id] != null)
        {
			cards[id].text.text = newName;
			cards[id].text.alpha = signedIn ? 1f : .5f;
			//cards[id].GetComponent<GlobalFade>().alpha = signedIn ? 1f : .5f;
        }			
    }
	virtual protected void OnGameEvent(string type, GameEventData data)
    {
		if(type == GameEventType.SPAWN && data.playerID == 2 && data.teamID == 1)
        {
			ShowPlayers();
        }
    }

	virtual protected void ShowPlayers()
    {
		var i = targetID == 0 ? 4 : 0;
		while (i >= 0 && i <= 4)
        {
			var p = GameModel.instance.teams[team].players[i];
			var s = SpriteDB.allSprites[team].playerSprites[i];
			var card = PushNewCard();
			card.player.sprite = s.icon;
			card.text.text = p.displayNameWithoutTeam;
			if (p.playerName.property == "")
				card.text.alpha = .5f; //de-emphasize playes not signed in

			i = targetID == 0 ? i - 1 : i + 1;
        }
    }

}

