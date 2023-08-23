using UnityEngine;
using System.Collections;

public class UnityCaptureTexture : MonoBehaviour
{
	RenderTexture source;

	UnityCapture.Interface captureInterface;

	public static UnityCaptureTexture instance;
	public static bool captureEnabled = false;

    private void Awake()
    {
		instance = this;
    }
    // Use this for initialization
    void Start()
	{
		
	}

	public static void Init() => instance._Init();
	public void _Init()
    {
		if (captureEnabled) return;

		source = new RenderTexture(1920, 1080, 24);
		Camera.main.targetTexture = source;

		captureInterface = new UnityCapture.Interface(UnityCapture.ECaptureDevice.CaptureDevice1);

		captureEnabled = true;
	}
	// Update is called once per frame
	void Update()
	{
		if(captureEnabled)
			captureInterface.SendTexture(source);
	}
}

