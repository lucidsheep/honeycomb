using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputModeManager : MonoBehaviour
{
    public float heightPercent = .125f;
    public static bool inputModeEnabled = false;
    public GameObject masterObject;
    public TMP_InputField blueTeamName, goldTeamName, blueTeamScore, goldTeamScore, setScore;
    //public Button

    private void Update()
    {
        if (SetupScreen.setupInProgress) return;
        if (LSConsole.visible) return;
        if (OverlayControlPanel.controlPanelActive) return;

        if((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            if (Input.mousePosition.y > Camera.main.pixelHeight * heightPercent && inputModeEnabled)
                EndInputMode();
            else if (Input.mousePosition.y <= Camera.main.pixelHeight * heightPercent && !inputModeEnabled)
                StartInputMode();
        }
    }

    void StartInputMode()
    {
        int blue = UIState.blue;
        int gold = UIState.gold;
        inputModeEnabled = true;
        masterObject.SetActive(true);
        blueTeamName.text = GameModel.instance.teams[blue].teamName.property;
        goldTeamName.text = GameModel.instance.teams[gold].teamName.property;
        blueTeamScore.text = GameModel.instance.teams[blue].setWins.property + "";
        goldTeamScore.text = GameModel.instance.teams[gold].setWins.property + "";
        setScore.text = GameModel.instance.setPoints.property + "";
    }

    public void EndInputMode()
    {
        int blue = UIState.blue;
        int gold = UIState.gold;

        inputModeEnabled = false;
        masterObject.SetActive(false);
        GameModel.instance.teams[blue].teamName.property = blueTeamName.text;
        GameModel.instance.teams[gold].teamName.property = goldTeamName.text;
        int blueScore = -1, goldScore = -1, setPoints = -1;
        int.TryParse(blueTeamScore.text, out blueScore);
        int.TryParse(goldTeamScore.text, out goldScore);
        int.TryParse(setScore.text, out setPoints);

        GameModel.instance.setPoints.property = setPoints >= 0 ? setPoints : 0;
        GameModel.instance.teams[blue].setWins.property = blueScore >= 0 ? blueScore : 0;
        GameModel.instance.teams[gold].setWins.property = goldScore >= 0 ? goldScore : 0;

        GameModel.onGamePointForcedUpdate.Invoke();
    }

    public void ResetSet()
    {
        int blue = UIState.blue;
        int gold = UIState.gold;

        GameModel.instance.ResetSet();
        blueTeamScore.text = GameModel.instance.teams[blue].setWins.property.ToString();
        goldTeamScore.text = GameModel.instance.teams[gold].setWins.property.ToString();
    }
}
