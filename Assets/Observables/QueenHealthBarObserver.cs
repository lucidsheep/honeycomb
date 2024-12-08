using UnityEngine;
using DG.Tweening;
using System;
public class QueenHealthBarObserver : KQObserver
{
    public SpriteRenderer fill;
    public bool isTrail = false;

    float _progress = 1f;
    float progress {
        get { return _progress;}
        set { _progress = value; SetFill(value);}
    }

    void SetFill(float pct)
    {
		fill.material.SetFloat("_Fill", pct);
    }

    Tweener fillAnim;
    int currentHP = 3;
    int maxHP = 3;

    public override void Start()
    {
        base.Start();

        GameModel.instance.teams[targetID].players[2].curGameStats.deaths.onChange.AddListener(OnQueenDeath);
        GameModel.onGameStart.AddListener(OnGameStart);
        MapDB.currentMap.onChange.AddListener(OnMapChange);
    }

    private void OnGameStart()
    {
        currentHP = maxHP;
        OnHealthDelta(0);
    }

    private void OnMapChange(MapData before, MapData after)
    {
        maxHP = currentHP = after.queen_lives;
        OnHealthDelta(0);
    }

    private void OnQueenDeath(int before, int after)
    {
        if(before >= after) return;
        OnHealthDelta(-1);
    }

    public void OnHealthDelta(int delta)
    {
        bool decreasing = delta < 0;
        currentHP += delta;
        if(currentHP < 0) currentHP = 0;
        float newPct = (float)currentHP / (float)maxHP;
        if(fillAnim != null && !fillAnim.IsComplete())
        {
            if(isTrail)
                fillAnim.Kill();
            else
                fillAnim.Complete();
        }
        fillAnim = DOTween.To(() => progress, x => progress = x, newPct, (decreasing ? (isTrail ? 1.5f : .05f) : 1.5f)).SetEase((decreasing ? Ease.OutQuad : Ease.Linear)).SetDelay(isTrail ? .75f : 0f);
    }

}