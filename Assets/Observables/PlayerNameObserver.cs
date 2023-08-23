using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerNameObserver : KQObserver
{ 
	public TextMeshPro text;
	public GameObject textContainer;
	public SpriteRenderer[] icons;
	public float pronounSize = 2f;

	TournamentTeamData presetTeamData;
	bool dirty = false;
	bool presetMode = false;
	bool forceIcons = false;
	// Use this for initialization
	public override void Start()
	{
		base.Start();
		for(int p = 0; p < 5; p++)
		{
			GameModel.instance.teams[targetID].players[p].playerName.onChange.AddListener((b, a) => dirty = true);
		}
		if (targetID == 0) TournamentPresetData.blueTeam.onChange.AddListener(OnPresetTeamData);
		else TournamentPresetData.goldTeam.onChange.AddListener(OnPresetTeamData);
		PlayerStaticData.onPlayerData.AddListener(_ => dirty = true);
		dirty = true;
	}

	void OnPresetTeamData(TournamentTeamData b, TournamentTeamData a)
    {
		presetTeamData = a;
		dirty = true;
    }
	// Update is called once per frame
	void Update()
	{
		if(dirty)
        {
			dirty = false;
			string txt = "";
			SetPresetTeamDisplayMode(presetTeamData != null);
			int lineNum = 0;
			for (int j = 0; j < 5; j++)
			{
				//force queen to display first if present
				int i = j;
				if (j == 0) i = 2;
				else if (j <= 2) i = j - 1;
				
				if (GameModel.instance.teams[targetID].players[i].playerName.property == "" && !forceIcons) continue;
				var id = GameModel.instance.teams[targetID].players[i].hivemindID;
				txt += FormatName(GameModel.instance.teams[targetID].players[i].playerName.property, i) + " " + FormatPronouns(PlayerStaticData.GetPronouns(id)) + "\n";
				if(!presetMode)
					icons[lineNum].sprite = SpriteDB.allSprites[targetID].playerSprites[i].icon;
				lineNum++;
			}

			text.text = txt;
        }
	}

	void SetPresetTeamDisplayMode(bool preset)
    {
		presetMode = preset;
		textContainer.transform.localPosition = new Vector3(preset ? 0.05f : 0.33f, -1.3f, 0f);
		foreach (var icon in icons)
			icon.sprite = null;
    }
	string FormatPronouns(string input)
    {
		if (input == "") return "";
		return "<size=" + pronounSize + "><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>" + input.ToUpper() + "</b></mark></size>";
    }
	string FormatName(string input, int id)
    {
		if (id != 2) return input;
		return "<b>" + input + "</b>";
    }

    public override void OnParameters()
    {
        base.OnParameters();
		forceIcons = moduleParameters.ContainsKey("forceIcons");
		if (moduleParameters.ContainsKey("pronounSize"))
			pronounSize = float.Parse(moduleParameters["pronounSize"]);
	}
}

