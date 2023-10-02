using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class OverlayControlPanel : MonoBehaviour
{

	public TMP_InputField blueName, goldName, blueScore, goldScore, maxScore;
	//public Toggle doubleElim, instantReplay, computerVision, virtualDesktop, lowResCamera;

	public static bool controlPanelActive = false;
	// Use this for initialization
	void Start()
	{
		for (int i = 0; i < 2; i++)
		{
			var copiedIndex = i;
			GameModel.instance.teams[copiedIndex].teamName.onChange.AddListener((b, a) => { OnTeamNameUpdated(copiedIndex == 0, a); });
			GameModel.instance.teams[copiedIndex].setWins.onChange.AddListener((b, a) => { OnTeamScoreUpdated(copiedIndex == 0, a); });
			GameModel.instance.setPoints.onChange.AddListener((b, a) => OnMaxPointChange(a));

			if(i == 0)
            {
				blueName.text = GameModel.instance.teams[copiedIndex].teamName.property;
				blueScore.text = GameModel.instance.teams[copiedIndex].setWins.property.ToString();
			} else
            {
				goldName.text = GameModel.instance.teams[copiedIndex].teamName.property;
				goldScore.text = GameModel.instance.teams[copiedIndex].setWins.property.ToString();
			}
		}
		maxScore.text = GameModel.instance.setPoints.property.ToString();
		controlPanelActive = true;
	}

	 void OnTeamNameUpdated(bool blueTeam, string name)
    {
		if (blueTeam)
			blueName.text = name;
		else
			goldName.text = name;
    }

	 void OnTeamScoreUpdated(bool blueTeam, int score)
    {
		if (blueTeam)
			blueScore.text = score.ToString();
		else
			goldScore.text = score.ToString();
    }

	void OnMaxPointChange(int score)
    {
		maxScore.text = score.ToString();
    }

	public void UpdateTeamData()
    {
		for(int i = 0; i < 2; i++)
        {
			GameModel.instance.teams[i].teamName.property = i == 0 ? blueName.text : goldName.text;
			GameModel.instance.teams[i].setWins.property = int.Parse(i == 0 ? blueScore.text : goldScore.text);
		}
		GameModel.instance.setPoints.property = int.Parse(maxScore.text);
		GameModel.onGamePointForcedUpdate.Invoke();
	}

	public void NewSet()
	{
		GameModel.instance.ResetSet();
		GameModel.onGamePointForcedUpdate.Invoke();
	}

	public void ForceClosePostgame()
    {
		PostGameScreen.ForceClosePostgame();
		MatchPreviewScreen.ForceClosePreview();
    }
	// Update is called once per frame
	void Update()
	{
			
	}
}

