using UnityEngine;
using System.Collections;
using TMPro;

public class TeamNameObserver : KQObserver
{
	TextMeshPro txt;
	string teamName;
	bool dirty = false;
	bool useBold = false;
	bool forceLowercase = true;
    // Use this for initialization

    private void Awake()
    {
		txt = GetComponent<TextMeshPro>();
	}
    override public void Start()
	{
		base.Start();
		for (int i = 0; i < 2; i++)
		{
			var copiedIndex = i;
			GameModel.instance.teams[copiedIndex].teamName.onChange.AddListener((b, a) => { if (copiedIndex == team) OnChange(b, a); });
		}
	}

    public void OnChange(string before, string after)
    {
		teamName = after;
		dirty = true;
    }

    private void Update()
    {
        if(dirty)
        {
			var s = (useBold ? "<b>" : "") + GameModel.instance.teams[team].teamName.property + (useBold ? "</b>" : "");
			if (forceLowercase)
				s = s.ToLower();
			txt.text = s;
			dirty = false;
		}
    }

    protected override void OnThemeChange()
    {
        base.OnThemeChange();
		if(ViewModel.currentTheme.postgameHeaderFont != "")
        {
			txt.font = FontDB.GetFont(ViewModel.currentTheme.postgameHeaderFont);
			useBold = true;
			forceLowercase = false;
			dirty = true;
        }
    }

}

