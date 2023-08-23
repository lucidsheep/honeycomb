using UnityEngine;
using System.Collections;

public class ProfilePicture : MonoBehaviour
{
	public SpriteRenderer sr;
	// Use this for initialization
	void Start()
	{

	}

	public void SetPicture(Sprite pic, int rotationCode = 0, float scale = 1f)
    {
		int rotationLevel = 0;
		bool hFlip = false;
		//see https://sirv.com/help/articles/rotate-photos-to-be-upright/
		switch (rotationCode)
		{
			case 0: case 1: break;
			case 2: hFlip = true; break;
			case 3: rotationLevel = 2; break;
			case 4: rotationLevel = 2; hFlip = true; break;
			case 5: rotationLevel = 1; hFlip = true; break;
			case 6: rotationLevel = 1; break;
			case 7: rotationLevel = 3; hFlip = true; break;
			case 8: rotationLevel = 3; break;
			default: break;
		}

		sr.sprite = pic;
		sr.transform.localScale = new Vector3(1f * (hFlip ? -1f : 1f), 1f, 1f) * scale;
		sr.transform.localRotation = Quaternion.Euler(0f, 0f, -90f * rotationLevel);
	}
}

