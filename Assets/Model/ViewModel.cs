using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections.Generic;

public class ViewModel : MonoBehaviour
{
	public static ViewModel instance;

	public static float centerHexPadding = 2f;

	public static LSProperty<float> leftHexPadding = new LSProperty<float>(-3.5f);
	public static LSProperty<float> leftSetPadding = new LSProperty<float>(0f);

	public static LSProperty<float> rightHexPadding = new LSProperty<float>(3.5f);
	public static LSProperty<float> rightSetPadding = new LSProperty<float>(0f);

	public static LSProperty<float> bottomBarPadding = new LSProperty<float>(0f);

	public static LSProperty<bool> endGameAnimDelay = new LSProperty<bool>(false);
	public static LSProperty<int>[] hideSetPoints = new LSProperty<int>[2];

	public static float screenWidth;
	public static float screenHeight;

	public static float totalBarWidth = 20.12f;
	public static float totalTextWidth = 88f;

	public static string defaultTheme = "";

	public bool appView = false;

	public GameObject mainTransform;
	public Canvas webcamCanvas;
	public SpriteRenderer topLevelGraphicContainer;
	public SpriteRenderer[] backgroundGraphicContainers;

	public Color bgFilter;

	public ThemeData theme;
	public List<ThemeData> themeList;
	public static ThemeData currentTheme { get { return instance.theme; } }
	public static UnityEvent onThemeChange = new UnityEvent();

	public static Transform stage { get { return instance.mainTransform.transform; } }

	Sequence setPointTimeout;
	Sequence pipSeq;
	bool pipActive = false;
	bool vdActive = false;
	int vdIndex = -1;
	bool isFullscreen = false;
	int curBackground = 0;
	VirtualDesktop.IVirtualDesktop mainDisplay;
	VirtualDesktop.IVirtualDesktop virtualDisplay;

	private void Awake()
	{
		hideSetPoints[0] = new LSProperty<int>(0);
		hideSetPoints[1] = new LSProperty<int>(0);
		instance = this;
	}
	void Start()
	{
		GameModel.onGameEvent.AddListener(OnGameEvent);
		MapDB.currentMap.onChange.AddListener(OnMapChange);
		if (defaultTheme != "")
			SetTheme(defaultTheme);
		LSConsole.AddCommandHook("setTheme", "sets the theme to [themeTag]", SetThemeCommand);

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
		instance.mainDisplay = VirtualDesktop.DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop();
		var info = Screen.mainWindowDisplayInfo;
		Screen.MoveMainWindowTo(in info, Vector2Int.zero);
		//LSConsole.AddCommandHook("display", "display commands. [list] displays, or [create] or [destroy] a virtual desktop. [switch <index> <fullscreen/windowed>] active display to a given index and set fullscreened or windowed.", DisplayCommand);
#endif
		FixResolution();
		backgroundGraphicContainers[0].color = bgFilter;
	}

	void OnMapChange(MapData before, MapData after)
	{
		Debug.Log("onmapchange " + after.name);

		var split = after.name.Split('_');
		Debug.Log("splitlen " + split.Length);
		if (split.Length == 0) return;
		Debug.Log("name is " + split[1]);
		var maybeSprite = AppLoader.GetStreamingSprite("background_" + split[1]);
		if (maybeSprite == null) return;

		if (backgroundGraphicContainers.Length == 0) return;
		if (backgroundGraphicContainers.Length == 1)
			backgroundGraphicContainers[0].sprite = maybeSprite;
		else
		{
			backgroundGraphicContainers[curBackground].DOColor(new Color(bgFilter.r, bgFilter.g, bgFilter.b, 0f), 2f);
			curBackground = curBackground + 1 >= backgroundGraphicContainers.Length ? 0 : curBackground + 1;
			backgroundGraphicContainers[curBackground].sprite = maybeSprite;
			backgroundGraphicContainers[curBackground].DOColor(bgFilter, 2f);
		}
    }
	void OnGameEvent(string type, GameEventData data)
	{
		if (type == GameEventType.SPAWN && data.teamID == 1 && data.playerID == 2)
		{
			hideSetPoints[0].property = hideSetPoints[1].property = 1;
			if (setPointTimeout != null && setPointTimeout.IsPlaying())
			{
				setPointTimeout.Kill();
			}
			setPointTimeout = DOTween.Sequence().AppendInterval(9.75f).AppendCallback(() => { hideSetPoints[0].property -= 1; hideSetPoints[1].property -= 1; });
		} else if (type == GameEventType.GAME_END_DETAIL)
		{
			hideSetPoints[0].property += 1;
			hideSetPoints[1].property += 1;
			if (setPointTimeout != null && setPointTimeout.IsPlaying())
			{
				setPointTimeout.Kill();
			}
		}
	}
	// Update is called once per frame
	void Update()
	{
		//var res = Screen.currentResolution;
		
	}

