using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;

public class DonationBox : MonoBehaviour
{
	public TextMeshPro donationText, timeText;
	public PlusMinusButton plusButton, minusButton;

	public UnityEvent<DonationBox> onDeath = new UnityEvent<DonationBox>();
	int numGames;

	public void Init(string txt, int games = 3)
    {
		donationText.text = txt;
		numGames = games;
		UpdateTime();
		GameModel.onGameModelComplete.AddListener(OnGameComplete);
    }

	void OnDelta(int delta)
    {
		numGames += delta;

		if(numGames <= 0)
        {
			onDeath.Invoke(this);
			Destroy(this.gameObject);
        } else
        {
			UpdateTime();
        }
    }

	void OnGameComplete(int _, string __)
    {
		OnDelta(-1);
    }
	void UpdateTime()
    {
		timeText.text = numGames + " game" + (numGames == 1 ? "" : "s");
    }
	// Use this for initialization
	void Start()
	{
		plusButton.onPressed.AddListener(OnDelta);
		minusButton.onPressed.AddListener(OnDelta);
	}

	// Update is called once per frame
	void Update()
	{
			
	}

}

