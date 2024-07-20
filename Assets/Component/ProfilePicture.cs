using UnityEngine;
using System.Collections;

public class ProfilePicture : MonoBehaviour
{
	public SpriteRenderer sr;
	public SpriteRenderer frame;
	public SpriteMask mask;
	public bool isSquare = false;
	// Use this for initialization
	void Awake()
	{
		if (mask == null)
			mask = GetComponentInChildren<SpriteMask>();
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
		sr.transform.localScale = new Vector3(1f * (hFlip ? -1f : 1f), 1f, 1f) * scale * (isSquare ? 1.5f : 1f);
		sr.transform.localRotation = Quaternion.Euler(0f, 0f, -90f * rotationLevel);
	}

	public void SetLayer(int layer)
    {
		sr.sortingOrder = layer;
		mask.frontSortingOrder = layer;
		mask.backSortingOrder = layer - 1;
		if(frame != null)
			frame.sortingOrder = layer + 10;
    }

	public void SetColor(Color color, int teamID)
    {
		if (mask == null) return;
		mask.GetComponent<SpriteRenderer>().color = color;

		if(frame != null)
		{
			var themeFrame = AppLoader.GetStreamingSprite("profileFrame" + (teamID == 0 ? "Blue" : "Gold"));
			frame.sprite = themeFrame;
		}
    }

}

