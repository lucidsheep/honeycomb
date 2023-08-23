using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class FontDB : MonoBehaviour
{
	static FontDB instance;

	[System.Serializable]
	public struct FontData
    {
		public TMP_FontAsset font;
		public string fontName;
    }

	public FontData[] allFonts;

	public static TMP_FontAsset GetFont(string fontName)
    {
		foreach(var f in instance.allFonts)
        {
			if (f.fontName == fontName) return f.font;
        }
		return null;
    }
    private void Awake()
    {
		instance = this;
    }

}

