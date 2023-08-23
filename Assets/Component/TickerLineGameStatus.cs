using UnityEngine;
using System.Collections;
using TMPro;

public class TickerLineGameStatus : TickerLineItem
{
    public TextMeshPro blueTeamName, blueTeamScore, goldTeamName, goldTeamScore, dayTimeTxt, seriesTxt;
    public SpriteRenderer blueQueenIcon, blueBerryIcon, blueSnailIcon, blueScoreBG, goldQueenIcon, goldBerryIcon, goldSnailIcon, goldScoreBG;
    public Color winConditionHighlightColor;

    //0 = queens, 1 = berries, 2 = snail
    int blueScoreFocus = -1, goldScoreFocus = -1;

    public void Init(TickerNetworkManager.TickerCabinetState cabState)
    {
        var blueTeam = cabState.gameInProgress ? cabState.blueTeam : cabState.blueTeamCached;
        var goldTeam = cabState.gameInProgress ? cabState.goldTeam : cabState.goldTeamCached;

        blueTeamName.text = blueTeam.teamName;
        goldTeamName.text = goldTeam.teamName;

        if (cabState.isTournamentPlay || cabState.isTournamentSeriesFinished)
        {
            var winningTeam = blueTeam.seriesScore > goldTeam.seriesScore ? blueTeam : goldTeam;
            var otherTeam = blueTeam.seriesScore > goldTeam.seriesScore ? goldTeam : blueTeam;

            if (winningTeam.seriesScore == otherTeam.seriesScore)
                seriesTxt.text = "Series tied " + winningTeam.seriesScore + "-" + winningTeam.seriesScore;
            else
                seriesTxt.text = winningTeam.teamName + " " + (cabState.isTournamentSeriesFinished ? "wins" : "leads") + " series " + winningTeam.seriesScore + "-" + otherTeam.seriesScore;
        }
        else
        {
            seriesTxt.text = "";
        }

        dayTimeTxt.text = cabState.mapName + " | " + Util.FormatTime(cabState.gameTime) + (cabState.gameInProgress ? "" : " | Final");


        if (!cabState.gameInProgress)
        {
            if (!cabState.gameWinner)
            {
                blueScoreBG.color = winConditionHighlightColor;
            }
            else
            {
                goldScoreBG.color = winConditionHighlightColor;
            }
        }
        var bluePressure = GetHighestPressureScore(blueTeam.queenScore, blueTeam.berryScore, blueTeam.snailScore, false, cabState);
        var goldPressure = GetHighestPressureScore(goldTeam.queenScore, goldTeam.berryScore, goldTeam.snailScore, true, cabState);

        blueScoreFocus = bluePressure.Item1;
        goldScoreFocus = goldPressure.Item1;

        blueTeamScore.text = bluePressure.Item2.ToString() + (blueScoreFocus == 2 ? "%" : "");
        goldTeamScore.text = goldPressure.Item2.ToString() + (goldScoreFocus == 2 ? "%" : "");

        blueQueenIcon.gameObject.SetActive(blueScoreFocus == 0);
        blueBerryIcon.gameObject.SetActive(blueScoreFocus == 1);
        blueSnailIcon.gameObject.SetActive(blueScoreFocus == 2);
        goldQueenIcon.gameObject.SetActive(goldScoreFocus == 0);
        goldBerryIcon.gameObject.SetActive(goldScoreFocus == 1);
        goldSnailIcon.gameObject.SetActive(goldScoreFocus == 2);

    }

    //win type, adjusted score
    (int, int) GetHighestPressureScore(int queens, int berries, int snail, bool isGold, TickerNetworkManager.TickerCabinetState cabState)
    {
        if(!cabState.gameInProgress && cabState.gameWinner == isGold)
        {
            //force win condition to highest number
            if (cabState.winType == "military")
                return (0, 3);
            if (cabState.winType == "economic")
                return (1, 12);
            if (cabState.winType == "snail")
                return (2, 100);
        }

        int queenPressure = queens == 0 ? 0 : queens == 1 ? 24 : queens == 2 ? 80 : 100;
        int berryPressure = berries < 4 ? berries * 2 : Mathf.Min(100, (berries - 0) * (berries - 0));
        int snailPressure = snail;

        if (berryPressure > queenPressure && berryPressure > snailPressure)
            return (1, berries);
        if (snailPressure > queenPressure && snailPressure > berryPressure)
            return (2, snail);

        return (0, queens);
    }
}

