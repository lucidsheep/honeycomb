using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PlayerCVTracker : MonoBehaviour
{
	public TextMeshPro nameText;
	public SpriteRenderer hat;
	public GameObject blueFireTemplate, goldFireTemplate;
	public GameObject blueFire, goldFire;

	ParticleSystem[] blueEmitters;
	ParticleSystem[] goldEmitters;

	int fireState = 0; //-1 = no fire, 0 = blue fire, 1 = gold fire

	// Use this for initialization
	void Start()
	{
		nameText.text = "";
		hat.sprite = null;

		blueFire = Instantiate(blueFireTemplate, transform.parent);
		goldFire = Instantiate(goldFireTemplate, transform.parent);

		blueEmitters = blueFire.GetComponentsInChildren<ParticleSystem>();
		goldEmitters = goldFire.GetComponentsInChildren<ParticleSystem>();

		SetFire(-1);
	}

	// Update is called once per frame
	void Update()
	{
		var myPos = transform.localPosition;
		myPos.y -= .83f;
		blueFire.transform.localPosition = goldFire.transform.localPosition = myPos;
	}

	public void SetFire(int newState)
    {
		if (fireState == newState) return;

		fireState = newState;
		foreach (var em in blueEmitters)
			em.enableEmission = fireState == 0;
		foreach (var em in goldEmitters)
			em.enableEmission = fireState == 1;
	}
}

