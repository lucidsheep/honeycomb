using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;
using System;

public class VideoClipper : MonoBehaviour
{
	public int numTotalClips = 4;
	public int numPriorityClips = 1;
	public int numClipFrames = 400;
	public int numBufferedFrames = 30;

	Texture videoSource;
	WebCamTexture webcam;
	static VideoClipper instance;
	static RenderTexture blitTex;

	public static RenderTexture bufferedFrame { get
        {
			if (instance == null || instance.curClip == null) return null;
			return instance.curClip.GetPastFrame(instance.numBufferedFrames);
		} }
	public static int frameBuffer { get { return instance.numBufferedFrames; } set { instance.numBufferedFrames = value; } }
	Vector2Int videoResolution;
	bool isRecording = false;

	public class Clip
	{
		RenderTexture[] texArray;
		int startIndex;
		int curIndex;
		public bool AdvancePlayback()
		{
			curIndex = curIndex + 1 >= texArray.Length ? 0 : curIndex + 1;
			return curIndex == startIndex;
		}
		public RenderTexture GetPastFrame(int numFrames)
        {
			var pastFrame = numFrames <= curIndex ?
				curIndex - numFrames
				: texArray.Length + curIndex - numFrames;
			return texArray[pastFrame];
        }
		public RenderTexture texture
		{
			get { return texArray[curIndex]; }
			set { texArray[curIndex] = value; }
		}
		public void FinalizeClip()
		{
			startIndex = curIndex;
		}
		public Clip(int clipLength, int width, int height, GraphicsFormat format)
		{
			texArray = new RenderTexture[clipLength];
			for (int i = 0; i < clipLength; i++)
			{
				texArray[i] = new RenderTexture(width, height, 0, format);
			}
			curIndex = startIndex = 0;
		}
		public void Release()
        {
			foreach (var texture in texArray)
				Destroy(texture);
        }
	}
	// 0 to (priorityClips - 1) = priority clip reserves
	// priorityClips to (length - 1) = normal clips
	List<Clip> clips = new List<Clip>();
	int curClipIndex = 0;

	Clip curClip;

	public static bool canDownResImage = false;
	public static bool canCopyTexture = false;
	// Use this for initialization
	void Awake()
	{
		instance = this;
		Debug.Log("Texture Copy Support: " + SystemInfo.copyTextureSupport.ToString());
		canDownResImage = (SystemInfo.copyTextureSupport & UnityEngine.Rendering.CopyTextureSupport.DifferentTypes) == UnityEngine.Rendering.CopyTextureSupport.DifferentTypes;
		canCopyTexture = ((SystemInfo.copyTextureSupport & UnityEngine.Rendering.CopyTextureSupport.RTToTexture) == UnityEngine.Rendering.CopyTextureSupport.RTToTexture &&
			(SystemInfo.copyTextureSupport & UnityEngine.Rendering.CopyTextureSupport.TextureToRT) == UnityEngine.Rendering.CopyTextureSupport.TextureToRT);

	}

    private void Start()
    {
		LSConsole.AddCommandHook("graphicsInfo", "prints out a list of info about graphics capabilities of this machine", GraphicsFormatCommand);
	}
	string GraphicsFormatCommand(string[] args)
    {
		return "Texture Copy Support: " + SystemInfo.copyTextureSupport.ToString() + "\n"
			+ "webcam format: " + WebcamController.gameCamera.GetWebcamTexture.graphicsFormat.ToString() + "\n"
			+ "graphics card: " + SystemInfo.graphicsDeviceName;
	}
	public void SetSource(WebCamTexture wc)
	{ 
		int ratio = canDownResImage ? 2 : 1;
		int width = wc.width / ratio;
		int height = wc.height / ratio;
		Debug.Log("webcam res: " + width + "x" + height + " format: " + wc.graphicsFormat.ToString() + " mipmaps: " + wc.mipmapCount);
		videoSource = wc;
		blitTex = new RenderTexture(width, height, 24);
		for (int i = 0; i < numTotalClips; i++)
		{
			clips.Add(new Clip(numClipFrames, width, height, wc.graphicsFormat));
		}
		curClipIndex = numPriorityClips;
		curClip = clips[curClipIndex];
		isRecording = true;
	}
	public void SetSource(Texture source, int width, int height)
    {
		Debug.Log(width + "x" + height);

		videoSource = source;
		blitTex = new RenderTexture(width, height, 24);
		for(int i = 0; i < numTotalClips; i++)
        {
			clips.Add(new Clip(numClipFrames, width, height, source.graphicsFormat));
        }
		curClipIndex = numPriorityClips;
		curClip = clips[curClipIndex];
		isRecording = true;
    }
	void FixedUpdate()
	{
		if(isRecording)
        {

			if (canDownResImage)
			{
				Graphics.Blit(videoSource, curClip.texture);
			}
			else
				Graphics.CopyTexture(videoSource, curClip.texture);

			curClip.AdvancePlayback();
		}
	}


	public void SaveClip(int priorityIndex = -1)
    {
		curClip.FinalizeClip();
		if(priorityIndex > -1)
        {
			//swap arrays around
			clips[curClipIndex] = clips[priorityIndex];
			clips[priorityIndex] = curClip;
			curClip = clips[curClipIndex];
        } else
        {
			curClipIndex = curClipIndex + 1 >= clips.Count ? numPriorityClips : curClipIndex + 1;
			curClip = clips[curClipIndex];
        }
    }

	public List<Clip> GetAllClips() { return clips; }
	public Clip GetClip(int index) { return clips[index]; }

    private void OnDestroy()
    {
		foreach (var clip in clips)
			clip.Release();
    }
}

