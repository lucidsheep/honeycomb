using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputModeManager : MonoBehaviour
{
    public float heightPercent = .125f;
    public static bool inputModeEnabled = false;
    MainBarInputArea masterObject { get { return Object.FindObjectOfType<MainBarInputArea>(true); } }
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
        masterObject.gameObject.SetActive(true);
        masterObject.blueTeamName.text = GameModel.instance.teams[blue].teamName.property;
        masterObject.goldTeamName.text = GameModel.instance.teams[gold].teamName.property;
        masterObject.blueTeamScore.text = GameModel.instance.teams[blue].setWins.property + "";
        masterObject.goldTeamScore.text = GameModel.instance.teams[gold].setWins.property + "";
        masterObject.setScore.text = GameModel.instance.setPoints.property + "";
    }

    public void EndInputMode()
    {
        int blue = UIState.blue;
        int gold = UIState.gold;

        inputModeEnabled = false;
        masterObject.gameObject.SetActive(false);
        GameModel.instance.teams[blue].teamName.property = masterObject.blueTeamName.text;
        GameModel.instance.teams[gold].teamName.property = masterObject.goldTeamName.text;
        int blueScore = -1, goldScore = -1, setPoints = -1;
        int.TryParse(masterObject.blueTeamScore.text, out blueScore);
        int.TryParse(masterObject.goldTeamScore.text, out goldScore);
        int.TryParse(masterObject.setScore.text, out setPoints);

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
        masterObject.blueTeamScore.text = GameModel.instance.teams[blue].setWins.property.ToString();
        masterObject.goldTeamScore.text = GameModel.instance.teams[gold].setWins.property.ToString();
    }
}
