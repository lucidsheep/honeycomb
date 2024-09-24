using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class KQObserver : MonoBehaviour
{
	public int targetID;
	public string moduleName;
	public float size;
	public SpriteRenderer bgContainer;
	public SpriteRenderer normalBG;
	public Vector2 offset;
	public bool absolutePos = false;
	public Vector2 bgCustomPivot = new Vector2(.5f, .5f);
	public Dictionary<string,string> moduleParameters = new Dictionary<string, string>();
	public int team { get { return targetID == 0 ? UIState.blue : UIState.gold; } }
	virtual public void Start()
    {
		UIState.onInvert.AddListener(OnInvert);
		ViewModel.onThemeChange.AddListener(OnThemeChange);
		OnThemeChange();
    }

	virtual public void OnInvert(bool inverted)
    {
		
    }

	virtual protected void OnThemeChange()
    {
		bool useDefaultBG = false;
		if (!ViewModel.instance.appView || bgContainer == null)
			useDefaultBG = true;
		else
		{
			var sprite = AppLoader.GetStreamingSprite(moduleName, bgCustomPivot);
			if (sprite != null)
			{
				bgContainer.sprite = sprite;
			}
			else
			{
				useDefaultBG = true;
			}
		}
		if(bgContainer != null)
			bgContainer.gameObject.SetActive(!useDefaultBG);
		if (normalBG != null)
			normalBG.gameObject.SetActive(useDefaultBG);
    }
	public void SetParameters(string[] args)
    {
		//i=0 is always module name, not a parameter
		for(int i = 1; i < args.Length; i++)
        {
			var argParsed = args[i].Split('=');
			if (!moduleParameters.ContainsKey(argParsed[0]))
				moduleParameters.Add(argParsed[0], (argParsed.Length == 1 ? "" : argParsed[1]));
			else
				moduleParameters[argParsed[0]] = (argParsed.Length == 1 ? "" : argParsed[1]);
		}
		OnParameters();
    }

	virtual public void OnParameters()
    {
		if(moduleParameters.ContainsKey("spacing"))
        {
			//Debug.Log("spacing = " + moduleParameters["spacing"]);
			size = float.Parse(moduleParameters["spacing"]);
        }
		if(moduleParameters.ContainsKey("position"))
        {
			offset.y = float.Parse(moduleParameters["position"]);
			absolutePos = true;
		}
		if(moduleParameters.ContainsKey("scale"))
        {
			var scale = float.Parse(moduleParameters["scale"]);
			transform.localScale = new Vector3(scale, scale, 1f);
        }
		if(moduleParameters.ContainsKey("font"))
        {
			var font = FontDB.GetFont(moduleParameters["font"]);
			if (font != null)
			{
				foreach (var t in GetComponentsInChildren<TextMeshPro>())
					t.font = font;
			}
        }
		if(moduleParameters.ContainsKey("offsetX"))
        {
			offset.x = float.Parse(moduleParameters["offsetX"]);
        }
		if (moduleParameters.ContainsKey("offsetY"))
		{
			offset.y = float.Parse(moduleParameters["offsetY"]);
		}
		if(bgContainer != null)
		{
			if (moduleParameters.ContainsKey("hideBackground"))
			{
				bgContainer.gameObject.SetActive(false);
				if (normalBG != null)
					normalBG.gameObject.SetActive(false);
			}
			Vector2 bgPos = Vector2.zero;
			if(moduleParameters.ContainsKey("backgroundX"))
				bgPos.x = float.Parse(moduleParameters["backgroundX"]);
			if(moduleParameters.ContainsKey("backgroundY"))
				bgPos.y = float.Parse(moduleParameters["backgroundY"]);
			bgContainer.transform.localPosition = bgPos;
		}
	}

}

