using UnityEngine;
using System.Collections.Generic;

public class FPSCalc : MonoBehaviour
{
    public int framesToCount = 10;

    LSRollingAverageFloat frames;
    int index = 0;

    static FPSCalc instance;

    private void Awake()
    {
        instance = this;
        frames = new LSRollingAverageFloat(framesToCount, Application.targetFrameRate);
    }

    public static float fps { get { return instance.frames.average; } }

    private void Update()
    {
        frames.AddValue((1f / Time.unscaledDeltaTime) + 1f);
        //Debug.Log("fps" + fps);
    }
}