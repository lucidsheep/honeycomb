using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;

public class QueenKillReplayObserver : KQObserver
{
    public RawImage replayImage;
    public SpriteRenderer bg;

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
            var xCenter = Mathf.Min(.7f, Math.Max(0f, ((float)data.coordinates.x / 1920f) - .15f));
            var yCenter = Mathf.Min(.7f, Mathf.Max(0f, ((float)data.coordinates.y / 1080f) - .15f));
            replayImage.uvRect = new Rect(xCenter, yCenter, .3f, .3f);
            bg.transform.DOScaleX(8.54f, .2f).SetEase(Ease.OutQuad);
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
        bg.transform.DOScaleX(0f, .4f).SetDelay(1f).SetEase(Ease.InQuad);
		//replayImage.color = new Color(1f, 1f, 1f, 0f);
		//replayImage.gameObject.SetActive(false);
	}
}