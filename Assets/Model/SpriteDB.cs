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

	public static Sprite GetIcon(int team, int position)
    {
		string name = team == 0 ? "blue" : "gold";
		switch(position)
        {
			case 0: name += "Stripes"; break;
			case 1: name += "Abs"; break;
			case 2: name += "Queen"; break;
			case 3: name += "Skulls"; break;
			case 4: name += "Chex"; break;
			default: break;
        }
		var ret = AppLoader.GetStreamingSprite(name);
		if (ret != null) return ret;

		return allSprites[team].playerSprites[position].icon;

    }
}

