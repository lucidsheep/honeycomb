using UnityEngine;
using System.Collections;

public class Themeable : MonoBehaviour
{
    public int targetID;
    public bool switchOnInversion = true;
    public SpriteRenderer[] primarySprites;
    public SpriteRenderer[] secondarySprites;
    public SpriteRenderer[] iconSprites;
    public bool preserveAlpha;
    bool inverted = false;
    

    void SetColor(SpriteRenderer target, Color color)
    {
        if (preserveAlpha)
            target.color = new Color(color.r, color.g, color.b, target.color.a);
        else
            target.color = color;
    }

    private void Awake()
    {
        if(switchOnInversion)
            UIState.onInvert.AddListener(OnInvert);
        ViewModel.onThemeChange.AddListener(SetTheme);
    }
    private void Start()
    {
        SetTheme();
    }

    private void OnInvert(bool val)
    {
        SetTheme();
    }

    public void SetTheme()
    {
        var themeToUse = targetID == (switchOnInversion ? UIState.blue : 0) ? ViewModel.currentTheme.blueTheme : ViewModel.currentTheme.goldTheme;
        foreach (var sprite in primarySprites)
            SetColor(sprite, themeToUse.pColor);
        foreach (var sprite in secondarySprites)
            SetColor(sprite, themeToUse.sColor);
        foreach (var sprite in iconSprites)
            SetColor(sprite, themeToUse.iColor);
    }
}

