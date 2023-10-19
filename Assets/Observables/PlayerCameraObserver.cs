using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerCameraObserver : KQObserver
{

    public enum AspectRatio {Wide, Ultrawide };
    public enum WebcamState { Off, FrameOnly, On }
    public static UnityEvent camerasUpdatedEvent = new UnityEvent();

    static bool _showPlayerCams = true;
    public static bool webcamsEnabled { get { return ViewModel.instance.appView; } }
    public static bool showPlayerCams { get { return ViewModel.currentTheme.showPlayerCams && _showPlayerCams; } }

    public WebcamState state
    {
        get {
            switch(_deviceName)
            {
                case "Off": return WebcamState.Off;
                case "Empty Frame": return WebcamState.FrameOnly;
                default: return WebcamState.On;
            }
        }
    }
   
    WebCamTexture webcam;
    public RawImage cameraImage;
    public string webcamName;
    public SpriteRenderer frame;
    public Image[] icons;
    public bool iconsVisible = true;

    AspectRatio _ratio;
    public AspectRatio aspectRatio { get { return _ratio; } set
        {
            _ratio = value;
            PlayerPrefs.SetInt(webcamName + "Ratio", (int)_ratio);
            SetCameraView();
        } }

    int _deviceIndex = 0;
    public int deviceIndex { get { return _deviceIndex; } }
    string _deviceName = "";
    public string deviceName { get { return _deviceName; } set
        {
            if (value == "Off" || value == "Empty Frame")
            {
                _deviceIndex = _deviceName == "Off" ? -2 : -1;
                _deviceName = value;
                PlayerPrefs.SetString(webcamName + "Device", value);
            }
            else
            {
                for (int i = 0; i < WebCamTexture.devices.Length; i++)
                {
                    if (WebCamTexture.devices[i].name == value)
                    {
                        _deviceIndex = i;
                        _deviceName = value;
                        PlayerPrefs.SetString(webcamName + "Device", value);
                        return;
                    }
                }
                Debug.Log("Camera device named " + value + " not found");
                _deviceIndex = -1;
                _deviceName = "Empty Frame";
            }
            SetState();
        } }


    public bool cameraIsPlaying = false;

    bool waitingForResolution = false;
    float cameraStartCooldown = 0f;
    bool startCameraAfterCooldown = false;

    public static int cameraFPS = 60;
    public static int cameraWidth = 1920;
    public static int cameraHeight = 1080;
    public static bool useCameraDefaults = false;

    static bool commandHooks = false;
    static List<PlayerCameraObserver> allCameras = new List<PlayerCameraObserver>();

    private void Awake()
    {
        if (webcamsEnabled)
        {
            webcam = new WebCamTexture();
        }
        allCameras.Add(this);

        deviceName = PlayerPrefs.GetString(webcamName + "Device", "Empty Frame");
        aspectRatio = (AspectRatio)PlayerPrefs.GetInt(webcamName + "Ratio", 0);
        if (state == WebcamState.On)
            StartCamera();
        ViewModel.onThemeChange.AddListener(OnTheme);
    }
    override public void Start()
    {
        base.Start();
        if (!commandHooks)
        {
            LSConsole.AddCommandHook("setCamera", "query the status of [blueCamera | goldCamera | commentaryCamera]. Add a [deviceName] to switch to switch cam to that device", SetCamera);
            LSConsole.AddCommandHook("cameraList", "shows a list of available camera devices and their resolutions", DeviceList);
            LSConsole.AddCommandHook("cameraQuality", "set camera quality to [low] or [high]", SetCameraQuality);
            LSConsole.AddCommandHook("cameraCustomQuality", "set camera attributes, specify [width height fps] in that order", SetCameraArgs);
            LSConsole.AddCommandHook("cameraDefaults", "use camera defaults instead of custom settings (starts false)", UseCameraDefault);
            commandHooks = true;
        }
        
    }

    public static void ShowPlayerCams(bool val)
    {
        _showPlayerCams = val;
        UpdateAllCameras();
    }

    public static void UpdateAllCameras()
    {
        foreach (var c in allCameras)
        {
            c.SetState();
            c.SetCameraView();
        }
        //gtfrcamerasUpdatedEvent.Invoke();
    }

    static string SetCameraQuality(string[] args)
    {
        int h, w, fps;
        if(args.Length < 1) return "Specify [high] or [low]";
        switch(args[0])
        {
            case "high": h = 1080; w = 1920; fps = 60; break;
            case "low": h = 1280; w = 720; fps = 30; break;
            default: return "Specify [high] or [low]"; break;
        }
        useCameraDefaults = false;
        return SetCameraArgs(w, h, fps);
    }
    static string SetCameraArgs(string[] args)
    {
        int h = 0, w = 0, fps = 0;
        if(args.Length < 3) return "Need 3 args";
        int.TryParse(args[0], out w);
        int.TryParse(args[1], out h);
        int.TryParse(args[2], out fps);

        return SetCameraArgs(w, h, fps);
    }

    static string SetCameraArgs(int w, int h, int fps)
    {
        if(h <= 0 || w <= 0 || fps <= 0) return "invalid arg";
        cameraHeight = h;
        cameraWidth = w;
        cameraFPS = fps;
        useCameraDefaults = false;

        foreach(var c in allCameras)
            c.StartCamera();

        return "";
    }

    static string UseCameraDefault(string[] args)
    {
        useCameraDefaults = true;
        return "";
    }
    static string SetCamera(string[] args)
    {
        string specificCamera = "";
        string ret = "";
        if (args.Length == 0)
        {
            foreach (var cam in allCameras)
                ret += GetCameraStatus(cam.webcamName) + "\n";
        }
        else if (args.Length == 1)
            ret += GetCameraStatus(args[0]);
        else
        {
            var cam = allCameras.Find(x => x.webcamName == args[0]);
            if (cam == null) return "Camera not found!";
            var thisDevice = "";
            for(int i = 1; i < args.Length; i++)
            {
                thisDevice += args[i];
                if (i + 1 < args.Length)
                    thisDevice += " ";
            }
            cam.deviceName = thisDevice;
            cam.SetCameraView();
            cam.StartCamera();
        }
        return ret;

    }

    static string DeviceList(string[] args)
    {
        var ret = "";
        for(int i = 0; i < WebCamTexture.devices.Length; i++)
        {
            var device = WebCamTexture.devices[i];
            ret += i + ":" + WebCamTexture.devices[i].name + "\n";
            if(device.availableResolutions != null)
            {
                foreach(var res in device.availableResolutions)
                {
                    ret += "  " + res.ToString() + "\n";
                }
            }
        }
        return ret;
    }

    public static PlayerCameraObserver GetCamera(string camName)
    {
        foreach(var c in allCameras)
        {
            if (c.webcamName == camName)
                return c;
        }
        return null;
    }
    static string GetCameraStatus(string cameraName)
    {
        string ret = cameraName + ": ";
        var cam = allCameras.Find(x => x.webcamName == cameraName);
        if (cam != null)
        {
            var ar = GetAspectRatio(cameraName);
            ret += "Aspect Ratio: [" + ar.ToString() + "], ";
            if (cam.webcam == null)
                ret += "Texture Disabled";
            else
            {
                ret += "webcam: [" + cam.webcam.deviceName + "], "; 
                for(int i = 0; i < WebCamTexture.devices.Length; i++)
                {
                    if (WebCamTexture.devices[i].name == cam.webcam.deviceName)
                        ret += "id: [" + i + "]";
                }

            }
        } else
        {
            ret += "Camera not found";
        }
        return ret;
    }
    void OnTheme()
    {
        SetState();
        //if(webcamName == "commentaryCamera")
        //    frame.gameObject.SetActive(bgContainer.sprite == null);
    }

    void SetState()
    {
        switch (state)
        {
            case WebcamState.Off:
                gameObject.SetActive(false);
                RemoveWebcamTexture();
                break;
            case WebcamState.FrameOnly:
                gameObject.SetActive(true);
                RemoveWebcamTexture();
                SetCameraView();
                SetIcons();
                break;
            case WebcamState.On:
                gameObject.SetActive(true);
                SetWebcamTexture();
                SetIcons();
                break;
        }

        camerasUpdatedEvent.Invoke();
    }
    public override void OnInvert(bool inverted)
    {
        base.OnInvert(inverted);
        if(targetID >= 0)
        {
            webcamName = team == 0 ? "blueCamera" : "goldCamera";
            SetState();
        }
    }

    void SetIcons()
    {
        if (targetID < 0) return;
        for (int i = 0; i < 5; i++)
        {
            icons[i].gameObject.SetActive(iconsVisible);
            icons[i].sprite = SpriteDB.allSprites[team].playerSprites[4 - i].icon;
            icons[i].GetComponent<RectTransform>().sizeDelta = Vector2.one * (aspectRatio == AspectRatio.Wide ? 100 : 75);
            icons[i].GetComponent<RectTransform>().anchorMax = icons[i].GetComponent<RectTransform>().anchorMin = new Vector2(icons[i].GetComponent<RectTransform>().anchorMin.x, aspectRatio == AspectRatio.Wide ? .875f : .125f);
        }
    }

    public static string GetSourceName(string camName)
    {
        var id = PlayerPrefs.GetInt(camName + "ID");
        return GetSourceName(id);
    }

    public static string GetSourceName(int id)
    {
        if (id >= WebCamTexture.devices.Length) return "Device Not Found";
        if (id == -2) return "Off";
        if (id == -1) return "Empty Frame";

        return WebCamTexture.devices[id].name;
    }
    public static AspectRatio GetAspectRatio(string camName)
    {

        foreach(var c in allCameras)
        {
            if (c.webcamName == camName)
                return c.aspectRatio;
        }
        return AspectRatio.Wide;
    }
    /*
    public static AspectRatio GetAspectRatio(float width, float height)
    {
        if (width <= 0f || height <= 0f) return AspectRatio.Off;
        var ratio = width / height;
        if (ratio <= 1f) return AspectRatio.Square;
        if (ratio <= 1.34f) return AspectRatio.Standard;
        if (ratio <= 1.78f) return AspectRatio.Wide;
        return AspectRatio.Ultrawide;
    }
    */

    public static WebcamState GetWebcamState(string wName)
    {
        if (!showPlayerCams)
            return WebcamState.Off;
        if (!webcamsEnabled)
        {
            return WebcamState.FrameOnly;
        }

        var cam = GetCamera(wName);
        return cam != null ? cam.state : WebcamState.Off;
    }

    public int GetCameraID()
    {
        return GetCameraID(webcamName, _deviceName);
    }
    public static int GetCameraID(string camName, string deviceName)
    {
        for (int i = 0; i < WebCamTexture.devices.Length; i++)
        {
            if (WebCamTexture.devices[i].name == deviceName)
            {
                return i;
            }
        }
        return -1;
    }


    public void StopCamera()
    {
        if (!cameraIsPlaying) return;

        webcam.Stop();
        cameraIsPlaying = false;
        cameraStartCooldown = 1f;
    }

    public void StartCamera()
    {
        if (deviceName == "") return;
        if (showPlayerCams == false && webcamName != "commentaryCamera") return;
        if (state != WebcamState.On) return;
          
        if(cameraIsPlaying)
        {
            Debug.Log("stopping " + deviceName);
            StopCamera();
            startCameraAfterCooldown = true;
            cameraStartCooldown = 1.5f;
            return;
        }

        Debug.Log("starting " + deviceName + " width " + cameraWidth + " defaults " + useCameraDefaults);
        SetWebcamTexture();
        int id = deviceIndex;

        webcam.deviceName = WebCamTexture.devices[id].name;
        if (!useCameraDefaults)
        {
            webcam.requestedFPS = cameraFPS;
            webcam.requestedHeight = cameraHeight;
            webcam.requestedWidth = cameraWidth;
        } else
        {
            //webcam.requestedFPS = 0;
        }
        //webcam.requestedHeight = cameraHeight;
        //webcam.requestedWidth = cameraWidth;

        webcam.Play();
        cameraIsPlaying = true;
        waitingForResolution = true;
        /*
        if (WebCamTexture.devices[id].kind == WebCamKind.UltraWideAngle)
            SetAspectRatio(webcamName, AspectRatio.Ultrawide);
        else
            SetAspectRatio(webcamName, AspectRatio.Wide);
        */
            //SetAspectRatio();

            //cameraImage.SetNativeSize();
        
    }

    void SetCameraView()
    {
        float width = aspectRatio == AspectRatio.Ultrawide ? 3120f : 1920f;
        float height = 1080f;
        /*
        if (webcam != null && state == WebcamState.On && !waitingForResolution)
        {
            width = webcam.width;
            height = webcam.height;
        }
        */
        var wcHeight = aspectRatio == AspectRatio.Ultrawide ? 1.35f : 1.85f;
        var wcWidth = (width / height) * wcHeight;

        GetComponent<Canvas>().GetComponent<RectTransform>().sizeDelta = new Vector2(wcWidth, wcHeight);

        var frameHeight = aspectRatio == AspectRatio.Ultrawide ? 1.52f : 2.08f;
        var frameWidth = (width / height) * frameHeight;
        frame.size = new Vector2(frameWidth, frameHeight + (aspectRatio == AspectRatio.Ultrawide ? .08f : 0f));

        

        camerasUpdatedEvent.Invoke();
    }

    RenderTexture clippedCamTexture;
    Rect crop;

    void SetWebcamTexture()
    {
        if (cameraImage == null) return;

        if (clippedCamTexture != null)
            Destroy(clippedCamTexture);

        cameraImage.enabled = true;

        if (aspectRatio == AspectRatio.Wide)
        {
            cameraImage.texture = webcam;
            //cameraImage.material.mainTexture = webcam;
        }
        else
        {
            var width = 1920;
            float originalHeight = 1080f;
            float height = aspectRatio == AspectRatio.Wide ? webcam.requestedHeight : Mathf.FloorToInt(width / 3.2f);
            clippedCamTexture = new RenderTexture(width, (int)height, 24);
            crop = new Rect(0f, ((originalHeight - height) / originalHeight) / 2f, 1f, (height / originalHeight));
            cameraImage.texture = clippedCamTexture;
            //cameraImage.material.mainTexture = clippedCamTexture;
        }
    }

    void RemoveWebcamTexture()
    {
        if (cameraImage == null) return;

        cameraImage.texture = null;
        cameraImage.material.mainTexture = null;
        cameraImage.enabled = false;
    }

    private void Update()
    {
        if(cameraStartCooldown > 0f)
        {
            cameraStartCooldown -= Time.deltaTime;
            if(cameraStartCooldown <= 0 && startCameraAfterCooldown)
            {
                Debug.Log("starting " + deviceName);
                startCameraAfterCooldown = false;
                StartCamera();
            }
        }
        if (waitingForResolution && cameraIsPlaying)
        {
            if (webcam.width >= 100)
            {
                waitingForResolution = false;
                Debug.Log(webcamName + " reported resolution: " + webcam.width + "x" + webcam.height + " @ " + webcam.requestedFPS + " fps");
                SetCameraView();
            }
        }
        if (state == WebcamState.On && aspectRatio == AspectRatio.Ultrawide)
            TextureCropTools.CropWithPctRect(webcam, crop, clippedCamTexture);
            //clippedCamTexture = cameraImage.material.mainTexture = clippedWebcam;
    }

    public override void OnParameters()
    {
        base.OnParameters();

        if (moduleParameters.ContainsKey("hideBackground"))
        {
            frame.gameObject.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        allCameras.Remove(this);
    }
}

