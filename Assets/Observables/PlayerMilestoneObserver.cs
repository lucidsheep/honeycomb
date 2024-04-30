using UnityEngine;
using System.Collections;
using System;
using TMPro;

public class PlayerMilestoneObserver : KQObserver
{
	bool dirty = false;
	public float instantMilestoneInterval = 15f; //minimum time before new milestone report
	public float passiveMilestoneInterval = 30f; //minimum time before we search for anything interesting
	public float playerMilestoneInterval = 60f; //minimum time before showing another milestone form the same player
	public float milestoneOnScreenDuration = 10f;
	public PlayerMilestoneToast toastTemplate;

	DateTime lastMilestoneAnnouncement = DateTime.MinValue;
	bool checkedPassive = false;
    // Use this for initialization
    public override void Start()
	{
		base.Start();
		for (var i = 0; i < 2; i++)
		{
			foreach (var p in GameModel.instance.teams[i].players)
			{
				var copiedIndex = i; //this is needed to capture i and pass into funcs as value and not reference
				var player = p;
				p.onMilestoneAchieved.AddListener((m) => { if (team == copiedIndex) OnMilestone(m, player); });
			}
		}
	}

	void OnMilestone(PlayerModel.Milestone milestone, PlayerModel player)
    {
		if (!ViewModel.currentTheme.showMilestones) return;

		if(milestone.timescale == PlayerModel.StatTimescale.Career)
        {
			Debug.Log(player.displayName + " career milestone: " + milestone.description);
        } 
		float seconds = (float)(DateTime.Now - lastMilestoneAnnouncement).TotalSeconds;
		float playerSeconds = (float)(DateTime.Now - player.lastMilestoneUsed).TotalSeconds;
		if (seconds >= instantMilestoneInterval && playerSeconds >= playerMilestoneInterval)
		{
			player.UseMilestone();
			CreateToast(player, milestone);
		}
    }

	void CreateToast(PlayerModel player, PlayerModel.Milestone milestone)
    {
		var newMilestone = Instantiate(toastTemplate, transform.parent);
		newMilestone.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
		newMilestone.Init(player, milestone, targetID == 0 ? -1 : 1);
		lastMilestoneAnnouncement = DateTime.Now;
		checkedPassive = false;
    }

	private void Update()
	{
		if (!ViewModel.currentTheme.showMilestones) return;

		if(!checkedPassive && (float)(DateTime.Now - lastMilestoneAnnouncement).TotalSeconds >= passiveMilestoneInterval)
        {
			checkedPassive = true;
			if (!GameModel.instance.gameIsRunning)
				return;
			foreach(var p in GameModel.instance.teams[team].players)
            {
				if(p.bestUnusedMilestone.milestoneValue > 0 && ((float)(DateTime.Now - p.lastMilestoneUsed).TotalSeconds >= playerMilestoneInterval))
                {
					CreateToast(p, p.bestUnusedMilestone);
					p.UseMilestone();
					break;
                }
            }
        }
	}

}

