using UnityEngine;
using System.Collections;

public class GraphEventMarker : MonoBehaviour
{
	public SpriteRenderer arrow, bg, queenEvent, warriorEvent, berryEvent, snailEvent, gateEvent;

    public enum GraphEventType { QUEEN, WARRIOR, BERRY, SNAIL, GATE, UNKNOWN }

    SpriteRenderer[] eventSprites;

    public void Init(int teamID, bool inverted, GraphEventType eventType)
    {
        eventSprites = new SpriteRenderer[5] { queenEvent, warriorEvent, berryEvent, snailEvent, gateEvent };
        var theme = ViewModel.currentTheme.GetTeamTheme(teamID);
        var rotation = eventType == GraphEventType.WARRIOR ? -90f : 0f;
        if (inverted) rotation += 180f;
        int typeToInt = (int)eventType;
        for(int i = 0; i < eventSprites.Length; i++)
        {
            SpriteRenderer thisSprite = eventSprites[i];
            thisSprite.enabled = i == typeToInt;
            thisSprite.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            thisSprite.color = theme.iconColor;
        }
        arrow.color = theme.primaryColor;
        bg.color = theme.secondaryColor;
    }
}

