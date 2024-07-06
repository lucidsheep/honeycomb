using UnityEngine;
using TMPro;

public class CreditsSubview : SubView
{
    public GoogleSheetsDB creditsData;
    public TextMeshPro creditsText;
    public SpriteRenderer bg;
    public float scrollRate = 1f;
    
    float destY = 0f;
    float yInc = 0.328986567f;
    bool dataLoaded = false;
    bool isScrolling = false;

    CamData blueCam = new CamData();
    CamData goldCam = new CamData();

    public override void OnSubViewStarted(params string[] args)
    {
        base.OnSubViewStarted(args);

        if(dataLoaded)
            SetCredits();
        else
        {
            creditsData.ImportData("campkq");
            creditsData.OnDownloadComplete += () => {dataLoaded = true; SetCredits(); };
        }
        bg.sprite = AppLoader.GetStreamingSprite("background_night");

        (blueCam.pos, blueCam.scale) = PlayerCameraObserver.SetCustomCameraView("blueCamera", new Vector2(-26.56f, 0f), 1f);
        (goldCam.pos, goldCam.scale) = PlayerCameraObserver.SetCustomCameraView("goldCamera", new Vector2(26.56f, 0f), 1f);
    }

    public override void OnSubViewClosed(params string[] args)
    {
        base.OnSubViewClosed(args);

        PlayerCameraObserver.SetCustomCameraView("blueCamera", blueCam.pos, blueCam.scale);
        PlayerCameraObserver.SetCustomCameraView("goldCamera", goldCam.pos, goldCam.scale);
    }

    void SetCredits()
    {
        int txtSheetIndex = creditsData.sheetTabNames.IndexOf("credits");
        var txtSheet = creditsData.dataSheets[txtSheetIndex];
        Debug.Log("Found " + txtSheet.AvailableRows.Count + " rows");
        for(int i = 0; i < txtSheet.AvailableRows.Count; i++)
        {
            txtSheet.CurrentRow = txtSheet.GetRowID(i);
            var title = txtSheet.GetString("Section Title");
            var name = txtSheet.GetString("Name");

            if(title != "")
            {
                creditsText.text += "\n<b>" + title + "</b>\n";
                Debug.Log(title + "\n---------\n");
                destY += (yInc * 2f);
            }
            if(name != "")
            {
                creditsText.text += name + "\n";
                Debug.Log(name + "\n");
                destY += yInc;
            }
        }
        isScrolling = true;
    }

    void Update()
    {
        if(isScrolling)
        {
            Vector3 t = creditsText.transform.localPosition;
            creditsText.transform.localPosition = new Vector3(t.x, t.y + (scrollRate * Time.deltaTime), t.z);

            if(creditsText.transform.localPosition.y >= destY)
            {
                creditsText.transform.localPosition = new Vector3(t.x, destY, t.z);
                isScrolling = false;
            }
        }
    }
}