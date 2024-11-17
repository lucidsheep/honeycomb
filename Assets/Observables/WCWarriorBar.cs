using System;
using UnityEngine;

public class WCWarriorBar : KQObserver
{
    public WarriorPip[] pips;
    int numWarriors = 0;
    int numConfirmedWarriors = 0;

    float[] trackedPlayers = new float[]{0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f};
    public override void Start()
    {
        base.Start();

        GameModel.onGameEvent.AddListener(OnGameEvent);
        GameModel.onGameStart.AddListener(OnGameStart);
        GameModel.onGameModelComplete.AddListener(OnGameEnd);
    }

    private void OnGameEnd(int arg0, string arg1)
    {
        while(numWarriors > 0)
        {
            numWarriors--;
            pips[numWarriors].RemoveFill();
            trackedPlayers = new float[]{0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f};
        }
        numConfirmedWarriors = 0;
    }

    private void OnGameStart()
    {
        trackedPlayers = new float[]{0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f};
        numWarriors = numConfirmedWarriors = 0;
    }

    void Update()
    {
        if(!GameModel.instance.gameIsRunning) return;

        for(int i = 0; i < trackedPlayers.Length; i++)
        {
            if(trackedPlayers[i] > 0f)
            {
                trackedPlayers[i] -= Time.deltaTime;
                if(trackedPlayers[i] <= 0f) {
                    AddPip(); //if we haven't gotten an exit event by now, the form is happening
                    trackedPlayers[i] -= .1f;
                }
            } else if(trackedPlayers[i] < 0f)
            {
                trackedPlayers[i] -= Time.deltaTime;
                if(trackedPlayers[i] < -3f)
                {
                    //stray start event without a complete, cancel it
                    trackedPlayers[i] = 0f;
                    numWarriors--;
                    if(numWarriors < 0) numWarriors = 0;
                    pips[numWarriors].RemoveFill();
                }
            }
        }
    }
    private void OnGameEvent(string type, GameEventData data)
    {
        if (type == GameEventType.GATE_USE_START && data.teamID == targetID)
        {
           //check if the gate is a warrior gate
           //Debug.Log("Gate form, feature is " + MapDB.currentMap.property.GetNearestFeature(data.coordinates).Item1.type.ToString());
           if (MapDB.currentMap.property.GetNearestFeature(data.coordinates).Item1.type == MapFeature.Type.SwordGate)
           {
            if(trackedPlayers[data.rawPlayerID] == 0f)
                trackedPlayers[data.rawPlayerID] = .75f; //start tracking only if we weren't already tracking
           }
        }
        if( type == GameEventType.GATE_USE_END && data.teamID == targetID && data.targetType == EventTargetType.GATE_SWORD)
        {
            trackedPlayers[data.rawPlayerID] = 0f;
            if(numConfirmedWarriors < 4)
            {
                pips[numConfirmedWarriors].CompleteFill();
                //sanity check?
                for(int i = numConfirmedWarriors - 1; i >= 0; i--)
                {
                    pips[i].CompleteFill();
                }
                numConfirmedWarriors++;
                if(numWarriors < numConfirmedWarriors) numWarriors = numConfirmedWarriors;
            }
        }
        //do something with gate_use_end and confirming a warrior
        if (type == GameEventType.GATE_USE_CANCEL && data.teamID == targetID)
        {
            if(trackedPlayers[data.rawPlayerID] < 0f)
            {
                //cancel the anim if it started already
                numWarriors--;
                if(numWarriors < 0) numWarriors = 0;
                pips[numWarriors].RemoveFill();
            }
            trackedPlayers[data.rawPlayerID] = 0f;
        }
        if(type == GameEventType.PLAYER_KILL && data.teamID != targetID && data.targetType == "Soldier")
        {
            RemovePip();
            trackedPlayers[data.rawPlayerID] = 0f;
        }
    }

    void AddPip()
    {
        if(numWarriors >= 4) return;

        pips[numWarriors].StartFill();
        numWarriors++;
    }

    void RemovePip()
    {
        if(numWarriors <= 0) return;

        numWarriors--;
        numConfirmedWarriors--;
        pips[numWarriors].RemoveFill();
    }
}