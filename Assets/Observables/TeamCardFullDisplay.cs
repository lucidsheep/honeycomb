using UnityEngine;
using System.Collections;

public class TeamCardFullDisplay : TeamCardDisplay
{
    bool primed = false;

    protected override void ShowPlayers()
    {
        base.ShowPlayers();
		for(int i = 0; i < 5; i++)
        {
            var cardModel = targetID == 0 ? 4 - i : i;
			var card = cards[i] as FeedCardFull;
			var topThree = GameModel.instance.teams[team].players[cardModel].GetBestStats(2);
			foreach(var stat in topThree)
            {
				card.SetStat(stat.type, stat.fullNumber);
            }
            card.SetCrowns(GameModel.instance.teams[team].players[cardModel].curGameStats.queenKills.property);
        }
    }

    protected override void OnGameEvent(string type, GameEventData data)
    {
        if(type == GameEventType.SPAWN && data.playerID == 2 && data.teamID == 1)
        {
            while (cards.Count > 0)
                FinishAnim(cards[0]);
        }
    }

    protected override void UpdateName(string newName, int id, bool signedIn)
    {
        //base.UpdateName(newName, id, signedIn);
    }

    protected override void Start()
    {
        base.Start();
        GameModel.onGameModelComplete.AddListener(OnGameComplete);
        ViewModel.endGameAnimDelay.onChange.AddListener((b, a) => { if(a == false && b != a) MaybeShowAnim(); });
    }

    void OnGameComplete(int _, string __)
    {
        primed = true;
        MaybeShowAnim();
    }
    void MaybeShowAnim()
    {
        if(ViewModel.endGameAnimDelay.property == true) return;
        if(!primed) return;
        primed = false;
        ShowPlayers();
    }
}

