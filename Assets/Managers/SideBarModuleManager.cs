using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SideBarModuleManager : MonoBehaviour
{
	public List<KQObserver> moduleOptions;

	public List<KQObserver> curModules = new List<KQObserver>();
	public GameObject leftSidebar;
	public GameObject rightSidebar;
	public TextMeshPro gameIDText, timeText;
	public Color gameInfoColor;

	KQObserver commentaryModule;
	// Use this for initialization
	bool init = false;

    private void Awake()
    {
		//always create commentary camera module, we need it to update camera state
		commentaryModule = Instantiate(moduleOptions.Find(x => x.moduleName == "commentaryCamera"), transform);
    }
    void Start()
	{
		ViewModel.onThemeChange.AddListener(OnThemeChange);
	}

	void OnThemeChange()
    {
		SetModules();
		bool showInfo = !ViewModel.currentTheme.showBuzzBar && ViewModel.currentTheme.layout != ThemeData.LayoutStyle.Game_Only;
		gameIDText.color = timeText.color = (showInfo ? gameInfoColor : new Color(1f, 1f, 1f, 0f));
    }

    private void SetModules()
    {
		foreach (var m in curModules)
			Destroy(m.gameObject);
		curModules = new List<KQObserver>();

		var infoY = ViewModel.currentTheme.layout == ThemeData.LayoutStyle.TwoCol ? -5.8f : -4.5f;
		rightSidebar.GetComponent<GameInfoObserver>().time.transform.localPosition = new Vector3(-1.72f, infoY, 0f);
		rightSidebar.GetComponent<GameInfoObserver>().id.transform.localPosition = new Vector3(-1.67f, infoY, 0f);

		for(int i = 0; i < 2; i++)
        {
			var moduleList = i == 0 ? ViewModel.currentTheme.sidebarModules : ViewModel.currentTheme.sidebarModules_Left;
			var transformToUse = i == 0 ? rightSidebar.transform : leftSidebar.transform;
			var curY = 5f;

			transformToUse.localScale = Vector3.one * (ViewModel.currentTheme.layout == ThemeData.LayoutStyle.TwoCol ? .75f : 1f);
			var pos = new Vector3(7.3f, 0f, 0f) + (ViewModel.currentTheme.layout == ThemeData.LayoutStyle.TwoCol ? new Vector3(.35f, 1.18f, 0f) : Vector3.zero);
			if (i == 1)
				pos.x *= -1f;
			transformToUse.localPosition = pos;
			foreach (var moduleName in moduleList)
			{
				curY -= ViewModel.currentTheme.sideBarPadding;
				var parsedModule = moduleName.Split('|');
				if (parsedModule[0] == "commentaryCamera")
				{
					if (PlayerCameraObserver.GetWebcamState("commentaryCamera") == PlayerCameraObserver.WebcamState.Off)
					{
						continue;
					}
					else
					{
						commentaryModule.transform.parent = transformToUse;
						commentaryModule.transform.localScale = Vector3.one;
						commentaryModule.SetParameters(parsedModule);
						commentaryModule.transform.localPosition = new Vector3(0f + commentaryModule.offset.x, curY - (commentaryModule.size / 2f) + commentaryModule.offset.y, 0f);
						curY -= commentaryModule.size;
					}
				}
				else
				{
					var thisModule = moduleOptions.Find(x => x.moduleName == parsedModule[0]);
					if (thisModule != null)
					{
						var size = thisModule.size;
						var newMod = Instantiate(thisModule, transformToUse);
						newMod.SetParameters(parsedModule);
						newMod.transform.localPosition = new Vector3(0f + newMod.offset.x, curY - (newMod.size / 2f) + newMod.offset.y, 0f);
						curY -= newMod.size;
						if (thisModule.moduleName == "leaderboard" && ViewModel.currentTheme.leaderboardID < 0)
                        {
							Destroy(newMod.gameObject);
                        } else
                        {
							curModules.Add(newMod);
						}
						//curY -= size;
						
					}
				}
			}
        }
    }
    // Update is called once per frame
    void Update()
	{
		if (!init)
        {
			init = true;
			SetModules();
        }
	}
}

