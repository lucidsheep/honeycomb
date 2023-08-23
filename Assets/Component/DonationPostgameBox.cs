using UnityEngine;
using System.Collections;
using TMPro;

public class DonationPostgameBox : MonoBehaviour
{
	public string donationURL;
	public TextMeshPro donationTxt;
	public SpriteRenderer qrCode;
	public SpriteRenderer themeBG;

	// Use this for initialization
	void Start()
	{
		qrCode.sprite = LSQRCode.GenerateQRCode(donationURL, LSQRCode.ColorMode.WhiteOnBlack);
		ViewModel.onThemeChange.AddListener(OnThemeChange);
		OnThemeChange();
	}

	void OnThemeChange()
    {
		themeBG.sprite = AppLoader.GetStreamingSprite("postgameBanner");
    }

	// Update is called once per frame
	void Update()
	{
			
	}
}

