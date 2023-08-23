using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Video;

[CreateAssetMenu()]
public class ThemeData : ScriptableObject
{
	[System.Serializable]
	public struct TeamTheme
    {
		public Color primaryColor;
		public Color secondaryColor;
		public Color iconColor;
		public string postgameVideoURL;
    }
	[System.Serializable]
	public enum LayoutStyle { OneCol_Right, OneCol_Left, Game_Only, TwoCol}
	public string themeName;
	public TeamTheme blueTheme;
	public TeamTheme goldTheme;
	public TMP_FontAsset teamNameFont;
	public TMP_FontAsset postgameFont;
	public bool showPlayerCams;
	public LayoutStyle layout;
	public string[] sidebarModules;
	public string[] sidebarModules_Left;
	public string replayVideoURL;
	public string videoSetName;
	public GameObject postGameSecondaryScreen;
	public float sideBarPadding;
	public bool showBuzzBar;
	public string playerCardFont;
	public string postgameHeaderFont;
	public int leaderboardID;
	public bool hideMilestones;
	public bool hideCrownAnimation;

	public TeamTheme GetTeamTheme(int id) { if (id == 0) return blueTheme; return goldTheme; }
}

[System.Serializable]
public class ThemeXML
{
	public string name;
	public TickerThemeData[] tickerLineItems;
}

