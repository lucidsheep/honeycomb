using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class TopBarObserver : KQObserver
{
	public SpriteRenderer topBar;
	public SpriteRenderer bottomBar;
    public TextMeshPro titleText;

    void Awake()
    {
        PlayerCameraObserver.camerasUpdatedEvent.AddListener(OnCameraUpdate);
        ViewModel.onThemeChange.AddListener(OnCameraUpdate);
    }

    private void Start()
    {
        base.Start();
        OnCameraUpdate();
    }

    void OnCameraUpdate()
    {
        string camera = team == 0 ? "blueCamera" : "goldCamera";
        var aspectRatio = PlayerCameraObserver.GetAspectRatio(camera);
        if (PlayerCameraObserver.GetWebcamState(camera) == PlayerCameraObserver.WebcamState.Off)
            aspectRatio = PlayerCameraObserver.AspectRatio.Ultrawide;
        float topBarWidth, titleWidth;
        switch(aspectRatio)
        {
            case PlayerCameraObserver.AspectRatio.Wide:
                topBarWidth = (ViewModel.totalBarWidth / 2f) - 4.625f; titleWidth = (ViewModel.totalTextWidth / 2f) - 24f; break;
            case PlayerCameraObserver.AspectRatio.Ultrawide:
                topBarWidth = ViewModel.totalBarWidth / 2f; titleWidth = ViewModel.totalTextWidth / 2f; break;
            default:
                topBarWidth = 7.5f; titleWidth = 32.175f; break;
        }
        if (topBar != null)
        {
            topBar.transform.localScale = new Vector3(topBarWidth,
               topBar.transform.localScale.y, 1f);
            titleText.rectTransform.sizeDelta = new Vector2(titleWidth, titleText.rectTransform.sizeDelta.y);
        }
        //bottomBar.transform.localScale = new Vector3(botBarWidth,
        //  bottomBar.transform.localScale.y, 1f);
        
    }
}

