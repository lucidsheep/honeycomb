using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class WinGraph : MonoBehaviour
{
	public Vector2 graphDimensions;
	public int numMarkers = 5;
	public SpriteRenderer bg;
	public SpriteRenderer[] gradients;
	public LineRenderer line;
	public GameObject timeMarkerTemplate;
	public GraphEventMarker eventMarkerTemplate;
	public float gradientAlpha;

	bool gameInProgress = false;
	bool dirty = false;

	public struct WinProbDataPoint
    {
		public float pct;
		public float timeStamp;
		public GameEventData eventData;
		public bool notableEvent;
    }

	List<WinProbDataPoint> eventList = new List<WinProbDataPoint>();
	List<GameObject> timeMarkerList = new List<GameObject>();
	List<GraphEventMarker> eventMarkerList = new List<GraphEventMarker>();
	// Use this for initialization
	void Start()
	{
		KQuityManager.onBlueWinProbability.AddListener(OnWinProb);
		GameModel.onGameStart.AddListener(OnGameStart);
		GameModel.onGameModelComplete.AddListener(OnGameEnd);

		bg.transform.localScale = new Vector3(graphDimensions.x, graphDimensions.y, 1f);
		line.transform.localPosition = new Vector3((graphDimensions.x * -.5f) + .1f, 0f, 0f);
	}

	void OnWinProb(float pct, GameEventData eventData)
	{
		if (!gameInProgress) return;

		var curTime = GameModel.instance.gameTime.property;
		var lastPct = eventList[eventList.Count - 1].pct;
		var lastTime = eventList[eventList.Count - 1].timeStamp;
		var noteworthy = (Mathf.Abs(lastPct - pct) > .1f) || (eventData.eventType == GameEventType.PLAYER_KILL && eventData.targetID == 2);

		if(curTime - lastTime > .3f)
			eventList.Add(new WinProbDataPoint() { pct = lastPct, timeStamp = curTime - .01f, eventData = eventData, notableEvent = false });
		eventList.Add(new WinProbDataPoint() { pct = pct, timeStamp = curTime, eventData = eventData, notableEvent = noteworthy });
    }

	void OnGameStart()
    {
		eventList.Clear();
		eventList.Add(new WinProbDataPoint() { pct = .5f, timeStamp = 0f, eventData = null });

		foreach(var g in timeMarkerList)
        {
			Destroy(g);
        }
		timeMarkerList.Clear();
		foreach(var e in eventMarkerList)
        {
			Destroy(e.gameObject);
        }
		eventMarkerList.Clear();

		gameInProgress = true;
		gradientAlpha = 0f;
		line.positionCount = 1;
    }

	void OnGameEnd(int _, string __)
    {
		gameInProgress = false;
		dirty = true;
		gradientAlpha = 0.572549f;

	}

	float[] timeScalesToTry = new float[] { 30f, 60f, 120f, 180f, 240f, 300f, 450f, 600f };

	void DrawGraph()
    {
		float startX = graphDimensions.x * -.5f + .1f;
		float startY = graphDimensions.y * -.5f + .05f;
		float finalGameTime = GameModel.instance.gameTime;
		if (finalGameTime <= 0f) return;
		//time markers
		float chosenTimescale = 600f;
		for(int i = 0; i < timeScalesToTry.Length; i++)
        {
			if(timeScalesToTry[i] * numMarkers >= finalGameTime)
            {
				chosenTimescale = timeScalesToTry[i];
				break;
            }
        }
		for(int i = 0; i < numMarkers; i++)
        {
			var thisTime = chosenTimescale * i;
			if (thisTime > finalGameTime) break;

			var thisPos = startX + ((thisTime / finalGameTime) * (graphDimensions.x - .2f));
			var thisMarker = Instantiate(timeMarkerTemplate, transform);
			thisMarker.transform.localPosition = new Vector3(thisPos, 0f, 0f);
			thisMarker.GetComponentInChildren<SpriteRenderer>().transform.localScale = new Vector3(.05f, graphDimensions.y, 1f);
			thisMarker.GetComponentInChildren<TextMeshPro>().text = Util.FormatTime(thisTime);
			thisMarker.GetComponentInChildren<TextMeshPro>().transform.localPosition = new Vector3(0f, graphDimensions.y * -.5f, 1f);
			timeMarkerList.Add(thisMarker);
        }
		//line.positionCount = 1; //reset points
		line.positionCount = eventList.Count + 1;
		int count = 1;
		foreach(var e in eventList)
        {
			var pointPosition = new Vector2(
				0f + ((e.timeStamp / finalGameTime)) * (graphDimensions.x - .2f),
				startY + (e.pct * (graphDimensions.y - .1f)));
			line.SetPosition(count, pointPosition);
			count++;

			if(e.notableEvent)
            {
				GraphEventMarker.GraphEventType eventTag;
				switch (e.eventData.eventType)
                {
					case GameEventType.BERRY_SCORE:
					case GameEventType.BERRY_KICK:
						eventTag = GraphEventMarker.GraphEventType.BERRY; break;
					case GameEventType.PLAYER_KILL:
						if (e.eventData.targetID == 2)
							eventTag = GraphEventMarker.GraphEventType.QUEEN;
						else
							eventTag = GraphEventMarker.GraphEventType.WARRIOR;
						break;
					case GameEventType.SNAIL_START:
					case GameEventType.SNAIL_END:
					case GameEventType.SNAIL_ESCAPE:
					case GameEventType.SNAIL_EAT:
						eventTag = GraphEventMarker.GraphEventType.SNAIL; break;
					case GameEventType.GATE_TAG:
					case GameEventType.GATE_USE_CANCEL:
					case GameEventType.GATE_USE_END:
					case GameEventType.GATE_USE_START:
						eventTag = GraphEventMarker.GraphEventType.GATE; break;
					default:
						eventTag = GraphEventMarker.GraphEventType.UNKNOWN; break;
				}
				var teamID = e.eventData.teamID;
				var thisMarker = Instantiate(eventMarkerTemplate, transform);
				thisMarker.transform.localPosition = new Vector3(pointPosition.x + startX, pointPosition.y, 0f);
				bool inverted = teamID == 1;
				if (e.pct < .3f) inverted = false;
				if (e.pct > .7f) inverted = true;
				thisMarker.transform.localRotation = Quaternion.Euler(0f, 0f, inverted ? 180f : 0f);
				thisMarker.Init(teamID, inverted, eventTag);
				eventMarkerList.Add(thisMarker);
				
            }
        }
    }
	// Update is called once per frame
	void Update()
	{
		if (dirty)
		{
			DrawGraph();
			dirty = false;
		}
		//if (bg.color.a > .5f)
		//	bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 92f / 255f);
		foreach(var s in gradients)
        {
			s.color = new Color(s.color.r, s.color.g, s.color.b, gradientAlpha);
		}
	}
}

