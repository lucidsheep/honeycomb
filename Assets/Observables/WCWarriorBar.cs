using System;
using UnityEngine;

public class WCWarriorBar : KQObserver
{
    public WarriorPip[] pips;
    int numWarriors = 0;

    float[] trackedPlayers = new float[]{0f,0f,0f,0f,0f,0f,0f,0f,0f,0f};
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
            trackedPlayers = new float[]{0f,0f,0f,0f,0f,0f,0f,0f,0f,0f};
        }
    }

    private void OnGameStart()
    {
        trackedPlayers = new float[]{0f,0f,0f,0f,0f,0f,0f,0f,0f,0f};
    }

    void Update()
    {
        if(!GameModel.instance.gameIsRunning) return;

        for(int i = 0; i < trackedPlayers.Length; i++)
        {
            if(trackedPlayers[i] > 0f)
            {
                trackedPlayers[i] -= Time.deltaTime;
                if(trackedPlayers[i] <= 0f)
                    AddPip(); //if we haven't gotten an exit event by now, the form is happening
            }
        }
    }
    private void OnGameEvent(string type, GameEventData data)
    {
        if (type == GameEventType.GATE_USE_START && data.teamID == targetID)
        {
           //check if the gate is a warrior gate
           Debug.Log("Gate form, feature is " + MapDB.currentMap.property.GetNearestFeature(data.coordinates).Item1.type.ToString());
           if (MapDB.currentMap.property.GetNearestFeature(data.coordinates).Item1.type == MapFeature.Type.SwordGate)
           {
            trackedPlayers[data.rawPlayerID] = 1f; //maybe?
           }
        }
        if (type == GameEventType.GATE_USE_CANCEL && data.teamID == targetID)
        {
            trackedPlayers[data.rawPlayerID] = 0f;
        }
        if(type == GameEventType.PLAYER_KILL && data.targetID == targetID && data.targetType == "Soldier")
        {
            RemovePip();
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
        pips[numWarriors].RemoveFill();
    }
}