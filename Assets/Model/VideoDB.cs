using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;

public class VideoDB : MonoBehaviour
{
	[System.Serializable]
	public struct TeamVideos
	{
		public VideoClip replayClip;
		public VideoClip replayClip_military;
		public VideoClip replayClip_economic;
		public VideoClip replayClip_snail;
		public VideoClip postgame;
	}

	[System.Serializable]
	public struct VideoSet
    {
		public TeamVideos blueVideos;
		public TeamVideos goldVideos;
		public string name;
    }
	public static VideoDB instance;

	public List<VideoSet> _videoDatabase;

	public static List<VideoSet> videoDatabase => instance._videoDatabase;

	// Use this for initialization

	private void Awake()
	{
		instance = this;
	}

	public static VideoSet GetVideoSet(string setName)
    {
		var ret = videoDatabase.Find(x => x.name == setName);
		Debug.Log("video set name " + ret.name);
		if (ret.name != default(string)) return ret;
		Debug.Log("fallback");
		//fallback to classic set if theme isn't found
		return videoDatabase.Find(x => x.name == "classic");
    }

	public static VideoSet GetVideoSet()
    {
		return GetVideoSet(ViewModel.currentTheme.videoSetName);
    }

	public static TeamVideos blueVideos { get { return GetVideoSet().blueVideos; } }
	public static TeamVideos goldVideos { get { return GetVideoSet().goldVideos; } }
}

