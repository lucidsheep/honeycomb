using UnityEngine;
using System.Collections;
using TMPro;

public class FeedCard : MonoBehaviour
{
	public SpriteRenderer player;
	public SpriteRenderer target;
	public SpriteRenderer action;
	public SpriteRenderer[] bgs;
	public SpriteRenderer[] shadows;
	public TextMeshPro text;
	public GlobalFade alpha;
	public int pos = 0;
	public float cardWidth = 14f;

	int team = 0;

	public void SetTeam(int teamID)
    {
		team = teamID;
		GetComponent<Themeable>().targetID = team;
    }

	public void SetPos(int newPos, bool faceRight)
    {
		pos = newPos;
		
		foreach (var bg in bgs)
			bg.sortingOrder = newPos * 2 * (faceRight ? -1 : 1);
		foreach (var shadow in shadows)
			shadow.sortingOrder = newPos * 2 * (faceRight ? -1 : 1) - 1;
		if (player != null) player.sortingOrder = newPos * 2 * (faceRight ? -1 : 1) + 1;
		if (target != null) target.sortingOrder = newPos * 2 * (faceRight ? -1 : 1) + 1;
		if (action != null) action.sortingOrder = newPos * 2 * (faceRight ? -1 : 1) + 1;
		if (text != null) text.sortingOrder = newPos * 2 * (faceRight ? -1 : 1) + 1;
		
	}

}

