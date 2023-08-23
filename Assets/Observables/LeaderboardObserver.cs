using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;

public class LeaderboardObserver : MonoBehaviour
{
	public enum Mode { Menu, JPoints }

	public class JPointsData : IComparable
	{
		public int playerID;
		public string playerName;
		public int jasonPoints;
		public JPointsData(int pid, string pName) { playerID = pid; playerName = pName; jasonPoints = 1; }

        public int CompareTo(object obj)
        {
			if (obj is JPointsData)
				return ((JPointsData)obj).jasonPoints.CompareTo(this.jasonPoints);
			return 0;
		}
    }

	public TextMeshPro header;
	public TextMeshPro text;

	List<JPointsData> jasonPointLeaderboard = new List<JPointsData>();

	Mode curMode = Mode.Menu;

	const int jason = 166; //jason
	const string menuText = "Make Up a Handicap <size=2><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>$1</b></mark></size>\nPick a Team <size=2><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>$5</b></mark></size>\nBlindfold Player <size=2><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>$5 </b></mark></size>\nDon't Drink and Fly Set <size=2><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>$5</b></mark></size>\nCustom Rules Set <size=2><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>$10</b></mark></size>\nBUBG Tournament <size=2><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>$10</b></mark></size>\nMeatball Set <size=2><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>$10</b></mark></size>\nAdd Fog <size=2><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>$1/$10 ALL GATES</b></mark></size>\nKiller Karaoke <size=2><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>$10</b></mark></size>\nTrue Darkness Set <size=2><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>$25</b></mark></size>";
	// Use this for initialization
	void Start()
	{
		GameModel.onGameEvent.AddListener(OnGameEvent);
	}

	void OnGameEvent(string eType, GameEventData data)
    {
		if(eType == GameEventType.PLAYER_KILL)
        {
			int targetID = GameModel.GetPlayer(1 - data.teamID, data.targetID).hivemindID;
			if(targetID == jason) //jason is dead :(
            {
				int killerID = GameModel.GetPlayer(data.teamID, data.playerID).hivemindID;
				string killerName = GameModel.GetPlayer(data.teamID, data.playerID).playerName.property;
				if (killerID > 0 && killerName != "")
                {
					var leaderboardEntry = jasonPointLeaderboard.Find(x => x.playerID == killerID);
					if(leaderboardEntry != null)
                    {
						leaderboardEntry.jasonPoints++;
                    } else
                    {
						jasonPointLeaderboard.Add(new JPointsData(killerID, killerName));
                    }
                }
				if (curMode == Mode.JPoints) UpdateDisplay();
			}
        }
    }

	void UpdateDisplay()
    {
		switch(curMode)
        {
			case Mode.Menu: text.text = menuText; header.text = "Donation Incentives"; break;
			case Mode.JPoints:
				header.text = "Jason Points™";
				jasonPointLeaderboard.Sort();
				string txt = "";
				for(int i = 0; i < 9; i++)
                {
					if (i >= jasonPointLeaderboard.Count) break;
					txt += jasonPointLeaderboard[i].playerName + ": " + jasonPointLeaderboard[i].jasonPoints + "\n";
                }
				//very important
				txt += "Jason G: 0";
				text.text = txt;
				break;

        }
    }
	// Update is called once per frame
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.J) && !InputModeManager.inputModeEnabled && !SetupScreen.setupInProgress)
        {
			if (curMode == Mode.Menu) curMode = Mode.JPoints;
			else curMode = Mode.Menu;
			UpdateDisplay();
        }
	}
}

