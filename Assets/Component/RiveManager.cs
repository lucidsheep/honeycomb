using UnityEngine;
using Rive;
using Rive.Components;
using UnityEngine.SocialPlatforms.Impl;

public class RiveManager : MonoBehaviour
{
    public RiveWidget riveObject;

    bool isLoaded = false;

    protected void Start()
    {
 
    }

    void Update()
    {
        if(!isLoaded && riveObject.Status == WidgetStatus.Loaded)
        {
            isLoaded = true;
            Debug.Log("state machine null " + riveObject.StateMachine == null);
            Debug.Log("state machine name " + riveObject.StateMachineName);
            Debug.Log("leaderboard null " + riveObject.StateMachine.ViewModelInstance == null);
            var leaderboardInstance = riveObject.StateMachine.ViewModelInstance;
            leaderboardInstance.GetStringProperty("leaderboardTitle").Value = "hooray";
            var player = leaderboardInstance.GetViewModelInstanceProperty("propertyOfLeaderboardSlotVm00");
            player.GetStringProperty("leaderboardName").Value = "Nivek";
            player.GetNumberProperty("leaderboardScore").Value = 6969;
        }
    }
}