using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DonationFeed : MonoBehaviour
{
	public TMP_InputField newDonationText;
	public DonationBox donationTemplate;
	public float offsetY, offsetX, boxHeight;

	public List<DonationBox> feed = new List<DonationBox>();
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
			if(newDonationText.text != "")
            {
				CreateDonationBox(newDonationText.text);
				newDonationText.text = "";
				
            }
        }
	}

	void CreateDonationBox(string text)
    {
		Debug.Log("making box");
		var box = Instantiate(donationTemplate, transform);
		//box.transform.localPosition = new Vector3(offsetX, offsetY, 0f);
		box.Init(text, 3);
		box.onDeath.AddListener(OnBoxDestroyed);
		feed.Add(box);
		UpdateBoxes();
    }

	void UpdateBoxes()
    {
		for (var i = 0; i < feed.Count; i++)
		{
			feed[i].transform.localPosition = new Vector3(offsetX, offsetY + (boxHeight * (feed.Count - 1 - i)), 0f);
		}
	}
	void OnBoxDestroyed(DonationBox box)
    {
		feed.Remove(box);
		UpdateBoxes();
    }
}

