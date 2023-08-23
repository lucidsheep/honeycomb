using UnityEngine;
using System.Collections;

public class ProgressBar : MonoBehaviour
{
	public SpriteRenderer fill;
	// Use this for initialization
	public void SetFill(float pct)
    {
		fill.material.SetFloat("_Fill", pct);
    }
}

