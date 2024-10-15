using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;

public class QueenKillReplayObserver : KQObserver
{
    public RawImage replayImage;

    RenderTexture replayRender;
    VideoClipper.Clip clip;

    enum State { Ready, Playing, Finishing }

    State state = State.Ready;
    public override void Start()
    {
        base.Start();
        GameModel.onGameEvent.AddListener(OnGameEvent);
        replayRender = new RenderTexture(1920, 1080, 24);
    }

    private void OnGameEvent(string type, GameEventData data)
    {
        if (type == GameEventType.PLAYER_KILL && data.targetType == "Queen")
        {
            if(state != State.Ready) return;

            //calculate if the game is over, don't show replay if it is
            var blueQueens = GameModel.instance.teams[1].players[2].curGameStats.deaths.property;
		    var goldQueens = GameModel.instance.teams[0].players[2].curGameStats.deaths.property;
            if(blueQueens >= MapDB.currentMap.property.queen_lives || goldQueens >= MapDB.currentMap.property.queen_lives)
                return;
            
            var index = VideoClipper.SaveClip();
            clip = VideoClipper.GetClip(index);
            clip.AdvanceFrames(200);
            replayImage.texture = clip.texture;
            replayImage.material.mainTexture = clip.texture;
            state = State.Playing;
            clip.AdvancePlayback();
            replayImage.texture = replayRender;
            var xCenter = Mathf.Min(.85f, Math.Max(.15f, ((float)data.coordinates.x / 1920f) - .15f));
            var yCenter = Mathf.Min(.85f, Mathf.Max(.15f, ((float)data.coordinates.y / 1080f) - .15f));
            replayImage.uvRect = new Rect(xCenter, yCenter, .3f, .3f);
            replayImage.DOColor(Color.white, .5f);
        }
    }

    private void FixedUpdate()
    {
		if (state == State.Playing)
		{
            //Debug.Log(clip.GetProgress());
			bool done = clip.AdvancePlayback();
			if (!done)
			{
				if(VideoClipper.canDownResImage)
					Graphics.Blit(clip.texture, replayRender);
				else
					Graphics.CopyTexture(clip.texture, replayRender);
                //Debug.Log(clip.GetProgress());
			}
			else
			{
				FinishReplay();
			}
		}
	}
	void FinishReplay()
    {
        Debug.Log("finish replay");
        state = State.Finishing;
        replayImage.DOColor(new Color(1f,1f,1f,0f), .5f).SetDelay(1f).OnComplete(() => state = State.Ready);
		//replayImage.color = new Color(1f, 1f, 1f, 0f);
		//replayImage.gameObject.SetActive(false);
	}
}