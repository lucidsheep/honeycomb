using UnityEngine;
using System.Collections;
using TMPro;

public class SetPointTextObserver : KQObserver
{
    public TextMeshPro label;

    int wins = 0;
    bool isVisible = false;
    bool dirty = false;


    private void Start()
    {
        GameModel.instance.setPoints.onChange.AddListener(OnMaxPointSet);
        GameModel.onGamePointForcedUpdate.AddListener(OnForcedUpdate);
        GameModel.onGameModelComplete.AddListener(OnGameComplete);
        //GameModel.instance.teams[0].setWins.onChange.AddListener((b, a) => { if (team == 0) SetWins(a); });
        //GameModel.instance.teams[1].setWins.onChange.AddListener((b, a) => { if (team == 1) SetWins(a); });
        NetworkManager.instance.tournamentEventDispatcher.AddListener(OnRealTournamentData);
        GameModel.onDelayedTournamentData.AddListener(OnTournamentData);
        //ViewModel.leftHexPadding.onChange.AddListener((b, a) => { if (team == 0) UpdatePositions(); });
        //ViewModel.rightHexPadding.onChange.AddListener((b, a) => { if (team == 1) UpdatePositions(); });

    }

    void OnForcedUpdate()
    {
        OnMaxPointSet(GameModel.instance.setPoints.property, GameModel.instance.setPoints.property);
        SetWins(GameModel.instance.teams[team].setWins.property);
    }
    void OnGameComplete(int winningTeam, string winType)
    {
        if (winningTeam == team && !GameModel.instance.isWarmup)
        {
                OnVictory();
        }
    }


    void OnRealTournamentData(HMMatchState data)
    {
        if (GameModel.newSetTimeout > 0f)
            return; //don't process if we're pending a reset
        OnTournamentData(data);
    }
    void OnTournamentData(HMMatchState data)
    {
        if (data.current_match == null || data.current_match.blue_team == null) // empty game, reset wins
        {
            //CancelDelay();
            wins = 0;
            OnMaxPointSet(0, 0);
            dirty = true;
        }
        else
        {
            //got tournament data
            var newWins = team == 0 ? data.current_match.blue_score : data.current_match.gold_score;
            if (newWins > wins || newWins < wins)
            {
                //model desync, hard set wins to value
                //CancelDelay();
                SetWins(newWins);
            }
        }

    }

    void OnVictory()
    {
        wins++;
        dirty = true;
    }

    void OnMaxPointSet(int before, int after)
    {
        isVisible = after > 0;
        dirty = true;
    }

    void ResetWins()
    {
        wins = 0;
        dirty = true;
    }

    void SetWins(int val)
    {
        wins = val;
        dirty = true;
    }

    private void Update()
    {
        if(dirty)
        {
            dirty = false;
            label.text = isVisible ? wins.ToString() : "";
        }
    }
}

