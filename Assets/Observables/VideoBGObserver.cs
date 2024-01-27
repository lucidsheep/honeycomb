using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class VideoBGObserver : KQObserver
{
	public MeshRenderer bgMesh;
	public VideoPlayer player;

	// Use this for initialization
	override public void Start()
	{
		base.Start();
		bgMesh.enabled = false;
	}

    protected override void OnThemeChange()
    {
        base.OnThemeChange();

		var url = AppLoader.GetAssetPath("background.mp4");
		if(url == "")
        {
			player.Stop();
			bgMesh.enabled = false;
        } else
        {
			player.url = url;
			bgMesh.enabled = true;
			player.Play();
        }
    }
}

