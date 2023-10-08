using UnityEngine;
using System.Collections.Generic;

public class MainLayoutModuleManager : MonoBehaviour
{
	public List<MainBarObserver> mainBars;
	public List<PostGameScreenObserver> postgameScreens;

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
}

