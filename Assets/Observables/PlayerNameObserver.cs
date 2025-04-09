using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerNameObserver : KQObserver
{ 
	public TextMeshPro text, pronounText, sceneText, statText;
	public GameObject textContainer;
	public GameObject[] bgFrame;
	public GameObject mainBG;
	public SpriteRenderer[] icons;
	public float pronounSize = 2f;

	public int maxNameLength = 99;
	public bool boldQueen = true;

	public bool showKD = false;

	public bool showDefaultNames = false;
	public Color defaultNameColor = Color.white;

	bool dirty = false;
	bool forceIcons = false;
	bool hideIcons = false;
	bool queenFirst = true;
	bool hideIfEmpty = false;

	bool useCustomText = false;

	public override void Start()
	{
		base.Start();
		for(int p = 0; p < 5; p++)
		{
			GameModel.instance.teams[targetID].players[p].playerName.onChange.AddListener((b, a) => dirty = true);
			if(showKD){
				GameModel.instance.teams[targetID].players[p].curGameStats.kills.onChange.AddListener((_, __) => dirty = true);
				GameModel.instance.teams[targetID].players[p].curGameStats.deaths.onChange.AddListener((_, __) => dirty = true);
			}
		}
		PlayerStaticData.onPlayerData.AddListener(_ => dirty = true);
		//textContainer.transform.localPosition = new Vector3(0.33f, -1.3f, 0f);
		dirty = true;
	}

	void Update()
	{
		if(dirty)
        {
			dirty = false;
			string txt = "";
			string pronounTxt = "";
			string sceneTxt = "";
			string kdText = "";
			int lineNum = 0;
			for (int j = 0; j < 5; j++)
			{
				//force queen to display first if present
				int i = j;
				if(queenFirst)
				{
					if (j == 0) i = 2;
					else if (j <= 2) i = j - 1;
				}
				icons[lineNum].sprite = null;
				var isEmptyName = GameModel.instance.teams[targetID].players[i].playerName.property == "";
				
				if (isEmptyName && !forceIcons && !hideIcons && !showDefaultNames) continue;

				var id = GameModel.instance.teams[targetID].players[i].hivemindID;
				if(isEmptyName && showDefaultNames)
					txt += FormatName(GameModel.instance.teams[targetID].players[i].displayNameWithoutTeam, i, true);
				else
					txt += FormatName(GameModel.instance.teams[targetID].players[i].playerName.property, i);
				if(pronounText != null) 
				{
					pronounTxt += FormatPronouns(PlayerStaticData.GetPronouns(id)) + "\n";
					txt += "\n";
				} else
				{
					txt += " " + FormatPronouns(PlayerStaticData.GetPronouns(id)) + "\n";
				}
				if(sceneText != null)
				{
					sceneTxt += FormatScene(PlayerStaticData.GetSceneTag(id)) + "\n";
				}
				if(hideIcons == false)
					icons[lineNum].sprite = SpriteDB.GetIcon(targetID, i);
				else
					icons[lineNum].sprite = null;

				if(showKD)
				{
					kdText += GameModel.GetPlayer(targetID, i).curGameDerivedStats[PlayerModel.StatValueType.KD].fullNumber + "\n";
				}
				lineNum++;
			}

			text.text = txt;
			if(pronounText != null)
				pronounText.text = pronounTxt;

			if(sceneText != null)
				sceneText.text = sceneTxt;
			if(kdText != null)
				statText.text = kdText;
				
			if(mainBG != null && hideIfEmpty)
				mainBG.SetActive(txt != "");
        }
	}

	string FormatPronouns(string input)
    {
		if (input == "" || pronounSize <= 0f) return "";
		return "<size=" + pronounSize + "><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>" + input.ToUpper() + "</b></mark></size>";
    }
	string FormatScene(string input)
    {
		if (input == "") return "";
		return "<size=" + pronounSize + "><mark=#8DDE8677 padding=\"10, 10, 0, 0\"><b>" + input.ToUpper() + "</b></mark></size>";
    }
	string FormatName(string input, int id, bool isDefaultName = false)
    {
		var ret = pString(input);
		ret = Util.SmartTruncate(ret, maxNameLength);
		if(id == 2 && boldQueen)
			ret = "<b>" + ret + "</b>";
		if(isDefaultName)
			ret = "<color=#" + ColorUtility.ToHtmlStringRGB(defaultNameColor) + ">" + ret + "</color>";
		return ret;
    }

    public override void OnParameters()
    {
        base.OnParameters();
		forceIcons = moduleParameters.ContainsKey("forceIcons");
		queenFirst = !moduleParameters.ContainsKey("cabOrder");
		hideIcons = moduleParameters.ContainsKey("hideIcons");
		if (moduleParameters.ContainsKey("pronounSize"))
			pronounSize = float.Parse(moduleParameters["pronounSize"]);
		hideIfEmpty = moduleParameters.ContainsKey("autoHide") && bool.Parse(moduleParameters["autoHide"]);
		dirty = true;
	}

    protected override void OnThemeChange()
    {
        base.OnThemeChange();

				if(bgContainer != null && bgContainer.sprite != null)
		{
			foreach(var bg in bgFrame)
				bg.SetActive(false);
		}
    }
}

