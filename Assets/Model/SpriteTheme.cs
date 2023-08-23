using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "SpriteTheme")]
public class SpriteTheme : ScriptableObject
{
	public Color[] color;
	public Material[] material;
}

