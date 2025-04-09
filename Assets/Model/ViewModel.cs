using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections.Generic;
using System.IO;
using StreamDeckSharp;

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

	public MainBarObserver mainBar;
	public MatchPreviewScreen matchPreview;
	public Canvas webcamCanvas;
	public SpriteRenderer topLevelGraphicContainer;
	public SpriteRenderer[] backgroundGraphicContainers;
	public TextAsset[] localThemes;

	public Color bgFilter;

	public ThemeDataJson theme;
	public List<ThemeData> themeList;
	public List<ThemeDataJson> themeListJson;

	public SubViewData[] subViews;
	public static ThemeDataJson currentTheme { get { return instance.theme; } }
	public static UnityEvent onThemeChange = new UnityEvent();

	public static Transform stage { get { return instance.mainBar.transform; } }

	Sequence setPointTimeout;
	Sequence pipSeq;
	bool pipActive = false;

	bool isFullscreen = false;
	int curBackground = 0;
	LSTimer subviewTransitionTimer;

	SubView currentActiveSubview;
	int currentActiveSubviewIndex = 0;

	private void Awake()
	{
		hideSetPoints[0] = new LSProperty<int>(0);
		hideSetPoints[1] = new LSProperty<int>(0);
		instance = this;

		if(appView)
			themeListJson = AppLoader.GetThemeList();
		else
        {
			themeListJson = new List<ThemeDataJson>();
			foreach (var text in localThemes)
            {

				themeListJson.Add(JsonUtility.FromJson<ThemeDataJson>(text.text));
			}
        }
		Debug.Log("found " + themeListJson.Count + " json themes");
		foreach (var theme in themeListJson)
			Debug.Log(theme.name);
	}
	void Start()
	{
		GameModel.onGameEvent.AddListener(OnGameEvent);
		MapDB.currentMap.onChange.AddListener(OnMapChange);
		if (defaultTheme != "")
			SetTheme(defaultTheme);
		else SetTheme(PlayerPrefs.GetString("theme", "oneCol"));
		LSConsole.AddCommandHook("setTheme", "refreshes current theme, or sets the theme to [themeTag]", SetThemeCommand);
		LSConsole.AddCommandHook("skipSetup", "set to [true/false], true will skip the setup dialog on launch", SkipSetupCommand);

		FixResolution();
		backgroundGraphicContainers[0].color = bgFilter;

		LSConsole.AddCommandHook("startSubview", "Opens a subview specified with [subviewName]", SetSubviewCommand);
		LSConsole.AddCommandHook("closeSubview", "Closes any open subview", CloseSubviewCommand);

		subviewTransitionTimer = new LSTimer(.1f, () => {});
	}
	public static void OnStreamDeckConnected()
	{
		if(StreamDeckManager.streamDeckActive)
		{
			//cancel function
			StreamDeckManager.RegisterDeckButton(new Vector2Int(0, 0), () => instance.EndCurrentSubView());
			StreamDeckManager.SetKeyImage(0, "streamDeckCancelIcon");

			foreach(var view in instance.subViews)
			{
				var order = view.streamDeckOrder;
				StreamDeckManager.RegisterDeckButton(new Vector2Int(order % StreamDeckManager.dimensions.x, order / StreamDeckManager.dimensions.x), () => instance.StartSubView(view.viewObject));
				StreamDeckManager.SetKeyImage(order, view.streamDeckIcon);
			}
		}
	}
	string SetSubviewCommand(params string[] args)
	{
		if(args.Length == 0) return "Specify name of subview to show";
		var svName = args[0];

		foreach(var sv in subViews)
		{
			if(sv.viewName == svName)
			{
				StartSubView(sv.viewObject);
				return "";
			}
		}
		return "No subview with name '" + svName + "' found";
	}

	string CloseSubviewCommand(params string[] args)
	{
		EndCurrentSubView();
		return "Current Subview closed";
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

	public string SetThemeCommand(string[] args)
	{
		if (args.Length == 0)
			SetTheme(ViewModel.currentTheme.name);
		else
			SetTheme(args[0]);
		return "";
	}

	public string SkipSetupCommand(string[] args)
    {
		bool curVal = PlayerPrefs.GetInt("skipSetup") == 1;
		if(args.Length == 0)
        {
			curVal = !curVal;
        } else
        {
			curVal = args[0] == "true" || args[0] == "yes" || args[0] == "enabled";
        }
		PlayerPrefs.SetInt("skipSetup", curVal ? 1 : 0);
		return "skipSetup set to " + curVal;
	}

	public static void SetTheme(string themeTag)
    {
		Debug.Log("setTheme " + themeTag);
		ThemeDataJson newTheme = instance.themeListJson.Find(x => x.name == themeTag);
		if (newTheme != null)
		{
			instance.theme = newTheme;
			switch (newTheme.GetLayout())
			{
				case ThemeDataJson.LayoutStyle.OneCol_Left:
				case ThemeDataJson.LayoutStyle.OneCol_Right:
					totalBarWidth = 10.045f * 2f;
					totalTextWidth = 88f;
					break;
				case ThemeDataJson.LayoutStyle.TwoCol:
					totalBarWidth = 9.31f * 2f;
					totalTextWidth = 78f;
					break;
			}
			UIState.inverted = newTheme.startReversed;
			bottomBarPadding.property = newTheme.showTicker ? .30f : 0f;
			instance.topLevelGraphicContainer.sprite = AppLoader.GetStreamingSprite("mainFrame");
			instance.backgroundGraphicContainers[0].sprite = AppLoader.GetStreamingSprite("background");
			SetMatchPreviewScreen(newTheme.matchPreview == null ? "" : newTheme.matchPreview.name);
			if(newTheme.backgroundColor != null && newTheme.backgroundColor != "")
			{
				instance.bgFilter = Util.HexToColor(newTheme.backgroundColor);
				instance.backgroundGraphicContainers[0].color = instance.bgFilter;
			}
			onThemeChange.Invoke();
		}
    }

	static void SetMatchPreviewScreen(string styleName)
    {
		if (instance.matchPreview != null)
			Destroy(instance.matchPreview.gameObject);

		instance.matchPreview = Instantiate(MainLayoutModuleManager.GetMatchPreview(styleName), instance.transform.parent);
		
    }

	void StartSubView(SubView view)
	{
		if(subviewTransitionTimer != null && subviewTransitionTimer.progress < 1f) return;

		if(currentActiveSubview != null) EndCurrentSubView();
		else
		{
			currentActiveSubview = Instantiate(view, Vector3.zero, Quaternion.identity);
			currentActiveSubview.OnSubViewStarted();
			subviewTransitionTimer = new LSTimer(.6f, () => {});
		}
	}

	void EndCurrentSubView()
	{
		if(subviewTransitionTimer != null && subviewTransitionTimer.progress < 1f) return;

		if(currentActiveSubview == null) return;
		currentActiveSubview.OnSubViewClosed();
		currentActiveSubview = null;
		subviewTransitionTimer = new LSTimer(.6f, () => {});
	}
}
