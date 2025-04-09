using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPoint : MonoBehaviour
{
    public enum DayType { Day, Night, Dusk, Twilight }
    public enum VictoryType { Military, Berries, Snail }

    public Sprite[] spriteOptionsBlue;
    public Sprite[] spriteOptionsGold;
    public GameObject shadow;
    public GameObject filledShadow;
    public Color emptyBlue = new Color(0x00, 0x5D, 0x87);
    public Color emptyGold = new Color(0x8C, 0x65, 0x00);
    public Color outlineBlue;
    public Color outlineGold;
    Sprite emptySprite;

    int tid;

    static int incrementalDay = 0;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        emptySprite = sr.sprite;
    }

    public void SetRandomSprite(int teamID)
    {
        tid = teamID;
        int index = (incrementalDay * 3) + Random.Range(1, 3);
        incrementalDay = incrementalDay == 3 ? 0 : incrementalDay + 1;
        sr.sprite = teamID == 0 ? spriteOptionsBlue[index] : spriteOptionsGold[index];
        sr.color = Color.white;
        if(filledShadow != null)
        {
            filledShadow.SetActive(true);
            shadow.SetActive(false);
        }
    }
    public void SetSprite(int teamID, DayType day, VictoryType victory)
    {
        tid = teamID;
        int index = ((int)day * 3) + (int)victory;
        sr.sprite = teamID == 0 ? spriteOptionsBlue[index] : spriteOptionsGold[index];
        sr.color = Color.white;
        if(filledShadow != null)
        {
            filledShadow.SetActive(true);
            shadow.SetActive(false);
        }
    }

    public void SetEmpty(int teamID)
    {
        tid = teamID;
        sr.sprite = emptySprite;
        shadow.SetActive(true);
        shadow.GetComponent<SpriteRenderer>().color = teamID == 0 ? outlineBlue : outlineGold;
        if(filledShadow != null)
            filledShadow.SetActive(false);
        sr.color = teamID == 0 ? emptyBlue : emptyGold;
    }

    void OnChangeSetPointVisibility(int before, int after)
    {
        sr.enabled = shadow.GetComponent<SpriteRenderer>().enabled = (after <= 0);
    }
}
