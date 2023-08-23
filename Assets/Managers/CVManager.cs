using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CVManager : MonoBehaviour
{
    public Scanner_Ego scanner;

    public static CVManager instance;
    public static bool useManager = false;
    public static float snailObservedPosition = 0f;

    public static bool useFrameBuffer = true;
    public static LSRollingAverageFloat inferenceTimes;

    static int frameBufferOffset = 0;

    private void Awake()
    {
        instance = this;
        inferenceTimes = new LSRollingAverageFloat(10, 0f);
    }
    void Start()
    {
        SetupScreen.onSetupComplete.AddListener(OnSetupComplete);

        LSConsole.AddCommandHook("cvModule", "[start] or [stop] the CV module. Hats.", CVToggle);
        LSConsole.AddCommandHook("cvFrameBuffer", "[enable] or [disable] CV frane buffer. Can also set a number of offset frames [0+].", CVBuffer);
    }

    string CVToggle(string[] options)
    {
        if (options.Length == 0) scanner.inferenceMode = !scanner.inferenceMode;
        else scanner.inferenceMode = options[0] == "start" ? true : false;
        useManager = scanner.inferenceMode;
        return "";
    }

    string CVBuffer(string [] args)
    {
        if (args.Length == 0) useFrameBuffer = !useFrameBuffer;
        else
        {
            int maybeOffset = -1;
            int.TryParse(args[0], out maybeOffset);
            if (maybeOffset >= 0)
                frameBufferOffset = maybeOffset;
            else
                useFrameBuffer = args[0].Contains("enable") ? true : false;
        }

        return "";
    }

    void OnSetupComplete()
    {
        scanner.Init(WebcamController.gameCamera, PlayerPrefs.GetInt("computerVision") == 1);
        useManager = scanner.inferenceMode;
    }

    public static void OnInferenceComplete(float duration)
    {
        inferenceTimes.AddValue(duration);
        if (useFrameBuffer && instance.scanner.inferenceMode)
            VideoClipper.frameBuffer = Mathf.Min(30, Mathf.FloorToInt((inferenceTimes.average + frameBufferOffset) / (1f / 60f)));
        else
            VideoClipper.frameBuffer = 0;
    }
    // Update is called once per frame
    void Update()
    {
        //extra insurance to stop frameBuffer when exiting CV mode
        if (VideoClipper.frameBuffer > 0 && !instance.scanner.inferenceMode)
            VideoClipper.frameBuffer = 0;
    }
}
