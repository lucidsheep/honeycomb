using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;

public class SpriteDB : MonoBehaviour
{
	[System.Serializable]
	public struct TeamSprites
	{
		public PlayerSprites[] playerSprites;
	}

	public static SpriteDB instance;

	public TeamSprites[] _allSprites;

	public static TeamSprites[] allSprites { get { return instance._allSprites; } }

	[System.Serializable]
	public class HatDBItem
    {
		public string validEmoji;
		public int hivemindID;
		public Sprite hat;
    }
		//: Dictionary<string, Sprite> { };

	public List<HatDBItem> _hatDatabase;

	public static List<HatDBItem> hatDatabase => instance._hatDatabase;

    // Use this for initialization

    private void Awake()
    {
		instance = this;
    }
    void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
			
	}
}