	public static void FixResolution() { instance._FixResolution(); }
	public void _FixResolution()
    {
		if (appView && !isFullscreen && Camera.main.aspect != (16f / 9f))
		{
			Screen.SetResolution(1280, 720, false);
		}

		screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2f;
		screenHeight = Camera.main.orthographicSize * 2f;
	}
	public static void CreateVirtualDesktop()
    {
		if (instance.vdActive) return;

		var d = VirtualDesktop.DesktopManager.VirtualDesktopManagerInternal.CreateDesktop();
		instance.vdIndex = VirtualDesktop.DesktopManager.GetDesktopIndex(d);
		Debug.Log("created virtual desktop at index " + instance.vdIndex);
		instance.vdActive = true;
		instance.virtualDisplay = d;

		Debug.Log(instance.DisplayCommand(new string[] { }));
	}

	public static void SwitchToDesktop(int index, bool fullscreen)
    {
		var layouts = new List<DisplayInfo>();
		Screen.GetDisplayLayout(layouts);
		var info = layouts[index];
		Screen.MoveMainWindowTo(in info, Vector2Int.zero);
		if (fullscreen)
		{
			Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
			Screen.SetResolution(1920, 1080, true);
		}
		else
		{
			Screen.fullScreenMode = FullScreenMode.Windowed;
			Screen.SetResolution(1280, 720, false);
		}
		instance.isFullscreen = fullscreen;
	}
	public static void StartPIP(bool instant)
	{
		if (instance.pipActive) return;
		instance.pipActive = true;
		if (instance.pipSeq != null && !instance.pipSeq.IsComplete())
			instance.pipSeq.Kill();

		//set webcam to smaller proportions, change layer
		instance.webcamCanvas.sortingLayerName = "TopStats";
		Vector3 targetPos;
		float scaleMult;
		(targetPos, scaleMult) = WebcamController.GetPositionAndScale(true);
		if (instant)
		{
			instance.webcamCanvas.transform.position = targetPos;
			instance.webcamCanvas.transform.localScale = Vector3.one * scaleMult;
		} else
		{
			instance.pipSeq = DOTween.Sequence()
				.Append(instance.webcamCanvas.transform.DOMove(targetPos, .5f))
				.Join(instance.webcamCanvas.transform.DOScale(Vector3.one * scaleMult, .5f));
		}

	}

	public static void EndPIP(bool instant)
	{
		if (!instance.pipActive) return;
		instance.pipActive = false;
		if (instance.pipSeq != null && !instance.pipSeq.IsComplete())
			instance.pipSeq.Kill();

		Vector3 targetPos;
		float scaleMult;
		(targetPos, scaleMult) = WebcamController.GetPositionAndScale(false);

		//tween webcam to big proportions, change layer
		if (instant)
		{
			instance.webcamCanvas.transform.position = targetPos;
			instance.webcamCanvas.transform.localScale = Vector3.one * scaleMult;
			instance.webcamCanvas.sortingLayerName = "Default";
		} else
		{
			instance.pipSeq = DOTween.Sequence()
				.Append(instance.webcamCanvas.transform.DOMove(targetPos, .5f))
				.Join(instance.webcamCanvas.transform.DOScale(Vector3.one * scaleMult, .5f))
				.OnComplete(() => instance.webcamCanvas.sortingLayerName = "Default");
		}
	}

