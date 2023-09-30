using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;
using System;

public class MatchPreviewScreen : KQObserver
{
	public TextMeshPro roundName, winCondition, blueTeamName, goldTeamName, blueTeamScore, goldTeamScore;
	public MatchPreviewPlayer[] bluePlayers;
	public MatchPreviewPlayer[] goldPlayers;

	float timeSinceLastPostgame = 99f;
	bool screenVisible = false;

	// Use this for initialization
	void Start()
	{
		GameModel.onGameStart.AddListener(OnGameStart);
		GameModel.onDelayedTournamentData.AddListener(OnTournamentData);
	}

    private void OnTournamentData(HMMatchState data)
    {
		if(!GameModel.instance.gameIsRunning)
        {
			PostGameScreen.ForceClosePostgame();
			OpenScreen(data);
		}
    }

    // Update is called once per frame
    void Update()
	{
			
	}

	void OnGameStart()
    {
		timeSinceLastPostgame = 0f;
		if (screenVisible)
			CloseScreen();
    }

	void OpenScreen(HMMatchState matchData)
    {
		screenVisible = true;
    }
	void CloseScreen()
    {
		screenVisible = false;
    }
}

