using UnityEngine;
using System.Collections;
using TMPro;

public class TeamNameObserver : KQObserver
{
	TextMeshPro txt;
	string teamName;
	bool dirty = false;
	public bool useBold = false;
	public bool forceLowercase = true;
	public bool forceUppercase = false;
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
			if (forceUppercase)
				s = s.ToUpper();
			txt.text = s;
			dirty = false;
		}
    }

    protected override void OnThemeChange()
    {
        base.OnThemeChange();
		if(ViewModel.currentTheme.headerFont != null && ViewModel.currentTheme.headerFont != "")
        {
			txt.font = FontDB.GetFont(ViewModel.currentTheme.headerFont);
			if (ViewModel.currentTheme.headerFont != "defaultHeader")
			{
				useBold = true;
				forceLowercase = false;
			}
        }
		dirty = true;
    }

}

