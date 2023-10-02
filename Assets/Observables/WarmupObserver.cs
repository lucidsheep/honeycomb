using UnityEngine;
using System.Collections.Generic;

public class WarmupObserver : KQObserver
{
	public GameObject[] objs;
	// Use this for initialization
	override public void Start()
	{
		base.Start();
		GameModel.instance.isWarmup.onChange.AddListener(OnWarmupChange);
		OnWarmupChange(false, false);
	}

	void OnWarmupChange(bool old, bool newVal)
    {
		foreach (var obj in objs)
			obj.SetActive(newVal);
    }
}

