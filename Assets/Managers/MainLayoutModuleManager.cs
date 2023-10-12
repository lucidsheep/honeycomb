using UnityEngine;
using System.Collections.Generic;

public class MainLayoutModuleManager : MonoBehaviour
{
	public List<MainBarObserver> mainBars;
	public List<PostGameScreenObserver> postgameScreens;
	public List<BoxScoreBase> boxScores;
	public List<PostgamePlayerCard> playerCards;

	static MainLayoutModuleManager instance;
	// Use this for initialization
	void Awake()
	{
		instance = this;
	}

	public static PostGameScreenObserver GetPostgameScreen(string screenName)
    {
		return instance.postgameScreens.Find(x => x.moduleName == screenName);
    }

	public static MainBarObserver GetMainBar(string barName)
    {
		return instance.mainBars.Find(x => x.moduleName == barName);
    }

	public static BoxScoreBase GetBoxScore(string boxName)
    {
		var ret = instance.boxScores.Find(x => x.moduleName == boxName);
		if (ret == null)
			return instance.boxScores[0];
		return ret;
    }

	public static PostgamePlayerCard GetPlayerCard(string playerCardName)
    {
		var ret = instance.playerCards.Find(x => x.moduleName == playerCardName);
		if (ret == null)
			return instance.playerCards[0];
		return ret;
	}
}

