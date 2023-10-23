using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class WCCPlayerNameDisplay : MonoBehaviour
{
	public int position;
	public Color sceneTagColor;
	public TextMeshPro playerName, sceneTag;
	public SpriteRenderer bg, icon, offBG, offIcon;
	public ProfilePicture profile;
	public Sprite[] offIcons;
	public Sprite[] onIcons;
	public bool pronounsFirst;

	bool curState = true;
	// Use this for initialization
	void Start()
	{
		offIcon.sprite = offIcons[position];
		icon.sprite = onIcons[position];
	}

	// Update is called once per frame
	void Update()
	{
			
	}

	public void SetDisplay(string name, string scene, string pronouns, bool instant = false)
    {
		if(name == "")
        {
			if (curState == false) instant = true;
			curState = false;

			if (instant)
            {
				bg.color = icon.color = playerName.color = new Color(1f, 1f, 1f, 0f);
				sceneTag.color = new Color(sceneTagColor.r, sceneTagColor.g, sceneTagColor.b, 0f);
				
            } else
            {
				playerName.text = name;
				sceneTag.text = scene;
				bg.DOColor(new Color(1f,1f,1f,0f), .5f).SetEase(Ease.OutQuad);
				icon.DOColor(new Color(1f, 1f, 1f, 0f), .5f).SetEase(Ease.OutQuad);
				playerName.DOColor(new Color(1f, 1f, 1f, 0f), .25f).SetEase(Ease.Linear);
				sceneTag.DOColor(new Color(1f, 1f, 1f, 0f), .25f).SetEase(Ease.Linear);
			}
        } else
        {
			if (curState) instant = true;
			curState = true;

			playerName.text = name;
			playerName.ForceMeshUpdate(); //need this to calculate size for pronouns
			playerName.text = (pronounsFirst ? FormatPronouns(pronouns) + " " : "") + name + (!pronounsFirst ? " " + FormatPronouns(pronouns) : "");
			sceneTag.text = scene;

			if (instant)
            {
				bg.color = icon.color = playerName.color = Color.white;
				sceneTag.color = sceneTagColor;
            } else
            {
				bg.DOColor(Color.white, .75f).SetEase(Ease.InElastic);
				icon.DOColor(Color.white, .75f).SetEase(Ease.InElastic);
				playerName.transform.DOLocalMoveX(-.3f, .25f).From().SetDelay(.75f).SetEase(Ease.Linear);
				sceneTag.transform.DOLocalMoveX(-.37f, .25f).From().SetDelay(.75f).SetEase(Ease.Linear);
				playerName.DOColor(Color.white, .25f).SetDelay(.75f).SetEase(Ease.Linear);
				sceneTag.DOColor(sceneTagColor, .25f).SetDelay(.75f).SetEase(Ease.Linear);
			}
        }
    }

	string FormatPronouns(string pronouns)
    {
		var size = playerName.fontSize * .75f;
		return "<size=" + size + ">" + pronouns.ToUpper() + "</size>";
	}
}

