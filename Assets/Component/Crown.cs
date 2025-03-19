using UnityEngine;
using System.Collections;

public class Crown : MonoBehaviour
{
	public SpriteRenderer main, outline;
	public Sprite empty_blue, empty_gold, full_blue, full_gold;
	public Color emptyColor = Color.black;
	public bool useColor = true;

	public void SetCrown(int team, bool filled, Color color)
	{

		if (team == -1)
		{
			main.sprite = empty_blue;
			main.color = emptyColor;
		} else
        {
			bool isBlue = team == UIState.blue;
			if(isBlue)
            {
				main.sprite = filled ? full_blue : empty_blue;
            } else
            {
				main.sprite = filled ? full_gold : empty_gold;
			}
			main.color = !useColor ? Color.white : color;
        }
	}
}

