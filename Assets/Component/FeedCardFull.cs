using UnityEngine;
using System.Collections;
using TMPro;

public class FeedCardFull : FeedCard
{
	public SpriteRenderer[] icons;
	public float[] iconScales;
	public TextMeshPro[] statLabels;
	public Sprite[] iconTypes;
	public SpriteRenderer[] crowns;

	int nextStat = 0;
	// Use this for initialization
	void Awake()
	{
		for(int i = 0; i < icons.Length; i++)
        {
			icons[i].sprite = null;
			statLabels[i].text = "";
        }
		SetCrowns(0);
	}

	// Update is called once per frame
	void Update()
	{
			
	}

	public void SetStat(PlayerModel.StatValueType type, string val)
    {
		if (nextStat >= icons.Length || (int)type >= iconTypes.Length) return;

		icons[nextStat].sprite = iconTypes[(int)type];
		icons[nextStat].transform.localScale = Vector3.one * iconScales[(int)type];
		statLabels[nextStat].text = val;
		nextStat++;
    }

	public void SetCrowns(int qKills)
    {
		for(int i = 0; i < 3; i++)
        {
			crowns[i].enabled = i < qKills;
        }
    }
}