	public string DisplayCommand(string[] args)
	{
		var ret = "";
		if (args.Length == 0 || args[0] == "list")
		{
			var id = VirtualDesktop.DesktopManager.GetDesktopIndex(mainDisplay);
			var layouts = new List<DisplayInfo>();
			Screen.GetDisplayLayout(layouts);
			ret = "Display List\nCurrent desktop: " + id;
			for(int i = 0; i < layouts.Count; i++)
            {
				var d = layouts[i];
				ret += "\n" + i + ": " + d.name + " (" + d.width + "x" + d.height + ")";
			}
			ret += "\n\nAlt Display List";
			for(int i = 0; i < Display.displays.Length; i++)
            {
				var d = Display.displays[i];
				ret += "\n" + d.ToString() + " (" + d.systemWidth + "x" + d.systemHeight + ")";
            }
		} else
        {
			switch(args[0])
            {
				case "create":
					CreateVirtualDesktop(); break;
				case "destroy":
					DestroyVirtualDesktop(); break;
				case "switch":
					var index = -1;
					if(args.Length < 2)
                    {
						ret = "Command requires index of display to switch to";
						break;
                    }
					var id = VirtualDesktop.DesktopManager.GetDesktopIndex(mainDisplay);
					var layouts = new List<DisplayInfo>();
					Screen.GetDisplayLayout(layouts);

					int.TryParse(args[1], out index);
					if(index < 0 || index >= layouts.Count)
                    {
						ret = "Invalid index";
						break;
                    }

					bool fullScreen = true;
					if (args.Length > 2 && args[2] == "windowed")
						fullScreen = false;

					SwitchToDesktop(index, fullScreen);
					break;
				default:
					ret = "Invalid display command";
					break;
            }
        }

		return ret;
	}

	public string SetThemeCommand(string[] args)
	{
		if (args.Length == 0)
			SetTheme("pdx");
		else
			SetTheme(args[0]);
		return "";
	}

	public static void SetTheme(string themeTag)
    {
		Debug.Log("setTheme " + themeTag);
		ThemeData newTheme = instance.themeList.Find(x => x.themeName.Equals(themeTag));
		if (newTheme != null)
		{
			instance.theme = newTheme;
			switch (newTheme.layout)
			{
				case ThemeData.LayoutStyle.OneCol_Left:
				case ThemeData.LayoutStyle.OneCol_Right:
					totalBarWidth = 20.22f;
					totalTextWidth = 88f;
					break;
				case ThemeData.LayoutStyle.TwoCol:
					totalBarWidth = 9.34f * 2f; // 17.72f;
					totalTextWidth = 78f;
					break;
			}
			bottomBarPadding.property = newTheme.showBuzzBar ? .30f : 0f;
			instance.topLevelGraphicContainer.sprite = AppLoader.GetStreamingSprite("mainFrame");
			instance.backgroundGraphicContainers[0].sprite = AppLoader.GetStreamingSprite("background");
			onThemeChange.Invoke();
		}
    }

	public static void DestroyVirtualDesktop()
    {
		if (!instance.vdActive) return;

		VirtualDesktop.DesktopManager.VirtualDesktopManagerInternal.RemoveDesktop(instance.virtualDisplay, instance.mainDisplay);
		instance.virtualDisplay = null;
		instance.vdIndex = -1;
		instance.vdActive = false;

		Debug.Log("Removed virtual desktop");
	}
    private void OnDestroy()
    {
        if(vdActive)
        {
			DestroyVirtualDesktop();
        }
    }
}

