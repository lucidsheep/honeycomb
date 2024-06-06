using System.Buffers.Text;
using UnityEngine;

public class PlayerCameraSubView : SubView
{
    CamData blueCam = new CamData();
    CamData goldCam = new CamData();
    public override void OnSubViewStarted(params string[] args)
    {
        base.OnSubViewStarted(args);

        (blueCam.pos, blueCam.scale) = PlayerCameraObserver.SetCustomCameraView("blueCamera", new Vector2(-6.56f, 0f), 1.75f);
        (goldCam.pos, goldCam.scale) = PlayerCameraObserver.SetCustomCameraView("goldCamera", new Vector2(6.56f, 0f), 1.75f);
    }

    public override void OnSubViewClosed(params string[] args)
    {
        base.OnSubViewClosed(args);

        PlayerCameraObserver.SetCustomCameraView("blueCamera", blueCam.pos, blueCam.scale);
        PlayerCameraObserver.SetCustomCameraView("goldCamera", goldCam.pos, goldCam.scale);
    }
}