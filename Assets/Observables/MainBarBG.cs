using UnityEngine;
using System.Collections;

public class MainBarBG : MonoBehaviour
{
	public SpriteRenderer bg;
	Vector3 baseVec;
	// Use this for initialization
	void Start()
	{
		UIState.onInvert.AddListener(OnInvert);
		baseVec = transform.localScale;
	}

	void OnInvert(bool inverted)
	{
		transform.localScale = new Vector3(baseVec.x * (inverted ? -1f : 1f), baseVec.y, baseVec.z);
	}
}

