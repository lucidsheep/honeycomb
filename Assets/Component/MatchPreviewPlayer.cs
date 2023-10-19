using UnityEngine;
using System.Collections;
using TMPro;

public class MatchPreviewPlayer : MonoBehaviour
{
	public ProfilePicture avatar;
	public TextMeshPro nameTxt, sceneTxt, pronounTxt;
	public Sprite defaultAvatar;

	public enum TagStyle { COLORIZE, UNBOLD, BOLD, SIZE, NONE}
	public TagStyle tagStyle = TagStyle.COLORIZE;
	public bool tagsFirst = false;
	public float tagSize = 30f;
	public bool sizeIsRatio = false;
	// Use this for initialization
	void Start()
	{
		if (GetComponentInParent<GlobalFade>() != null && sceneTxt != null)
			GetComponentInParent<GlobalFade>().SetBaseAlpha(sceneTxt, 0.4627451f);
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public void Init(PlayerStaticData.PlayerData player)
    {
		bool usedPronouns = false, usedScene = false;
		if (player.profilePic != null)
			avatar.SetPicture(player.profilePic, player.profilePicRotation, .55f);
		else
			avatar.SetPicture(defaultAvatar);
		
		if(sceneTxt != null)
        {
			sceneTxt.text = player.sceneTag.ToUpper();
			usedScene = true;
        }
		if(pronounTxt != null)
        {
			pronounTxt.text = player.pronouns.ToUpper();
			usedPronouns = true;
        }
		nameTxt.text = player.name;
		nameTxt.ForceMeshUpdate(); //needed to calculate pronoun size
		var tags = FormatTags(usedPronouns ? "" : player.pronouns, usedScene ? "" : player.sceneTag, "00aa00", (sizeIsRatio ? nameTxt.fontSize * tagSize : tagSize));
		if (tags == "")
			nameTxt.text = player.name;
		else
			nameTxt.text = (tagsFirst ? tags + " " : "") + player.name + (!tagsFirst ? " " + tags : "");
	}

	public void Clear()
    {
		avatar.SetPicture(defaultAvatar);
		nameTxt.text = "";
		if (sceneTxt != null)
			sceneTxt.text = "";
		if (pronounTxt != null)
			pronounTxt.text = "";
    }
	string FormatTags(string pronouns, string scene, string sceneColor = "00aa00", float size = 30f)
	{
		string ret = "";
		if (pronouns != "")
			ret += Format(pronouns, size, "E540D2");
		if (scene == "")
			return " " + ret;
		if (ret != "")
			ret += " ";
		return " " + ret + Format(scene, size, sceneColor);
	}

	string Format(string input, float size, string color)
    {
		switch(tagStyle)
        {
			case TagStyle.COLORIZE:
				return "<size=" + size + "><mark=#" + color + "77 padding=\"10, 10, 0, 0\"><b>" + input.ToUpper() + "</b></mark></size>";
			case TagStyle.UNBOLD:
				return "<size=" + size + "></b>" + input.ToUpper() + "</size><b>";
			case TagStyle.BOLD:
				return "<size=" + size + "><b>" + input.ToUpper() + "</size></b>";
			case TagStyle.SIZE:
				return "<size=" + size + ">" + input.ToUpper() + "</size>";
			default: return input;
		}
    }
}

