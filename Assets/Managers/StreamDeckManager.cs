using UnityEngine;
using System.Collections.Generic;
using StreamDeckSharp;
using System;
using OpenMacroBoard.SDK;

public class StreamDeckManager : MonoBehaviour
{
	public enum StreamDeckSize { Unknown, Small, Medium, Large }

	public static bool streamDeckActive = false;
	public static StreamDeckSize size = StreamDeckSize.Unknown;
	public static Vector2Int dimensions = new Vector2Int();

	static StreamDeckManager instance;
	static Dictionary<int, StreamDeckCallback> buttons = new Dictionary<int, StreamDeckCallback>();

	// Use this for initialization
	void Awake()
	{
		instance = this;
		try
		{
			using (var deck = StreamDeck.OpenDevice())
			{
				Debug.Log("Stream deck dectected with button grid " + deck.Keys.CountX + "x" + deck.Keys.CountY);
				if (deck.Keys.Count <= 6) size = StreamDeckSize.Small;
				else if (deck.Keys.Count <= 15) size = StreamDeckSize.Medium;
				else size = StreamDeckSize.Large;
				dimensions = new Vector2Int(deck.Keys.CountX, deck.Keys.CountY);
				deck.KeyStateChanged += OnKeyStateChanged;
				streamDeckActive = true;
			}
		} catch (StreamDeckSharp.Exceptions.StreamDeckNotFoundException ex)
        {
			Debug.Log("Stream Deck not found");
        }
	}

	public static void SetKeyImage(int keyID, string imageName)
	{
		var kbm = KeyBitmap.Create.FromFile(AppLoader.GetAssetPath(imageName));
		try
		{
			using (var deck = StreamDeck.OpenDevice())
			{
				deck.SetKeyBitmap(keyID, kbm);
			}
		} catch (StreamDeckSharp.Exceptions.StreamDeckNotFoundException ex) {}
	}
	public delegate void StreamDeckCallback();
	public static void RegisterDeckButton(Vector2Int pos, StreamDeckCallback callback)
    {
		if (pos.x >= dimensions.x || pos.y >= dimensions.y)
		{
			Debug.Log("Cannot add Stream Deck key: invalid coordinates");
			return;
		}
		int index = pos.y * dimensions.x + pos.x;
		buttons.Add(index, callback);
	} 
    private void OnKeyStateChanged(object sender, OpenMacroBoard.SDK.KeyEventArgs e)
    {
		Debug.Log("Key changed");

		if(e.IsDown)
			Debug.Log("Key " + e.ToString() + " is down");

		if (!e.IsDown || !buttons.ContainsKey(e.Key)) return;

		buttons[e.Key].Invoke();
    }

    // Update is called once per frame
    void Update()
	{
			
	}
}

