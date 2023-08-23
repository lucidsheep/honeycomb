using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using TMPro;

public class GlobalFade : MonoBehaviour
{
	SpriteRenderer[] srs;
	TextMeshPro[] tmps;
	MeshRenderer[] players;

	float _alpha = 1f;
	public float alpha {get { return _alpha; } set {
			_alpha = value;
			foreach (var s in srs)
				s.color = new Color(s.color.r, s.color.g, s.color.b, _alpha);
			foreach (var t in tmps)
				t.color = new Color(t.color.r, t.color.g, t.color.b, _alpha);
			foreach (var v in players)
				v.material.SetColor("_Color", new Color(1f, 1f, 1f, _alpha));
		} }
	void Awake()
	{
		SetFadeSubjects();
	}

	public void SetFadeSubjects()
    {
		srs = GetComponentsInChildren<SpriteRenderer>();
		tmps = GetComponentsInChildren<TextMeshPro>();
		players = GetComponentsInChildren<MeshRenderer>();

		//alpha = (_alpha + 1f - 1f);
	}
}

