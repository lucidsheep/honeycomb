using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SetupScreen : MonoBehaviour
{
	[System.Serializable]
	public class CameraState
    {
		public string name;
		public int id;
		public string deviceName;
		public PlayerCameraObserver.AspectRatio aspectRatio;
		public TMP_Text sourceText;
		public TMP_Text ratioText;
		public PlayerCameraObserver previewCamera;
    }
	public static bool setupInProgress = false;

	public TMP_InputField cabID, sceneID, themeName, tickerCabs;
	public Toggle doubleElim, instantReplay, computerVision, virtualDesktop, lowResCamera;
	public WebcamController webcamController;
	public CameraState gameCameraState, blueCameraState, goldCameraState, commCameraState;
	public TMP_Dropdown themeSelector;
	public GameObject controlPanelTemplate;
	public TMP_InputField[] tabList;
	public TMP_Text connectionModeToggleText, sceneText, cabText, ipText;

	public static UnityEvent onSetupComplete = new UnityEvent();

	const int THEME_UNDEFINED = -1;
	const int THEME_ONECOL = 0;
	const int THEME_TWOCOL = 1;
	const int THEME_TWOCOL_TICKER = 2;
	const int THEME_GAMEONLY = 3;
	const int THEME_CUSTOM = 4;

	string storedCustomThemeName;
	int curTab = -1;
	bool localMode = false;

	CameraState[] cameraStates;
    private void Awake()
    {
		setupInProgress = true; //setup flag if this object exists, otherwise skip setup
		cameraStates = new CameraState[] { gameCameraState, blueCameraState, goldCameraState, commCameraState };
    }
    // Use this for initialization
    void Start()
	{
		cabID.text = PlayerPrefs.GetString("cabID");
		sceneID.text = PlayerPrefs.GetString("sceneID");
		themeName.text = PlayerPrefs.GetString("theme");
		tickerCabs.text = PlayerPrefs.GetString("ticker");
		if (themeName.text == "")
			themeName.text = "oneCol";
		storedCustomThemeName = themeName.text;
		//cabName.text = PlayerPrefs.GetString("cabName");
		//twitchURL.text = PlayerPrefs.GetString("twitchURL");
		computerVision.isOn = PlayerPrefs.GetInt("computerVision") == 1;
		//doubleElim.isOn = PlayerPrefs.GetInt("doubleElim") == 1;
		instantReplay.isOn = PlayerPrefs.GetInt("instantReplay") == 1;
		virtualDesktop.isOn = PlayerPrefs.GetInt("virtualDesktop") == 1;
		lowResCamera.isOn = PlayerPrefs.GetInt("lowResCamera") == 1;
		var themeOption = PlayerPrefs.GetInt("themePreset", -1);
		themeSelector.onValueChanged.AddListener(OnThemeSelected);
		themeSelector.value = themeOption == THEME_UNDEFINED ? THEME_CUSTOM : themeOption;
		var gameplayCamID = PlayerPrefs.GetInt("gameplayCameraID", 0);
		webcamController.ChangeCamera(gameplayCamID);
		//PlayerCameraObserver.camerasUpdatedEvent.AddListener(OnCamerasUpdated);
		for(int i = 0; i < cameraStates.Length; i++)
        {
			if (cameraStates[i].name == "gameplayCamera")
			{
				cameraStates[i].aspectRatio = PlayerCameraObserver.AspectRatio.Wide;
				cameraStates[i].deviceName =
					PlayerPrefs.GetString("gameplayCameraName");
				cameraStates[i].id = PlayerCameraObserver.GetCameraID(cameraStates[i].deviceName);
			}
			else
			{
				var cam = PlayerCameraObserver.GetCamera(cameraStates[i].name);
				if (cameraStates[i].name == "commentaryCamera")
					cam.aspectRatio = PlayerCameraObserver.AspectRatio.Wide;
				cameraStates[i].aspectRatio = cam.aspectRatio;
				cameraStates[i].deviceName = cam.deviceName;
				cameraStates[i].id = cam.deviceIndex;
			}
			if(cameraStates[i].previewCamera != null)
				cameraStates[i].previewCamera.StartCamera();
		}
		OnCamerasUpdated();

		int tabIndex = 0;
		foreach(var tab in tabList)
        {
			int thisTab = tabIndex;
			tab.onSelect.AddListener(s => OnTabSelected(thisTab));
			tabIndex++;
        }

		if (PlayerPrefs.GetInt("skipSetup") == 1)
			OnDoneClicked();
	}

	public void ToggleMode()
    {
		localMode = !localMode;

		if(localMode)
        {
			connectionModeToggleText.text = "Local";
			sceneText.gameObject.SetActive(false);
			cabText.gameObject.SetActive(false);
			cabID.gameObject.SetActive(false);
			ipText.gameObject.SetActive(true);

			sceneID.text = "kq.local";
        } else
        {
			connectionModeToggleText.text = "HiveMind";
			sceneText.gameObject.SetActive(true);
			cabText.gameObject.SetActive(true);
			cabID.gameObject.SetActive(true);
			ipText.gameObject.SetActive(false);
			sceneID.text = PlayerPrefs.GetString("sceneID");
		}
    }
	void OnTabSelected(int tab)
    {
		curTab = tab;
    }

	void SelectTab(int tab)
    {
		tabList[tab].Select();
    }

	void OnThemeSelected(int newTheme)
    {
		storedCustomThemeName = themeName.text;
		themeName.gameObject.SetActive(newTheme == THEME_CUSTOM);
    }

	string GetThemeName()
    {
		switch(themeSelector.value)
        {
			case THEME_ONECOL: case THEME_UNDEFINED: return "oneCol";
			case THEME_TWOCOL: return "twoCol";
			case THEME_TWOCOL_TICKER: return "twoColTicker";
			case THEME_GAMEONLY: return "gameOnly";
			case THEME_CUSTOM: default: return themeName.text;
        }
    }
	// Update is called once per frame
	void Update()
	{

		if (setupInProgress && AppLoader.SKIP_SETUP)
			OnDoneClicked();
		if(Input.GetKeyDown(KeyCode.Tab))
        {
			bool found = false;
			do
			{
				curTab = curTab + 1 >= tabList.Length ? 0 : curTab + 1;
			} while (!tabList[curTab].IsActive());

			SelectTab(curTab);
        }
	}

	void OnCamerasUpdated()
    {
		foreach(var camera in cameraStates)
        {
			camera.ratioText.text = camera.aspectRatio.ToString();
			camera.sourceText.text = camera.deviceName;
			if (camera.previewCamera != null)
            {
				camera.previewCamera.deviceName = ViewModel.currentTheme.GetLayout() == ThemeDataJson.LayoutStyle.Game_Only ? "Off" : camera.deviceName;
				camera.previewCamera.StartCamera();
            }
		}
	}

	public void ToggleGameplay()
    {
		ToggleCam(gameCameraState);
	}

	public void ToggleBlue()
	{
		ToggleCam(blueCameraState);
	}

	public void ToggleGold()
	{
		ToggleCam(goldCameraState);
	}

	public void ToggleCommentary()
	{
		ToggleCam(commCameraState);
	}

	public void ToggleBlueAspect()
    {
		ToggleAspect(blueCameraState);
    }

	public void ToggleGoldAspect()
	{
		ToggleAspect(goldCameraState);
	}

	public void ToggleCommentaryAspect()
	{
		ToggleAspect(commCameraState);
	}

	void ToggleAspect(CameraState camera)
    {
		var ratio = camera.aspectRatio;
		if (ratio == PlayerCameraObserver.AspectRatio.Wide)
			ratio = PlayerCameraObserver.AspectRatio.Ultrawide;
		else
			ratio = PlayerCameraObserver.AspectRatio.Wide;
		camera.aspectRatio = ratio;
		OnCamerasUpdated();
	}
	void ToggleCam(CameraState camera)
    {
		//int curID = camera.id; PlayerPrefs.GetInt(camera.name + "ID", -2);
		camera.id = camera.id + 1 >= WebCamTexture.devices.Length ? -2 : camera.id + 1;
		if (camera.name == "gameplayCamera" && camera.id < 0)
			camera.id = 0;
		camera.deviceName = PlayerCameraObserver.GetSourceName(camera.id);
		OnCamerasUpdated();
		//PlayerPrefs.SetInt(camName + "ID", curID);
		//PlayerCameraObserver.camerasUpdatedEvent.Invoke();
	}
	public void OnDoneClicked()
    {
		PlayerPrefs.SetString("cabID", cabID.text);
		if(!localMode)
			PlayerPrefs.SetString("sceneID", sceneID.text);
		PlayerPrefs.SetString("theme", GetThemeName());
		PlayerPrefs.SetInt("themePreset", themeSelector.value);
		PlayerPrefs.SetString("ticker", tickerCabs.text);
		//PlayerPrefs.SetString("cabName", cabName.text);
		//PlayerPrefs.SetString("twitchURL", twitchURL.text);
		//PlayerPrefs.SetString("curCameraName", curCameraName);
		//PlayerPrefs.SetInt("doubleElim", doubleElim.isOn ? 1 : 0);
		PlayerPrefs.SetInt("doubleElim", 0); //disabling for now
		PlayerPrefs.SetInt("instantReplay", instantReplay.isOn ? 1 : 0);
		PlayerPrefs.SetInt("computerVision", computerVision.isOn ? 1 : 0);
		PlayerPrefs.SetInt("virtualDesktop", virtualDesktop.isOn ? 1 : 0);
		PlayerPrefs.SetInt("lowResCamera", lowResCamera.isOn ? 1 : 0);

		setupInProgress = false;
		NetworkManager.instance.sceneName = sceneID.text;
		NetworkManager.instance.cabinetName = cabID.text;
		NetworkManager.BeginNetworking(localMode ? sceneID.text : "");

		string[] cabsToWatch = tickerCabs.text.Split(',');
		for (int i = 0; i < cabsToWatch.Length; i++)
			cabsToWatch[i] = cabsToWatch[i].Replace(" ", "");
		TickerNetworkManager.Init(cabsToWatch);
		ViewModel.SetTheme(GetThemeName());

		GoogleSheetsDB.instance.ImportData(sceneID.text);

		//todo - change these to ultrawide settings
		if (lowResCamera.isOn)
		{
			PlayerCameraObserver.cameraHeight = 1080;
			PlayerCameraObserver.cameraWidth = 1920;
			PlayerCameraObserver.cameraFPS = 30;
			PlayerCameraObserver.useCameraDefaults = false;
		}
		else
		{
			PlayerCameraObserver.cameraHeight = 1080;
			PlayerCameraObserver.cameraWidth = 1920;
			PlayerCameraObserver.cameraFPS = 60;
			PlayerCameraObserver.useCameraDefaults = false;
		}
		foreach (var camera in cameraStates)
		{
			if (camera.name == "gameplayCamera")
			{
				Debug.Log("gameplay camera is " + camera.deviceName);
				webcamController.ChangeCamera(camera.deviceName);
				PlayerPrefs.SetString("gameplayCameraName", camera.deviceName);
			}
			else
			{
				var cam = PlayerCameraObserver.GetCamera(camera.name);
				if (cam != null)
				{
					cam.deviceName = ViewModel.currentTheme.GetLayout() == ThemeDataJson.LayoutStyle.Game_Only ? "Off" : camera.deviceName;
					cam.aspectRatio = camera.aspectRatio;
					if (cam.state == PlayerCameraObserver.WebcamState.On)
						cam.StartCamera();
				}
			}
		}

		PlayerCameraObserver.UpdateAllCameras();
		if(virtualDesktop.isOn)
        {
			UnityCaptureTexture.Init();
			Instantiate(controlPanelTemplate);
        }

		onSetupComplete.Invoke();

		Destroy(this.gameObject);
	}
}

