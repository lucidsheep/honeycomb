using UnityEngine;
using System.Collections;
using DG.Tweening;

public class MapSkinObserver : KQObserver
{
    public SpriteRenderer sprite;

    bool skinsEnabled = true;

    public override void Start()
    {
        base.Start();
        GameModel.onGameEvent.AddListener(OnGameEvent);
        LSConsole.AddCommandHook("setMapSkins", "Set custom map skins to [on/off]", SetSkins);
    }

    protected override void OnThemeChange()
    {
        base.OnThemeChange();

    }

    string SetSkins(string[] args)
    {
        if (args.Length == 0) skinsEnabled = !skinsEnabled;
        else
        {
            var command = args[0].ToLower();
            if (command == "true" || command == "enabled" || command == "on")
                skinsEnabled = true;
            else
                skinsEnabled = false;
        }
        SetVisible(GameModel.instance.gameIsRunning);

        return "";
    }
    void OnGameEvent(string eventType, GameEventData data)
    {
        switch(eventType)
        {
            case GameEventType.GAME_START:
                SetVisible(true);
                break;
            case GameEventType.GAME_END_DETAIL:
                SetVisible(false);
                break;
        }
    }

    void SetVisible(bool vis)
    {
        sprite.enabled = vis;
        sprite.DOColor(vis ? Color.white : new Color(1f, 1f, 1f, 0f), 1f);

        if(vis)
        {
            string mapName = "mapSkin_" + MapDB.currentMap.property.display_name.ToLower();
            sprite.sprite = AppLoader.GetStreamingSprite(mapName);
        }
    }
}

