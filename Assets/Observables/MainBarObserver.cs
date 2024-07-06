using UnityEngine;
using System.Collections;

public class MainBarObserver : KQObserver
{
	public PlayerCameraObserver blueCam;
	public PlayerCameraObserver goldCam;
	public MainBarModule scoreBar;
	public MainBarInputArea inputArea;

    // Use this for initialization

    private void Awake()
    {
		ViewModel.onThemeChange.AddListener(OnThemeChange);
	}
    public override void Start()
	{
		base.Start();
		
	}

	void OnThemeChange()
    {
		SetScorebar();
		Vector3 newPos = new Vector3(0f, -4.24f + ViewModel.bottomBarPadding.property, 0f);
		float scale = 1.0f;
		float camX = 7.2f;
		float camY = -.79f;
		float camScale = 1f;
		switch(ViewModel.currentTheme.GetLayout())
        {
			case ThemeDataJson.LayoutStyle.Game_Only:
				newPos.y = -50f;
				break;
			case ThemeDataJson.LayoutStyle.TwoCol:
				camX = 6.674f;
				scale = .95f;
				break;
			case ThemeDataJson.LayoutStyle.OneCol_Right:
				newPos.x = -1.65f;
				newPos.y = -4.18f + ViewModel.bottomBarPadding.property;
				break;
			case ThemeDataJson.LayoutStyle.OneCol_Left:
				newPos.x = 1.65f;
				newPos.y = -4.18f + ViewModel.bottomBarPadding.property;
				break;

        }
		if(ViewModel.currentTheme.barStyle.cameraScale > 0f)
        {
			camX = ViewModel.currentTheme.barStyle.cameraX;
			camY = ViewModel.currentTheme.barStyle.cameraY;
			camScale = ViewModel.currentTheme.barStyle.cameraScale;
			blueCam.iconsVisible = goldCam.iconsVisible = !ViewModel.currentTheme.barStyle.hideCameraIcons;
		}
		if(ViewModel.currentTheme.barStyle.customScale > 0f)
        {
			newPos = new Vector3(ViewModel.currentTheme.barStyle.customPositionX, ViewModel.currentTheme.barStyle.customPositionY, 0f);
			scale = ViewModel.currentTheme.barStyle.customScale;
        }
		transform.localPosition = newPos;
		blueCam.transform.localPosition = new Vector3(-camX, camY, -3.3f);
		goldCam.transform.localPosition = new Vector3(camX, camY, -3.3f);
		blueCam.transform.localScale = goldCam.transform.localScale = Vector3.one * camScale;
		transform.localScale = Vector3.one * scale;

		//gameObject.SetActive(ViewModel.currentTheme.layout != ThemeData.LayoutStyle.Game_Only);
		//-3.94
    }

	void SetScorebar()
    {
		if (scoreBar != null)
			Destroy(scoreBar.gameObject);
		scoreBar = Instantiate(MainLayoutModuleManager.GetMainBar(ViewModel.currentTheme.barStyle == null ? "defaultBar" : ViewModel.currentTheme.barStyle.name), transform);
    }
	// Update is called once per frame
	void Update()
	{
			
	}
}

