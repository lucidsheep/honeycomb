using UnityEngine;
using System.Collections;
using DG.Tweening;

public class MapSkinObserver : KQObserver
{
    public SpriteRenderer sprite;

    public override void Start()
    {
        base.Start();
        GameModel.onGameEvent.AddListener(OnGameEvent);
    }

    protected override void OnThemeChange()
    {
        base.OnThemeChange();

    }

    void OnGameEvent(string eventType, GameEventData data)
    {
        switch(eventType)
        {
            case GameEventType.GAME_START:
                new LSTimer(8.8f, () => SetVisible(true));
                break;
            case GameEventType.GAME_END_DETAIL:
                SetVisible(false);
                break;
        }
    }

    void SetVisible(bool vis)
    {
        sprite.DOColor(vis ? Color.white : new Color(1f, 1f, 1f, 0f), 1f);

        if(vis)
        {
            string mapName = "mapSkin_" + MapDB.currentMap.property.display_name.ToLower();
            sprite.sprite = AppLoader.GetStreamingSprite(mapName);
        }
    }
}

