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
public class PostgameBoxScore
{
    public string name;
    public bool useCustomPosition;
    public float customPositionX;
    public float customPositionY;
    public float customScale;
}

[System.Serializable]
public class MatchPreviewStyle
{
    public string name;
    public bool useCustomPosition;
    public float customPositionX;
    public float customPositionY;
    public float customScale;
}

[System.Serializable]
public class MainBarStyle
{
    public string name;
    public bool useCustomPosition;
    public float customPositionX;
    public float customPositionY;
    public float customScale;
    public float cameraX;
    public float cameraY;
    public float cameraScale;
    public float customCameraWidth;
    public float customCameraHeight;
    public bool hideCameraIcons;
    public bool flipPlayerCameras;
}

[System.Serializable]
public class PostgamePlayerCardStyle
{
    public string name;
    public float xOffset;
    public float yOffset;
    public float width;
    public float scale;
    public string numberFont;
    public string statFont;
    public string nameFont;
    public float fontSize;
    public int fontSpacing;
    public float numberSize;
    public int numberSpacing;
}

[System.Serializable]
public class PositionTweak
{
    public string name;
    public float x, y, scale;
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
    public string matchPreviewStyle;
    public string videoSet;
    public string videoURLBlue, videoURLGold;
    public string playerCamRatio;
    public bool showTicker, showPlayerCams, showMilestones, showCrownAnimation, startReversed;
    public int leaderboardID;
    public float sidebarPadding;
    public bool useCustomCanvas;
    public bool postgameHideHeader;
    public float customCanvasX;
    public float customCanvasY;
    public float customCanvasScale;
    public string leaderboardTargetName;
    public int leaderboardTargetID;
    public ThemeTeamColors blueTheme;
    public ThemeTeamColors goldTheme;
    public string[] sideBarPrimary;
    public string[] sideBarSecondary;
    public MainBarStyle barStyle;
    public PostgameBoxScore boxScoreStyle;
    public MatchPreviewStyle matchPreview;
    public PostgamePlayerCardStyle playerCardStyle;
    public PositionTweak[] positionTweaks;

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

    public PlayerCameraObserver.AspectRatio GetAspectRatio()
    {
        switch(playerCamRatio)
        {
            case "ultrawide":
            case "Ultrawide": return PlayerCameraObserver.AspectRatio.Ultrawide;
            case "wide": case "Wide": default: return PlayerCameraObserver.AspectRatio.Wide;
        }
    }
    public ThemeTeamColors GetTeamTheme(int id) { if (id == 0) return blueTheme; return goldTheme; }

    public PositionTweak GetTweak(string elementName)
    {
        if (positionTweaks == null) return null;

        foreach(var t in positionTweaks)
        {
            if (elementName == t.name) return t;
        }
        return null;
    }

}

