using UnityEngine;
using System.Collections;
using DG.Tweening;

public class SetPointObserver : KQObserver
{
	public SetPoint spTemplate;
    public bool zigZagPattern = false;

    public Vector2 startPos;
    public float increment;
	SetPoint[] points;
    int wins = 0;

    Coroutine delayCoroutine;

    private void Start()
    {
        points = new SetPoint[0];
        GameModel.instance.setPoints.onChange.AddListener(OnMaxPointSet);
        GameModel.onGamePointForcedUpdate.AddListener(OnForcedUpdate);
        NetworkManager.instance.gameEventDispatcher.AddListener(OnGameEvent);
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
    void OnGameEvent(string type, GameEventData data)
    {
        if(type == GameEventType.GAME_END_DETAIL)
        {
            if(data.teamID == team)
                OnVictory(data.targetType);
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
        } else
        {
            //got tournament data
            var newWins = team == 0 ? data.current_match.blue_score : data.current_match.gold_score;
            if(newWins > wins || newWins < wins)
            {
                //model desync, hard set wins to value
                //CancelDelay();
                SetWins(newWins);
            }
        }

    }

    void CancelDelay()
    {
        if (delayCoroutine != null)
            StopCoroutine(delayCoroutine);
    }
    IEnumerator CheckWinsAfterDelay(int expectedWins)
    {
        yield return new WaitForSeconds(2f);
        if(expectedWins != wins)
        {
            //desync is still here, so hard set
            SetWins(expectedWins);
        }
    }
    void OnVictory(string winType)
    {
        wins++;
        Debug.Log("team " + team + " victory, stored wins " + wins);
        Debug.Log("win type is " + winType);
        Debug.Log("map name is " + MapDB.currentMap.property.name);
        if (wins - 1 >= points.Length) return;
        SetPoint.DayType day;
        switch(MapDB.currentMap.property.name)
        {
            case "map_day": day = SetPoint.DayType.Day; break;
            case "map_dusk": day = SetPoint.DayType.Dusk; break;
            case "map_night": day = SetPoint.DayType.Night; break;
            case "map_twilight": case "map_twilight2": day = SetPoint.DayType.Twilight; break;
            default: day = SetPoint.DayType.Day; break; //no twilight sprites yet
        }
        SetPoint.VictoryType victory;
        switch(winType)
        {
            case "military": victory = SetPoint.VictoryType.Military; break;
            case "snail": victory = SetPoint.VictoryType.Snail; break;
            case "economic":
            default: victory = SetPoint.VictoryType.Berries; break;
        }
        points[wins - 1].SetSprite(team, day, victory);
    }

    void OnMaxPointSet(int before, int after)
    {
        Debug.Log("maxpointset " + after);
        var storedSetPoints = points;
        points = new SetPoint[after];
        for(int j = 0; j < after; j++)
        {
            if (j < storedSetPoints.Length)
            {
                points[j] = storedSetPoints[j];
                storedSetPoints[j] = null;
            }
            else
            {
                var sp = Instantiate(spTemplate, ViewModel.stage);
                sp.transform.localPosition = new Vector3((targetID == 0 ? 1f : -1f), 0f, 0f);
                points[j] = sp;
                sp.SetEmpty(team);
            }
        }
        for(int k = 0; k < storedSetPoints.Length; k++)
        {
            if (storedSetPoints[k] != null)
                Destroy(storedSetPoints[k].gameObject);
        }
        UpdatePositions();
    }

    void UpdatePositions()
    {
        bool doZigZag = points.Length > 2 && zigZagPattern;
        for(int i = 0; i < points.Length; i++)
        {
            var sp = points[i];
            if (!doZigZag)
            {
                sp.transform.localPosition = new Vector3(
                    ((startPos.x + (targetID == 1 ? .02f : 0f) + (i * increment)) * (targetID == 0 ? 1f : -1f)), // + (targetID == 0 ? ViewModel.leftHexPadding.property : ViewModel.rightHexPadding.property),
                    startPos.y,
                    0f);
                if(targetID == 1)
                {
                    var ev = sp.transform.localScale;
                    ev.x *= -1f;
                    sp.transform.localScale = new Vector3(-88f, .88f, .88f);
                }
                
            } else
            {
                Vector3 pos = new Vector3();
                bool isHigh = i % 2 == 0;
                pos.x = ((7.9f + (i * 3.5f)) * (targetID == 0 ? -1f : 1f)); // + (targetID == 0 ? ViewModel.leftHexPadding.property : ViewModel.rightHexPadding.property);
                pos.y = isHigh ? -3.8f : -5.84f;
                sp.transform.localPosition = pos;
            }
        }
        float maxWidth = doZigZag
            ? ((3.4f + ((points.Length - 1) * 2.85f))) * (targetID == 0 ? -1f : 1f)
            : 4.5f * points.Length * (targetID == 0 ? -1f : 1f);
        if (targetID == 0)
            ViewModel.leftSetPadding.property = maxWidth;
        else
            ViewModel.rightSetPadding.property = maxWidth;
    }
    void ResetWins()
    {
        wins = 0;
        for (int i = 0; i < points.Length; i++)
        {
            points[i].SetEmpty(team);
        }
    }

    void SetWins(int val)
    {
        Debug.Log("team " + team + " setting wins to " + val + ", stored value " + wins);
        for(int i = 0; i < points.Length; i++)
        {
            if (i >= val)
                points[i].SetEmpty(team);
            else if (i >= wins)
                points[i].SetRandomSprite(team);
        }
        wins = val;
    }
}



