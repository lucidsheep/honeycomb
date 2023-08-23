using UnityEngine;
using System.Collections;

public class EventFeedObserver : PlayerFeedObserver
{
	void Start()
	{
		NetworkManager.instance.gameEventDispatcher.AddListener(OnGameEvent);
	}


	void OnGameEvent(string eType, GameEventData values)
	{

		if (eType == GameEventType.PLAYER_KILL && values.teamID == team && values.targetType != EventTargetType.DRONE && values.targetID != 2)
		{
			//Debug.Log("Kill event for team " + values.teamID + ", my tid= " + targetID);
			var card = PushNewCard();
			card.player.sprite = SpriteDB.allSprites[team].playerSprites[values.playerID].icon;
			card.target.sprite = SpriteDB.allSprites[1 - team].playerSprites[values.targetID].icon;
		}
		if(eType == GameEventType.GAME_END_DETAIL)
        {
			var tempCards = cards.ToArray();
			foreach (var card in tempCards)
				FinishAnim(card);
        }
	}
}

