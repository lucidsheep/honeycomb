using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class UIState : MonoBehaviour
{
	public static UIState instance;
	public static UnityEvent<bool> onInvert = new UnityEvent<bool>();

	public static bool inverted { get { return instance._inverted; } set { if (instance._inverted == value) return; instance._inverted = value; onInvert.Invoke(value); } }
	public static float invertf { get { return instance._inverted ? -1f : 1f; } }
	public static int inverti {  get { return instance._inverted ? -1 : 1; } }
	public static int blue { get { return instance._inverted ? 1 : 0; } }
	public static int gold { get { return instance._inverted ? 0 : 1; } }
	bool _inverted = false;

	// Use this for initialization
	void Awake()
	{
		instance = this;
	}

}

