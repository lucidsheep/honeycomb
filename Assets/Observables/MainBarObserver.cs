using UnityEngine;
using System.Collections;

public class MainBarObserver : KQObserver
{
	public PlayerCameraObserver blueCam;
	public PlayerCameraObserver goldCam;

	// Use this for initialization
	public override void Start()
	{
		base.Start();
		ViewModel.onThemeChange.AddListener(OnThemeChange);
	}

	void OnThemeChange()
    {
		Vector3 newPos = new Vector3(0f, -4.24f + ViewModel.bottomBarPadding.property, 0f);
		float scale = 1f;
		float camX = 7.2f;
		switch(ViewModel.currentTheme.layout)
        {
			case ThemeData.LayoutStyle.Game_Only:
				newPos.y = -50f;
				break;
			case ThemeData.LayoutStyle.TwoCol:
				camX = 6.674f;
				scale = .95f;
				break;
			case ThemeData.LayoutStyle.OneCol_Right:
				newPos.x = -1.65f;
				newPos.y = -4.18f + ViewModel.bottomBarPadding.property;
				break;
			case ThemeData.LayoutStyle.OneCol_Left:
				newPos.x = 1.65f;
				newPos.y = -4.18f + ViewModel.bottomBarPadding.property;
				break;

        }
		transform.localPosition = newPos;
		blueCam.transform.localPosition = new Vector3(-camX, -.79f, -3.3f);
		goldCam.transform.localPosition = new Vector3(camX, -.79f, -3.3f);
		transform.localScale = Vector3.one * scale;

		//gameObject.SetActive(ViewModel.currentTheme.layout != ThemeData.LayoutStyle.Game_Only);
		//-3.94
    }
	// Update is called once per frame
	void Update()
	{
			
	}
}

