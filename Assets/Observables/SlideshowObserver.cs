using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using DG.Tweening;

public class SlideshowObserver : KQObserver
{
    public SpriteRenderer firstSprite;
    public SpriteRenderer secondSprite;
    public SpriteRenderer frame;

    List<Sprite> spriteOptions = new List<Sprite>();
    SpriteRenderer[] srs;
    int curImage = 1;
    int curIndex = 0;
    float nextSwitch = -1f;

    static int switchInterval = 30;

    public static void SetInterval(int newInterval)
    {
        switchInterval = newInterval;
    }

    public override void Start()
    {
        base.Start();
        srs = new SpriteRenderer[2] { firstSprite, secondSprite };
    }

    public override void OnParameters()
    {
        base.OnParameters();

        string baseURL = "https://kq.style/etc/";
        int numImages = 1;
        if (moduleParameters.ContainsKey("baseURL"))
            baseURL = moduleParameters["baseURL"];
        if (moduleParameters.ContainsKey("numImages"))
            numImages = int.Parse(moduleParameters["numImages"]);
        if (moduleParameters.ContainsKey("interval"))
            switchInterval = int.Parse(moduleParameters["interval"]);
        baseURL += "/" + ViewModel.currentTheme.name;

        for(int i = 1; i <= numImages; i++)
        {
            StartCoroutine(AddSlideshowImage(baseURL + Util.AddZeroes(i, 2) + ".png"));
        }
    }

    IEnumerator AddSlideshowImage(string url)
    {
        using (var webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var result = DownloadHandlerTexture.GetContent(webRequest);
                ProcessTexture(result);
            }
        }
    }

    void ProcessTexture(Texture2D texture)
    {
        //var tex = TextureCropTools.CropWithRect(texture, new Rect(0f, 0f, 512f, 316f));
        bool heightLarger = (float)texture.height / (float)texture.width > .35f;
        float largerDim = heightLarger ? texture.height : texture.width;
        //float maxPPU = heightLarger ? 
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * .5f, largerDim * .125f * (heightLarger ? 1.55f : 1f));
        spriteOptions.Add(sprite);
        if (spriteOptions.Count == 1)
            RotateImage();
    }

    void RotateImage()
    {
        curImage = 1 - curImage;
        curIndex = (curIndex + 1 >= spriteOptions.Count ? 0 : curIndex + 1);
        srs[curImage].sprite = spriteOptions[curIndex];
        srs[1 - curImage].DOColor(new Color(1f, 1f, 1f, 0f), 1f);
        srs[curImage].DOColor(Color.white, 1f);
        nextSwitch = switchInterval;
    }

    private void Update()
    {
        if(nextSwitch > 0f)
        {
            nextSwitch -= Time.deltaTime;
            if (nextSwitch <= 0f)
                RotateImage();
        }
    }

    protected override void OnThemeChange()
    {
        base.OnThemeChange();
        frame.gameObject.SetActive(bgContainer.sprite == null);
    }
}

