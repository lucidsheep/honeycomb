using UnityEngine;
using System.Collections.Generic;

public class MainLayoutModuleManager : MonoBehaviour
{
	public List<MainBarModule> mainBars;
	public List<PostGameScreenObserver> postgameScreens;
	public List<BoxScoreBase> boxScores;
	public List<PostgamePlayerCard> playerCards;
	public List<MatchPreviewScreen> previewScreens;

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

	public static MainBarModule GetMainBar(string barName)
    {
		var ret = instance.mainBars.Find(x => x.moduleName == barName);
		if (ret == null)
			return instance.mainBars[0];
		return ret;
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

	public static MatchPreviewScreen GetMatchPreview(string screenName)
	{
		var ret = instance.previewScreens.Find(x => x.moduleName == screenName);
		if (ret == null)
			return instance.previewScreens[0];
		return ret;
	}
}

