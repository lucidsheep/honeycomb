using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlusMinusButton : MonoBehaviour
{
	public int delta = 1;

	public UnityEvent<int> onPressed = new UnityEvent<int>();

    private void OnMouseDown()
    {
		onPressed.Invoke(delta);
    }
    // Use this for initialization
    void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
			
	}
}

