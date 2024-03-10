using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerNameObserver : KQObserver
{ 
	public TextMeshPro text;
	public GameObject textContainer;
	public GameObject mainBG;
	public SpriteRenderer[] icons;
	public float pronounSize = 2f;

	bool dirty = false;
	bool forceIcons = false;
	bool hideIfEmpty = false;

	public override void Start()
	{
		base.Start();
		for(int p = 0; p < 5; p++)
		{
			GameModel.instance.teams[targetID].players[p].playerName.onChange.AddListener((b, a) => dirty = true);
		}
		PlayerStaticData.onPlayerData.AddListener(_ => dirty = true);
		dirty = true;
	}

	void Update()
	{
		if(dirty)
        {
			dirty = false;
			string txt = "";
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
				icons[lineNum].sprite = SpriteDB.GetIcon(targetID, i);
				lineNum++;
			}

			text.text = txt;

			if(mainBG != null)
				mainBG.SetActive(txt != "");
        }
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
		hideIfEmpty = moduleParameters.ContainsKey("autoHide") && bool.Parse(moduleParameters["autoHide"]);
	}
}

