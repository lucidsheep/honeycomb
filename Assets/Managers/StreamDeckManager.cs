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
	IMacroBoard deck;
	// Use this for initialization
	void Awake()
	{
		instance = this;
		if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
		{
			Debug.Log("Streamdeck not supported in Editor Mode");
			return; // not supported for Editor due to an AppDomain bug: https://github.com/OpenMacroBoard/StreamDeckSharp/issues/53
		}
		try
		{
			deck = StreamDeck.OpenDevice();
			Debug.Log("Stream deck dectected with button grid " + deck.Keys.CountX + "x" + deck.Keys.CountY);
			deck.ClearKeys();
			if (deck.Keys.Count <= 6) size = StreamDeckSize.Small;
			else if (deck.Keys.Count <= 15) size = StreamDeckSize.Medium;
			else size = StreamDeckSize.Large;
			dimensions = new Vector2Int(deck.Keys.CountX, deck.Keys.CountY);
			deck.KeyStateChanged += OnKeyStateChanged;
			streamDeckActive = true;
			ViewModel.OnStreamDeckConnected();
		} catch (StreamDeckSharp.Exceptions.StreamDeckNotFoundException ex)
        {
			Debug.Log("Stream Deck not found");
        }
	}

	public static void SetKeyImage(int keyID, string imageName)
	{
		var kbm = KeyBitmap.Create.FromFile(Application.streamingAssetsPath + "/" + imageName + ".png");
		instance.deck.SetKeyBitmap(keyID, kbm);
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

		Debug.Log("Streamdeck function registered on key " + pos.x + "x" + pos.y + ", index " + index);
	}
		bool buttonWasPressed = false;
		int kID = -1;
		bool isDown = false;
    private void OnKeyStateChanged(object sender, OpenMacroBoard.SDK.KeyEventArgs e)
    {

		buttonWasPressed = true;
		kID = e.Key;
		isDown = e.IsDown;

    }

    // Update is called once per frame
    void Update()
	{
			if(buttonWasPressed)
			{
				buttonWasPressed = false;

				if (buttons.ContainsKey(kID) && isDown)
					buttons[kID].Invoke();
			}
	}


	void OnDestroy()
	{
		if(streamDeckActive)
		{
				deck.KeyStateChanged -= OnKeyStateChanged;
				streamDeckActive = false;
				deck.Dispose();
		}
	}
}
