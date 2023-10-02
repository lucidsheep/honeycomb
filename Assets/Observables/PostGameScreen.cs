using UnityEngine;
using UnityEngine.Video;
using System;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;


public class PostGameScreen : KQObserver
{
	public GlobalFade fader;
	public BoxScore boxScore;
	public PostgamePlayerCard[] playerCards;
	public GameObject summaryPage;
	public TextMeshPro title, subText, seriesText;
	public Color[] bgColors;
	public VideoPlayer bg;
	public SpriteRenderer minimap;
	public GameObject combinedPostgame;
	public SpriteRenderer replayBG;
	public TextMeshPro replayText;
	public RawImage replayImage;
	public VideoClipper clipper;
	public float playerCardsDuration = 8f;
	bool doInstantReplay = false;
	bool isVisible = true;
	bool blueWins = false;
	Sequence transitionSeq;
	Sequence delayDisplaySeq;
	Sequence replaySeq;
	bool replayInProgress = false;
	VideoClipper.Clip replayClip;
	GameObject postgameSecondaryScreen;

	RenderTexture replayRender;

	public static bool skipNextPostgame = false;
	static PostGameScreen instance;

	public float _state = 0f;
	public float state
    {
		get { return _state; }
		set
        {
			_state = value;
			combinedPostgame.transform.localPosition = new Vector3(-20f * _state, 0f, 0f);
        }
    }
	int _actualState = 0;
	int actualState
    {
		get { return _actualState; }
		set
        {
			_actualState = value;
		}
    }

	string CommandPostgame(string[] parameters)
    {
		string error = "use with [start] or [stop] to control postgame screen";
		if (parameters.Length == 0 || parameters[0] == "start")
			OnGameEnd(0, "Military");
		else if (parameters[0] == "stop")
			OnGameStart();
		else
			return error;
		return "";
    }

    private void Awake()
    {
		instance = this;
    }
    // Use this for initialization
    override public void Start()
	{
		base.Start();
		GameModel.onGameStart.AddListener(OnGameStart);
		GameModel.onGameEvent.AddListener(OnGameEvent);
		GameModel.onGameModelComplete.AddListener(OnGameEnd);
		SetupScreen.onSetupComplete.AddListener(OnSetupComplete);
		ViewModel.onThemeChange.AddListener(OnThemeChange);
		if (ViewModel.instance.appView)
		{
			minimap.enabled = false;
		}
		
		//gameObject.SetActive(false);
		replayRender = new RenderTexture(1920, 1080, 24);
		LSConsole.AddCommandHook("postgame", "use with [start] or [stop] to control postgame screen", CommandPostgame);
	}

	void OnSetupComplete()
    {
		doInstantReplay = PlayerPrefs.GetInt("instantReplay") == 1;
		SetVisible(false, true);
	}

