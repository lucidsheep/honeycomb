using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;
using System;

public class MatchPreviewScreen : KQObserver
{
	public class TournamentTeamData
    {
		public int id;
		public int wins;
		public int losses;
    }
	public TextMeshPro roundName, winCondition, blueTeamName, goldTeamName, blueTeamScore, goldTeamScore;
	public MatchPreviewPlayer[] bluePlayers;
	public MatchPreviewPlayer[] goldPlayers;
	public GlobalFade fade;

	bool screenVisible = false;
	TournamentTeamData blueTeamData = new TournamentTeamData();
	TournamentTeamData goldTeamData = new TournamentTeamData();

	Tweener showHideAnim;
	static MatchPreviewScreen instance;

    private void Awake()
    {
		instance = this;
    }

    // Use this for initialization
    override public void Start()
	{
		base.Start();
		GameModel.onGameStart.AddListener(OnGameStart);
		GameModel.onDelayedTournamentData.AddListener(OnTournamentData);
		NetworkManager.instance.onTournamentTeamIDs.AddListener(OnTournamentTeamIDs);
		NetworkManager.instance.onTournamentTeamWinLossData.AddListener(OnTournamentTeamDetail);
		for(int i = 0; i < 5; i++)
        {
			bluePlayers[i].avatar.SetLayer(i + 10);
			goldPlayers[i].avatar.SetLayer(i + 10);
        }
		OnThemeChange();
		fade.SetFadeSubjects();
		fade.SetBaseAlpha(bgContainer, bgContainer.color.a);
		fade.alpha = 0f;
	}

	override protected void OnThemeChange()
	{
		var pos = new Vector3(-1.64f, .96f, 0f);
		float scale = 1f;
		switch (ViewModel.currentTheme.layout)
		{
			case ThemeData.LayoutStyle.OneCol_Right: break;
			case ThemeData.LayoutStyle.OneCol_Left:
				pos.x *= -1f;
				break;
			case ThemeData.LayoutStyle.TwoCol:
				//hack for extra space with camp frame. need to include parameter to let theme adjust this
				//float campBump = (ViewModel.currentTheme.themeName == "campkq" || ViewModel.currentTheme.themeName == "postcamp") ? .18f : 0f;
				pos.x = 0f;
				pos.y = 0.36f;
				scale = .88f;
				break;
			case ThemeData.LayoutStyle.Game_Only:
				pos = new Vector3(0f, -.12f, 0f);
				scale = 1.26f;
				break;
		}
		pos.y += ViewModel.bottomBarPadding.property;
		transform.localPosition = pos;
		transform.localScale = Vector3.one * scale;

		if (ViewModel.currentTheme.postgameHeaderFont != "")
		{
			blueTeamName.font = goldTeamName.font = FontDB.GetFont(ViewModel.currentTheme.postgameHeaderFont);
		}
	}
	private void OnTournamentData(HMMatchState data)
    {
		if (data.current_match == null || data.current_match.id <= 0) //null tournament data, do not show preview screen
			return;

		if(!GameModel.instance.gameIsRunning)
        {
			PostGameScreen.ForceClosePostgame();
			OpenScreen(data);
		}
    }

	void OnGameStart()
    {
		if (screenVisible)
			CloseScreen();
    }

	void OnTournamentTeamIDs(int blue, int gold)
    {
		blueTeamData.id = blue;
		goldTeamData.id = gold;
    }

	void OnTournamentTeamDetail(int id, int wins, int losses)
    {
		TournamentTeamData toUse = id == blueTeamData.id ? blueTeamData : id == goldTeamData.id ? goldTeamData : null;
		if (toUse == null) return;
		toUse.wins = wins;
		toUse.losses = losses;
    }
	void OpenScreen(HMMatchState matchData)
    {
		blueTeamName.text = matchData.current_match.blue_team;
		goldTeamName.text = matchData.current_match.gold_team;
		blueTeamScore.text = blueTeamData.wins + "-" + blueTeamData.losses;
		goldTeamScore.text = goldTeamData.wins + "-" + goldTeamData.losses;
		roundName.text = matchData.current_match.round_name;
		winCondition.text = (matchData.current_match.wins_per_match > 0) ? "Best of " + ((matchData.current_match.wins_per_match * 2) - 1)
																		 : "Straight " + matchData.current_match.rounds_per_match;
		screenVisible = true;
		var blueTeamPlayerData = PlayerStaticData.GetTournamentPlayers(blueTeamData.id);
		var goldTeamPlayerData = PlayerStaticData.GetTournamentPlayers(goldTeamData.id);

		for(int i = 0; i < 5; i++)
        {
			if (blueTeamPlayerData.Count > i)
				bluePlayers[i].Init(blueTeamPlayerData[i]);
			else
				bluePlayers[i].Clear();

			if (goldTeamPlayerData.Count > i)
				goldPlayers[i].Init(goldTeamPlayerData[i]);
			else
				goldPlayers[i].Clear();
		}
		if (showHideAnim != null) showHideAnim.Complete();
		showHideAnim = DOTween.To(() => fade.alpha, x => fade.alpha = x, 1f, .5f);
    }
	void CloseScreen()
    {
		screenVisible = false;
		if (showHideAnim != null) showHideAnim.Complete();
		showHideAnim = DOTween.To(() => fade.alpha, x => fade.alpha = x, 0f, .5f);
	}

	public static void ForceClosePreview()
    {
		instance.CloseScreen();
    }
}

