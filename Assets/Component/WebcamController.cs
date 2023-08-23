using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WebcamController : MonoBehaviour
{
    public static WebcamController gameCamera;

    WebCamTexture webcam;
    RawImage rImage;
    public VideoClipper clipper;
    public Texture forcedTexture;
    public SpriteRenderer themeBG;
    RenderTexture bufferedFrame;
    bool isUsingFrameBuffer = false;

    int deviceID = 0;
    bool useClipper;


    // Start is called before the first frame update
    void Awake()
    {
        gameCamera = this;

        var devices = WebCamTexture.devices;
        foreach (var d in devices)
        {
            Debug.Log(d.name);
            Debug.Log(d.kind);
            if (d.availableResolutions != null)
            {
                foreach (var r in d.availableResolutions)
                {
                    Debug.Log(r.width + "x" + r.height + " @ " + r.refreshRate);
                }
            }
        }
        webcam = new WebCamTexture();
        rImage = GetComponentInChildren<RawImage>();
        rImage.texture = webcam;
        //rImage.material.mainTexture = webcam;
        bufferedFrame = new RenderTexture(1920, 1080, 24);

        if (forcedTexture != null)
            rImage.material.mainTexture = rImage.texture = forcedTexture;
    }

    private void Start()
    {
        GameModel.onGameModelComplete.AddListener(OnGameComplete);
        SetupScreen.onSetupComplete.AddListener(OnSetupComplete);
        ViewModel.onThemeChange.AddListener(OnThemeChange);
    }

    public static (Vector3, float) GetPositionAndScale(bool minimized)
    {
        Vector3 position = new Vector3(-1.65f, .95f, 90f);
        float scale = 0.007499999f;

        //hack for extra space with camp frame. need to include parameter to let theme to adjust this
        float campBump = (ViewModel.currentTheme.themeName == "campkq" || ViewModel.currentTheme.themeName == "postcamp") ? .12f : 0f;

        if (minimized)
        {
            position = new Vector3(2.63f, 3.53f, 0f);
            scale = 0.0025f;
            if (ViewModel.currentTheme.layout == ThemeData.LayoutStyle.OneCol_Left)
                position.x += 1f;
            else if (ViewModel.currentTheme.layout == ThemeData.LayoutStyle.TwoCol)
            {
                position = new Vector3(3.65f, 2.61f + campBump, 0f);
                scale = 0.0022f;
            } else if(ViewModel.currentTheme.layout == ThemeData.LayoutStyle.Game_Only)
            {
                //todo
            }
        } else
        {
            if (ViewModel.currentTheme.layout == ThemeData.LayoutStyle.OneCol_Left)
                position.x *= -1f;
            else if(ViewModel.currentTheme.layout == ThemeData.LayoutStyle.TwoCol)
            {
                
                position = new Vector3(0f, .36f + campBump, 90f); //y .48
                scale = 0.0066f;
            } else if(ViewModel.currentTheme.layout == ThemeData.LayoutStyle.Game_Only)
            {
                position = new Vector3(0f, 0f, 90f);
                scale = 0.00927f;
            }
        }
        position.y += ViewModel.bottomBarPadding.property;
        return (position, scale);
    }
    void OnThemeChange()
    {
        Vector3 position;
        float scale;
        (position, scale) = GetPositionAndScale(false);

        transform.parent.GetComponent<RectTransform>().position = position;
        transform.parent.GetComponent<RectTransform>().localScale = Vector3.one * scale;

        themeBG.sprite = AppLoader.GetStreamingSprite("gameplayCamera");
    }

    void OnSetupComplete()
    {
        useClipper = PlayerPrefs.GetInt("instantReplay") == 1;
        if (useClipper)
            clipper.SetSource(webcam);
            //clipper.SetSource(webcam, 1920 / 2, 1080 / 2);
    }
    void OnGameComplete(int _, string __)
    {
        if(useClipper)
            DOTween.Sequence().AppendInterval(1f).AppendCallback(() => clipper.SaveClip(0));
    }
    public Texture GetWebcamTexture => (forcedTexture != null ? forcedTexture : webcam);
    // Update is called once per frame
    void Update()
    {
        if(isUsingFrameBuffer && VideoClipper.frameBuffer == 0)
        {
            //end using frame buffer
            Debug.Log("framebuffer disabled");
            isUsingFrameBuffer = false;
            rImage.texture = webcam; // rImage.material.mainTexture = webcam;
        } else if(!isUsingFrameBuffer && VideoClipper.frameBuffer > 0)
        {
            //start using frame buffer
            Debug.Log("framebuffer enabled");
            isUsingFrameBuffer = true;
            rImage.texture = bufferedFrame;
        }
        if (isUsingFrameBuffer && VideoClipper.bufferedFrame != null)
        {
            Graphics.Blit(VideoClipper.bufferedFrame, bufferedFrame);
        }
    }

    public string ChangeCamera()
    {
        deviceID = deviceID >= WebCamTexture.devices.Length - 1 ? 0 : deviceID + 1;
        return ChangeCamera(deviceID);
        
    }
    public string ChangeCamera(int newID)
    {
        deviceID = newID;
        if (newID < 0 || newID >= WebCamTexture.devices.Length)
            return "";

        ChangeCamera(WebCamTexture.devices[newID].name);
        return WebCamTexture.devices[newID].name;
    }

    public void ChangeCamera(string newDeviceName)
    {
        webcam.Stop();
        if (PlayerCameraObserver.useCameraDefaults)
        {
            webcam.requestedFPS = 0;
        }
        else
        {
            webcam.requestedFPS = 60;
        }
        webcam.requestedHeight = 1920;
        webcam.requestedWidth = 1080;

        webcam.deviceName = newDeviceName;
        webcam.Play();
    }
}
