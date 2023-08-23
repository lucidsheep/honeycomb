using UnityEngine;
using System.Collections;

public class EdgeMask : MonoBehaviour
{
	public int sideID = 0;
	public float yPos = 0f;
	// Use this for initialization
	void Start()
	{
		yPos = transform.position.y;
	}

	// Update is called once per frame
	void Update()
	{
		transform.position = new Vector3(ViewModel.screenWidth * (sideID == 0 ? -.5f : .5f), yPos, 0f);
	}
}

