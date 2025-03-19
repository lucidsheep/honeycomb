using UnityEngine;

public class StaticImageObserver : KQObserver
{
    string imageName;
    public SpriteRenderer sprite;

    public override void Start()
    {
        base.Start();

        if(sprite == null)
            sprite = bgContainer;
        
    }
    public override void OnParameters()
    {
        base.OnParameters();

        if(moduleParameters.ContainsKey("imageName")){
            imageName = moduleParameters["imageName"];
            SetSprite();
        }
    }
    protected override void OnThemeChange()
    {
        base.OnThemeChange();

        SetSprite();
    }

    void SetSprite()
    {
        sprite.sprite = AppLoader.GetStreamingSprite(imageName);
    }
}