	void OnThemeChange()
    {
		var pos = new Vector3(-1.65f, .84f, 0f);
		float scale = 1f;
		switch(ViewModel.currentTheme.layout)
        {
			case ThemeData.LayoutStyle.OneCol_Right: break;
			case ThemeData.LayoutStyle.OneCol_Left:
				pos.x *= -1f;
				break;
			case ThemeData.LayoutStyle.TwoCol:
				//hack for extra space with camp frame. need to include parameter to let theme adjust this
				float campBump = (ViewModel.currentTheme.themeName == "campkq" || ViewModel.currentTheme.themeName == "postcamp") ? .18f : 0f;
				pos.x = 0f;
				pos.y = 0.23f + campBump;
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
		SetupSecondaryScreen();

		if(ViewModel.currentTheme.postgameHeaderFont != "")
        {
			title.font = FontDB.GetFont(ViewModel.currentTheme.postgameHeaderFont);
        }
    }

	void SetupSecondaryScreen()
    {
		if(postgameSecondaryScreen != null)
        {
			Destroy(postgameSecondaryScreen.gameObject);
			postgameSecondaryScreen = null;
        }
		if(ViewModel.currentTheme.postGameSecondaryScreen != null)
        {
			postgameSecondaryScreen = Instantiate(ViewModel.currentTheme.postGameSecondaryScreen, combinedPostgame.transform);
			postgameSecondaryScreen.transform.localPosition = new Vector3(20f, 0f, 0f);
        }
		fader.SetFadeSubjects();
    }

	public static void ForceClosePostgame()
    {
		instance.OnGameStart();
    }
	void OnGameStart()
    {
		if (transitionSeq != null && !transitionSeq.IsComplete())
			transitionSeq.Kill();
		if (delayDisplaySeq != null && !delayDisplaySeq.IsComplete())
			delayDisplaySeq.Kill();
		if (replaySeq != null && !replaySeq.IsComplete())
			replaySeq.Kill();
		if (replayInProgress)
			FinishReplay();
		if (actualState == 0) //force stats to show for a split second
			ChangeState(1, true);

		SetVisible(false);
    }

	void OnGameEvent(string type, GameEventData data)
    {
		if (type == GameEventType.GAME_END_DETAIL)
			blueWins = data.teamID == 0;
    }

	string GetSeriesText()
    {
		if (GameModel.instance.setPoints.property <= 0)
			return "";
		int blueScore = GameModel.instance.teams[0].setWins.property;
		int goldScore = GameModel.instance.teams[1].setWins.property;
		string seriesStatus = GameModel.newSetTimeout > 0f ? " wins series " : " leads series ";
		if (blueScore == goldScore)
			return "Series tied at " + blueScore + "-" + goldScore;
		else if (blueScore > goldScore)
			return GameModel.instance.teams[0].teamName.property + seriesStatus + blueScore + "-" + goldScore;
		else
			return GameModel.instance.teams[1].teamName.property + seriesStatus + goldScore + "-" + blueScore;
	}
	void OnGameEnd(int winningTeam, string winType)
	{
		if (GameModel.instance.isWarmup.property || skipNextPostgame)
		{
			skipNextPostgame = false;
			return; //ignore warmup game
		}
		//gameObject.SetActive(true);
		if(doInstantReplay)
			delayDisplaySeq = DOTween.Sequence().AppendInterval(2.5f).AppendCallback(() => StartInstantReplay(winningTeam, winType));
		else
			delayDisplaySeq = DOTween.Sequence().AppendInterval(3f).AppendCallback(() => SetVisible(true));
		title.text = (blueWins ? "Blue" : "Gold") + " Victory";
		subText.text = MapDB.currentMap.property.display_name + " | " + Util.FormatTime(GameModel.instance.gameTime.property);
		boxScore.OnPostgame(winningTeam, winType);
		minimap.sprite = MapDB.currentMap.property.thumbnail;
		var bestPlayers = GameModel.instance.GetBestPlayers(playerCards.Length);
		for(int i = 0; i < bestPlayers.Length; i++)
        {
			playerCards[i].OnPostgame(bestPlayers[i].teamID, bestPlayers[i].positionID);
        }
		state = 0f;
		actualState = 0;
    }

	void StartInstantReplay(int winningTeam, string winType)
    {
		var videos = winningTeam == 1 ? VideoDB.goldVideos : VideoDB.blueVideos;
		var video = winType == "military" ? videos.replayClip_military : winType == "economic" ? videos.replayClip_economic : videos.replayClip_snail;
		bg.clip = video != null ? video : videos.replayClip;
		bg.time = 0f;
		bg.Play();
		bool doFade = video != null;
		if(doFade)
        {
			bg.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
			bg.GetComponent<MeshRenderer>().material.DOColor(new Color(1f, 1f, 1f, 1f), 1.0f);
        }
		replaySeq = DOTween.Sequence().AppendInterval(.15f);
		if (!doFade)
		{
			replaySeq
			.AppendCallback(() => bg.GetComponent<MeshRenderer>().material.color = Color.white);
		}
		replaySeq.AppendInterval(1f)
			.AppendCallback(() => SetupReplay())
			.AppendInterval(1.85f)
			.AppendCallback(() => StartReplay());
		if (doFade)
		{
			replaySeq//.AppendInterval(.25f)
			.Append(bg.GetComponent<MeshRenderer>().material.DOColor(new Color(1f, 1f, 1f, 0f), .4f));
		} else { 
			replaySeq.AppendInterval(.4f)
			.AppendCallback(() => bg.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 0f));
		}
			
    }

