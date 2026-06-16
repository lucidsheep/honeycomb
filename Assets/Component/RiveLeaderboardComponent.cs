using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LeaderboardEntry : RiveSubComponent
{
    public RiveString leaderboardName;
    public RiveInt leaderboardScore;
    public RiveBool leaderboardRankup;
    public void Init(ViewModelInstance vm, string componentName)
    {
        viewModel = vm.GetViewModelInstanceProperty(componentName);
        leaderboardName = new RiveString(viewModel, "leaderboardName");
        leaderboardScore = new RiveInt(viewModel, "leaderboardScore");
        leaderboardRankup = new RiveBool(viewModel, "leaderboardRankupBarAndArrow");
    }
}
public class RiveLeaderboardComponent : RiveComponent
{
    public RiveWidget testWidget; //test
    public RiveStringRun testRun;
    public RiveString leaderboardCategory;
    public RiveBool leaderboardSwitch;
    public LeaderboardEntry[] leaderboardList;
    protected override void OnLoaded()
    {
        base.OnLoaded();
        leaderboardCategory = new RiveString(viewModel, "leaderboardCategory");
        leaderboardSwitch = new RiveBool(viewModel, "categorySwitch");
        leaderboardList = new LeaderboardEntry[10];

        for(int i = 0; i < 10; i++)
        {
            var thisEntry = new LeaderboardEntry();
            thisEntry.Init(viewModel, "propertyOfLeaderboardSlotVm0" + i);
            leaderboardList[i] = thisEntry;
        }

        testRun = new RiveStringRun(testWidget.StateMachine.ViewModelInstance, "run", "Nǟȶɦǟn");
    }

    public void SetLeaderboardEntry(int entryNumber, string playerName, int score, bool rankUp)
    {
        if(!isLoaded || entryNumber >= leaderboardList.Length) return;
        var thisEntry = leaderboardList[entryNumber];
        thisEntry.leaderboardName.property = playerName;
        thisEntry.leaderboardScore.property = score;
        thisEntry.leaderboardRankup.property = rankUp;
    }
}