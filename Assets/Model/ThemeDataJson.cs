using UnityEngine;
using System.Collections;

[System.Serializable]
public class ThemeTeamColors
{
    public string primaryColor, secondaryColor, iconColor;
    public Color pColor { get { return StringToColor(primaryColor); } }
    public Color sColor { get { return StringToColor(secondaryColor); } }
    public Color iColor { get { return StringToColor(iconColor); } }

    public static Color StringToColor(string hexColor)
    {
        if (hexColor.Length == 0) return Color.white;

        if (hexColor[0] == '#')
            hexColor = hexColor.Substring(1, hexColor.Length - 1);

        if (hexColor.Length < 6) return Color.white;

        float r = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        float g = int.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        float b = int.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        float a = hexColor.Length < 8 ? 255f : int.Parse(hexColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}
[System.Serializable]
public class ThemeDataJson
{
    public enum LayoutStyle { OneCol_Right, OneCol_Left, Game_Only, TwoCol }

    public string name;
    public string layout;
    public string headerFont, postgameHeaderFont, postgameCardFont, postgameDetailFont;
    public string postgameScreen;
    public string mainBarStyle;
    public string videoSet;
    public bool showTicker, showPlayerCams, showMilestones, showCrownAnimation, startReversed;
    public int leaderboardID;
    public float sidebarPadding;
    public bool useCustomCanvas;
    public float customCanvasX;
    public float customCanvasY;
    public float customCanvasScale;
    public ThemeTeamColors blueTheme;
    public ThemeTeamColors goldTheme;
    public string[] sideBarPrimary;
    public string[] sideBarSecondary;

    public LayoutStyle GetLayout()
    {
        switch (layout)
        {
            case "oneColumnRight": case "oneColumn": return LayoutStyle.OneCol_Right;
            case "oneColumnLeft": return LayoutStyle.OneCol_Left;
            case "gameOnly": return LayoutStyle.Game_Only;
            case "twoColumn": return LayoutStyle.TwoCol;
            default: return LayoutStyle.OneCol_Right;
        }
    }

    public ThemeTeamColors GetTeamTheme(int id) { if (id == 0) return blueTheme; return goldTheme; }

}

