using Rive;
using UnityEngine;

public class RiveBillboard : RiveComponent
{
    public RiveTrigger switchTrigger;
    public RiveImage imageSource00;
    public RiveImage imageSource01;
    public RiveImage imageSource02;
    public RiveImage imageSource03;
    public RiveImage imageSource04;

    protected override bool OobAssetLoaderDelegate(EmbeddedAssetReference assetReference)
    {
        if (assetReference is ImageEmbeddedAssetReference imageEmbeddedAssetReference)
        {
            Debug.Log(imageEmbeddedAssetReference.Name);
            if(imageEmbeddedAssetReference.Name == "imageSource00")
            {
                Debug.Log("connected");
                 imageSource00 = new RiveImage(imageEmbeddedAssetReference, "https://kq.style/etc/campkq01.png");
                 return true;
            }
        }
        return false;
    }
    protected override void OnLoaded()
    {
        base.OnLoaded();
        switchTrigger = new RiveTrigger(viewModel, "switchTrigger");
        /*imageSource00 = new RiveImage(viewModel, "imageSource00", "https://kq.style/etc/campkq01.png");
        imageSource01 = new RiveImage(viewModel, "imageSource01", "https://kq.style/etc/campkq02.png");
        imageSource02 = new RiveImage(viewModel, "imageSource02", "https://kq.style/etc/campkq03.png");
        imageSource03 = new RiveImage(viewModel, "imageSource03", "https://kq.style/etc/campkq04.png");
        imageSource04 = new RiveImage(viewModel, "imageSource04", "https://kq.style/etc/campkq05.png");*/
    }
}