	void SetupReplay()
    {
		replayClip = clipper.GetClip(0);
		replayImage.texture = replayClip.texture;
		replayImage.material.mainTexture = replayClip.texture;
		
    }

	void StartReplay()
    {
		replayInProgress = true;
		replayImage.gameObject.SetActive(true);
		replayClip.AdvancePlayback();
		replayImage.color = Color.white;
		replayImage.texture = replayRender;
    }
	void FinishReplay()
    {
		replayInProgress = false;
		replayImage.color = new Color(1f, 1f, 1f, 0f);
		replayImage.gameObject.SetActive(false);
	}
	void SetVisible(bool newVal, bool instant = false)
    {
		/*
		if (newVal == isVisible)
			//if (!newVal) gameObject.SetActive(false);
			return;
		}
		*/
		isVisible = newVal;
		fader.DOKill();
		if (!ViewModel.instance.appView)
		{
			//stream video
			bg.source = VideoSource.Url;
			bg.url = "https://kq.style/etc/" + ViewModel.currentTheme.GetTeamTheme(blueWins ? 0 : 1).postgameVideoURL;
		}
		else
		{
			bg.source = VideoSource.VideoClip;

			bg.clip = blueWins ? VideoDB.blueVideos.postgame : VideoDB.goldVideos.postgame;
		}
		if (ViewModel.instance.appView && newVal)
			ViewModel.StartPIP(false);
		else if(ViewModel.instance.appView && !newVal)
        {
			ViewModel.EndPIP(false);
        }
		if (instant)
		{
			fader.alpha = newVal ? 1f : 0f;
			SetChildrenActive(newVal);
			//gameObject.SetActive(newVal);
		}
		else
		{
			if (newVal == true) SetChildrenActive(true);
			DOTween.To(() => fader.alpha, x => fader.alpha = x, newVal ? 1f : 0f, .5f)
				.OnComplete(() => { if (!newVal) SetChildrenActive(newVal); }); // gameObject.SetActive(newVal));
		}
		if (newVal)
		{
			ChangeState(1, false, playerCardsDuration);
			//set text delay to see if we're starting a new set
			seriesText.text = GetSeriesText();
		}
	}


	void SetChildrenActive(bool isActive)
    {
		if(postgameSecondaryScreen != null)
			postgameSecondaryScreen.SetActive(isActive);
		combinedPostgame.SetActive(isActive);
		//boxScore.gameObject.SetActive(isActive);
		//foreach (var go in gameObject.GetComponentsInChildren<GameObject>())
			//go.SetActive(isActive);
    }
	void ChangeState(int newState, bool instant = false, float delay = 0f)
    {
		if (postgameSecondaryScreen == null)
			newState = 0; //always stay on main screen if there's no secondary screen

		if (transitionSeq != null && transitionSeq.IsPlaying())
			transitionSeq.Kill();
		if (instant)
			state = actualState = newState;
		else
		{
			transitionSeq = DOTween.Sequence();
			if (delay > 0f)
				transitionSeq.AppendInterval(delay);
			transitionSeq.AppendCallback(() => actualState = newState).Append(DOTween.To(() => state, x => state = x, (float)newState, .5f));
		}
	}

    private void FixedUpdate()
    {
		if (replayInProgress)
		{
			bool done = replayClip.AdvancePlayback();
			if (!done)
			{
				if(VideoClipper.canDownResImage)
					Graphics.Blit(replayClip.texture, replayRender);
				else
					Graphics.CopyTexture(replayClip.texture, replayRender);
			}
			else
			{
				FinishReplay();
				SetVisible(true);
			}
		}
	}
    // Update is called once per frame
    void Update()
	{
#if UNITY_EDITOR
		if(Input.GetMouseButtonDown(0) && isVisible)
        {
			ChangeState(1 - actualState, false);
        }
#endif
	}
}

