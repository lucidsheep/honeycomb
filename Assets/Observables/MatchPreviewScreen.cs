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

	public enum StatType { MilitaryKD, Berries, Snail, BestMap, WorstMap }
	StatType curStatDisplay = StatType.WorstMap;
	MatchPreviewStatPanel statPanel;
	int numStats = 5;
	float statCycleTimer;

	bool screenVisible = false;
	TournamentTeamData blueTeamData = new TournamentTeamData();
	TournamentTeamData goldTeamData = new TournamentTeamData();
	TeamTournamentStats blueTeamGameData = new TeamTournamentStats(0);
	TeamTournamentStats goldTeamGameData = new TeamTournamentStats(0);
	HMMatchState curMatchData = new HMMatchState();
	Tweener showHideAnim;
	static MatchPreviewScreen instance;

    private void Awake()
    {
		instance = this;
		statPanel = GetComponentInChildren<MatchPreviewStatPanel>();
    }

    // Use this for initialization
    override public void Start()
	{
		base.Start();
		GameModel.onGameStart.AddListener(OnGameStart);
		GameModel.onDelayedTournamentData.AddListener(OnTournamentData);
		NetworkManager.instance.onTournamentTeamIDs.AddListener(OnTournamentTeamIDs);
		NetworkManager.instance.onTournamentTeamWinLossData.AddListener(OnTournamentTeamDetail);
		NetworkManager.instance.onTeamGameData.AddListener(OnTeamGameData);
		NetworkManager.instance.onTournamentTeamPlayers.AddListener(OnTeamPlayerData);
		for (int i = 0; i < 5; i++)
        {
			bluePlayers[i].avatar.SetLayer(i + 10);
			goldPlayers[i].avatar.SetLayer(i + 10);
        }
		OnThemeChange();
		fade.SetFadeSubjects();
		fade.SetBaseAlpha(bgContainer, bgContainer.color.a);
		fade.alpha = 0f;
	}

    private void Update()
    {
        if(statPanel != null && screenVisible)
        {
			statCycleTimer -= Time.deltaTime;
			if(statCycleTimer <= 0f)
            {
				statCycleTimer += 6f;
				curStatDisplay = (StatType)((int)curStatDisplay + 1);
				if ((int)curStatDisplay >= numStats)
					curStatDisplay = 0;
				string blue, gold, center;
				switch(curStatDisplay)
                {
					case StatType.MilitaryKD:
						center = "Military K/D";
						blue = blueTeamGameData.militaryKills + "-" + blueTeamGameData.militaryDeaths;
						gold = goldTeamGameData.militaryKills + "-" + goldTeamGameData.militaryDeaths;
						break;
					case StatType.Berries:
						center = "Berries Run";
						blue = blueTeamGameData.berries.ToString();
						gold = goldTeamGameData.berries.ToString();
						break;
					case StatType.Snail:
						center = "Snail Meters";
						blue = blueTeamGameData.snailLengths.ToString();
						gold = goldTeamGameData.snailLengths.ToString();
						break;
					case StatType.BestMap:
						center = "Best Map";
						blue = blueTeamGameData.GetMap(true);
						gold = goldTeamGameData.GetMap(true);
						break;
					case StatType.WorstMap:
						center = "Worst Map";
						blue = blueTeamGameData.GetMap(false);
						gold = goldTeamGameData.GetMap(false);
						break;
					default: center = blue = gold = ""; break;
				}
				statPanel.SetDisplay(center, blue, gold);
            }
        }
    }

    override protected void OnThemeChange()
	{
		var pos = new Vector3(-1.64f, .96f, 0f);
		float scale = 1f;
		switch (ViewModel.currentTheme.GetLayout())
		{
			case ThemeDataJson.LayoutStyle.OneCol_Right: break;
			case ThemeDataJson.LayoutStyle.OneCol_Left:
				pos.x *= -1f;
				break;
			case ThemeDataJson.LayoutStyle.TwoCol:
				//hack for extra space with camp frame. need to include parameter to let theme adjust this
				//float campBump = (ViewModel.currentTheme.themeName == "caxmpkq" || ViewModel.currentTheme.themeName == "postcamp") ? .18f : 0f;
				pos.x = 0f;
				pos.y = 0.36f;
				scale = .88f;
				break;
			case ThemeDataJson.LayoutStyle.Game_Only:
				pos = new Vector3(0f, -.12f, 0f);
				scale = 1.26f;
				break;
		}
		pos.y += ViewModel.bottomBarPadding.property;
		transform.localPosition = pos;
		transform.localScale = Vector3.one * scale;

		if(ViewModel.currentTheme.matchPreview.useCustomPosition)
        {
			transform.localPosition = new Vector3(ViewModel.currentTheme.matchPreview.customPositionX, ViewModel.currentTheme.matchPreview.customPositionY, 0f);
			transform.localScale = Vector3.one * ViewModel.currentTheme.matchPreview.customScale;
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

		blueTeamGameData = new TeamTournamentStats(blueTeamData.id);
		goldTeamGameData = new TeamTournamentStats(goldTeamData.id);

		for(int i = 0; i < 5; i++)
        {
			bluePlayers[i].Clear();
			goldPlayers[i].Clear();
        }

		OnTeamPlayerData(blueTeamData.id);
		OnTeamPlayerData(goldTeamData.id);

	}

	void OnTeamPlayerData(int teamID)
    {
		if (blueTeamData.id != teamID && goldTeamData.id != teamID) return;

		var playerData = PlayerStaticData.GetTournamentPlayers(teamID);
		var arrToUse = blueTeamData.id == teamID ? bluePlayers : goldPlayers;

		for (int i = 0; i < 5; i++)
		{
			if (playerData.Count > i)
				arrToUse[i].Init(playerData[i]);
		}
	}

	void OnTournamentTeamDetail(int id, int wins, int losses)
    {
		TournamentTeamData toUse = id == blueTeamData.id ? blueTeamData : id == goldTeamData.id ? goldTeamData : null;
		if (toUse == null) return;
		toUse.wins = wins;
		toUse.losses = losses;
		UpdateUI();
    }

	private void OnTeamGameData(TeamGameStats data)
	{
		var dataToUse = data.teamID == blueTeamData.id ? blueTeamGameData : goldTeamGameData;
		dataToUse.AddGame(data);
	}

	void OpenScreen(HMMatchState matchData)
    {
		curMatchData = matchData;
		screenVisible = true;
		if (showHideAnim != null) showHideAnim.Complete();
		showHideAnim = DOTween.To(() => fade.alpha, x => fade.alpha = x, 1f, .5f);
		UpdateUI();
    }

	void UpdateUI()
    {
		blueTeamScore.text = blueTeamData.wins + "-" + blueTeamData.losses;
		goldTeamScore.text = goldTeamData.wins + "-" + goldTeamData.losses;

		if(curMatchData != null && curMatchData.current_match != null)
        {
			blueTeamName.text = curMatchData.current_match.blue_team;
			goldTeamName.text = curMatchData.current_match.gold_team;
			roundName.text = curMatchData.current_match.round_name;
			winCondition.text = (curMatchData.current_match.wins_per_match > 0) ? "Best of " + ((curMatchData.current_match.wins_per_match * 2) - 1)
															 : "Straight " + curMatchData.current_match.rounds_per_match;
		}
		else
        {
			blueTeamName.text = goldTeamName.text = roundName.text = winCondition.text = "";
        }

	}
	void CloseScreen()
    {
		screenVisible = false;
		if (statPanel != null) statPanel.HideDisplay();
		if (showHideAnim != null) showHideAnim.Complete();
		showHideAnim = DOTween.To(() => fade.alpha, x => fade.alpha = x, 0f, .5f);
	}

	public static void ForceClosePreview()
    {
		instance.CloseScreen();
    }
}

