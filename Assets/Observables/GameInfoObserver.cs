using UnityEngine;
using System.Collections;
using System;
using TMPro;

public class GameInfoObserver : MonoBehaviour
{
	public TextMeshPro time;
	public TextMeshPro id;

	int mins = -1;
	int curGameID = 0;
	bool newID = false;

	// Use this for initialization
	void Start()
	{
		NetworkManager.instance.onGameID.AddListener(OnGameID);
		SetTime();
	}

	void OnGameID(int id)
    {
		curGameID = id;
		newID = true;
    }
	// Update is called once per frame
	void Update()
	{
		if (mins != DateTime.Now.Minute)
			SetTime();
		if (newID)
			SetID();
	}

	void SetTime()
    {
		time.text = DateTime.Now.ToShortTimeString();
		mins = DateTime.Now.Minute;
    }

	void SetID()
    {
		id.text = "Game " + curGameID;
		newID = false;
    }
}

