using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SnailModel : MonoBehaviour
{
	public static SnailModel instance;

	const float snailPixelsPerSecond_normal = 20.896215463f;
	const float snailPixelsPerSecond_speed = 28.209890875f;

	public static float SNAIL_METER = 20.896215463f;

	public static LSProperty<float> currentPosition = new LSProperty<float>(0f);
	public static LSProperty<int> riderTeam = new LSProperty<int>(-1); //-1 = vacant
	int lastPosession = -1;
	int riderID = 0;
	public static LSProperty<bool> isSpeedSnail = new LSProperty<bool>(false);

	int eatenID = 0;
	public static LSProperty<bool> eatInProgress = new LSProperty<bool>(false);

	int snailStart_absolutePosition = 0;
	int snailRideStartPosition = 0;

	public static UnityEvent<float> onSnailMoveEstimate;
	public static LSProperty<int> bluePercentage;
	public static LSProperty<int> goldPercentage;
	public static LSProperty<int> blueSecondsLeft;
	public static LSProperty<int> goldSecondsLeft;
	// Use this for initialization
	void Awake()
	{
		instance = this;
		onSnailMoveEstimate = new UnityEvent<float>();
		bluePercentage = new LSProperty<int>(0);
		goldPercentage = new LSProperty<int>(0);
		blueSecondsLeft = new LSProperty<int>(0);
		goldSecondsLeft = new LSProperty<int>(0);
	}

    private void Start()
    {
		NetworkManager.instance.gameEventDispatcher.AddListener(OnGameEvent);
		ResetModel();
    }
	void OnGameEvent(string type, GameEventData data)
    {
		//player is riding snail and eats an enemy drone
		if (type == GameEventType.PLAYER_KILL && data.targetID == eatenID && lastPosession == data.teamID)
		{
			eatInProgress.property = false;
			//weird logic - letting a snail eat is worse for the eater as it gets closer to goal
			GameModel.instance.teams[data.teamID].players[data.playerID].AddDerivedStat(PlayerModel.StatValueType.SnailKills, 20 - (int)GameModel.instance.GetObjPressureRating(0, GameModel.ObjType.Snail), 1);
			//likewise, a sac when near the goal is more valuable
			GameModel.instance.teams[1 - data.teamID].players[eatenID].AddDerivedStat(PlayerModel.StatValueType.SnailFeeds, 3 * (int)GameModel.instance.GetObjPressureRating(0, GameModel.ObjType.Snail), 1);
			GameModel.instance.teams[1 - data.teamID].players[eatenID].curGameStats.snailDeaths.property += 1;
		}
		//player was being eaten and rider was killed
		if (type == GameEventType.SNAIL_ESCAPE && data.playerID == eatenID && lastPosession != data.teamID)
		{
			eatInProgress.property = false;
			GameModel.instance.teams[data.teamID].players[eatenID].AddDerivedStat(PlayerModel.StatValueType.SnailKills, 25, 1, 0);
		}
		if (type == GameEventType.GAME_END_DETAIL)
		{

		}
    }
    // Update is called once per frame
    void Update()
	{
		//Debug.Log(riderTeam.ToString());
		if (eatInProgress.property)
        {
			//do nothing
			//Debug.Log("eap");
			return;
        }
		if(riderTeam.property > -1)
        {
			if (CVManager.useManager)
			{
				var cvPosition = CVManager.snailObservedPosition;
				if (UIState.inverted)
					cvPosition = 1920f - cvPosition;
				currentPosition.property = cvPosition;
			}
			else
			{
				var dis = (isSpeedSnail.property ? snailPixelsPerSecond_speed : snailPixelsPerSecond_normal) * Time.deltaTime;
				currentPosition.property += (dis * (riderTeam == 0 ? -1f : 1f) * UIState.invertf);
			}
			onSnailMoveEstimate.Invoke(currentPosition.property);
			UpdatePercentages(Mathf.FloorToInt(currentPosition.property));
        }
	}

	public void OnGameEnd(bool snailVictory)
    {
		if (snailVictory)
		{
			currentPosition.property = 960f + (UIState.inverti * (riderTeam == 0 ? -1 : 1) * trackDistance);
			UpdatePercentages(Mathf.FloorToInt(currentPosition.property));
			OnSnailEnd(Mathf.FloorToInt(currentPosition.property));
		} else if(riderTeam.property > -1)
        {
			OnSnailEnd(Mathf.FloorToInt(currentPosition.property));
        }
		eatInProgress.property = false;
		riderTeam.property = -1;
		//now awarding style points per snail run, uncomment to award all at end
		/*
		foreach(var t in GameModel.instance.teams)
        {
			foreach(var p in t.players)
            {
				p.AddDerivedStat(PlayerModel.StatValueType.Snail, p.curGameStats.snailMoved.property / 4, p.curGameStats.snailMoved.property);
            }
        }
		*/
	}
	public static void ResetModel()
    {
		currentPosition.property = instance.snailStart_absolutePosition = instance.snailRideStartPosition = 960; //fixed mid point of map
		instance.riderID = 0;
		riderTeam.property = -1;
		instance.lastPosession = -1;
		isSpeedSnail.property = false;
		bluePercentage.property = goldPercentage.property = 0;
		instance.eatenID = 0; eatInProgress.property = false;
		UpdatePercentages(instance.snailStart_absolutePosition);
	}

	public static void OnSnailStart(int pos, int tid, int pid, bool isSpeed)
    {
		if (tid < 0 || pid < 0) return;

		currentPosition.property = pos;
		instance.snailRideStartPosition = pos;
		isSpeedSnail.property = isSpeed;
		riderTeam.property = tid;
		instance.lastPosession = tid;
		instance.riderID = pid;
    }

	public static void OnSnailEat(int pos, int victimID)
    {
		currentPosition.property = pos;
		eatInProgress.property = true;
		instance.eatenID = victimID;
		UpdatePercentages(pos);
    }

	public static void OnSnailEnd(int pos)
    {
		if (riderTeam < 0 || instance.riderID < 0) return;

		int distancePixels = Mathf.Abs(instance.snailRideStartPosition - pos);
		GameModel.instance.teams[riderTeam.property].players[instance.riderID].curLifeStats.snailMoved.property += distancePixels;
		GameModel.instance.teams[riderTeam.property].players[instance.riderID].AddDerivedStat(PlayerModel.StatValueType.Snail, distancePixels / 4, distancePixels);
		currentPosition.property = pos;
		eatInProgress.property = false;
		isSpeedSnail.property = false;
		instance.riderID = 0;
		riderTeam.property = -1;
		UpdatePercentages(pos);
	}
	static int trackDistance { get { return MapDB.currentMap.property.snail_track_width; } }
	static void UpdatePercentages(int pos)
    {
		var left = UIState.inverted ? goldPercentage : bluePercentage;
		var right = UIState.inverted ? bluePercentage : goldPercentage;

		left.property = Mathf.Max(0, Mathf.FloorToInt(100f * ((float)(960f - pos) / trackDistance)));
		right.property = Mathf.Max(0, Mathf.FloorToInt(100f * ((float)(pos - 960f) / trackDistance )));

		var leftPixelsLeft = Mathf.Max(0f, pos - (960f - (trackDistance / 1f)));
		var rightPixelsLeft = Mathf.Max(0f, (960f + (trackDistance / 1f)) - pos);

		var leftTimeLeft = leftPixelsLeft / snailPixelsPerSecond_normal;
		var rightTimeLeft = rightPixelsLeft / snailPixelsPerSecond_normal;

		//Debug.Log("pixels left " + leftPixelsLeft + "right " + rightPixelsLeft);
		//Debug.Log("time left " + leftTimeLeft + "right " + rightTimeLeft);

		//display time with speed snail factored in
		//var leftTimeLeft = isSpeedSnail.property && riderTeam.property == UIState.blue ? leftPixelsLeft / snailPixelsPerSecond_speed : leftPixelsLeft / snailPixelsPerSecond_normal;
		//var rightTimeLeft = isSpeedSnail.property && riderTeam.property == UIState.gold ? rightPixelsLeft / snailPixelsPerSecond_speed : rightPixelsLeft / snailPixelsPerSecond_normal;

		blueSecondsLeft.property = Mathf.CeilToInt(UIState.inverted ? rightTimeLeft : leftTimeLeft);
		goldSecondsLeft.property = Mathf.CeilToInt(UIState.inverted ? leftTimeLeft : rightTimeLeft);

	}
}

