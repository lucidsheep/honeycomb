using UnityEngine;
using System.Collections;
using TMPro;

public class MatchPreviewPlayer : MonoBehaviour
{
	public ProfilePicture avatar;
	public TextMeshPro nameTxt;
	public Sprite defaultAvatar;
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public void Init(PlayerStaticData.PlayerData player)
    {
		if (player.profilePic != null)
			avatar.SetPicture(player.profilePic, player.profilePicRotation, .35f);
		else
			avatar.SetPicture(defaultAvatar);
		nameTxt.text = player.name + " " + FormatTags(player.pronouns, player.sceneColor);
    }
	static string FormatTags(string pronouns, string scene, string sceneColor = "00aa00", float size = 30f)
	{
		string ret = "";
		if (pronouns != "")
			ret += "<size=" + size + "><mark=#E540D277 padding=\"10, 10, 0, 0\"><b>" + pronouns.ToUpper() + "</b></mark></size>";
		if (scene == "")
			return ret;
		if (ret != "")
			ret += " ";
		return ret + "<size=" + size + "><mark=#" + sceneColor + "77 padding=\"10, 10, 0, 0\"><b>" + scene.ToUpper() + "</b></mark></size>";
	}
}

