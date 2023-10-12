using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GlobalFade : MonoBehaviour
{
	public float debugFade = 100f;
	float oldDebugFade = 100f;
	SpriteRenderer[] srs;
	TextMeshPro[] tmps;
	MeshRenderer[] players;

	Dictionary<SpriteRenderer, float> baseAlphas = new Dictionary<SpriteRenderer, float>();
	float _alpha = 1f;
	public float alpha {get { return _alpha; } set {
			_alpha = value;
			SetAlpha();
		} }
	void Awake()
	{
		SetFadeSubjects();
	}

    private void Update()
    {
        if(oldDebugFade != debugFade)
        {
			oldDebugFade = debugFade;
			_alpha = debugFade;
			SetAlpha();
        }
    }
    void SetAlpha()
    {
		foreach (var s in srs)
		{
			float adjustedAlpha = baseAlphas.ContainsKey(s) ? baseAlphas[s] * _alpha : _alpha;
			s.color = new Color(s.color.r, s.color.g, s.color.b, adjustedAlpha);
		}
		foreach (var t in tmps)
			t.color = new Color(t.color.r, t.color.g, t.color.b, _alpha);
		foreach (var v in players)
			v.material.SetColor("_Color", new Color(1f, 1f, 1f, _alpha));
	}
	public void SetFadeSubjects()
    {
		Debug.Log("set fade subjects");
		srs = GetComponentsInChildren<SpriteRenderer>();
		tmps = GetComponentsInChildren<TextMeshPro>();
		players = GetComponentsInChildren<MeshRenderer>();
	}

	public void ForceUpdate()
    {
		SetAlpha();
    }
	public void SetBaseAlpha(SpriteRenderer sr, float baseAlpha)
    {
		baseAlphas.Add(sr, baseAlpha);
    }
}

