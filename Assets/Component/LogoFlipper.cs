using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LogoFlipper : MonoBehaviour
{
    public GameObject scorebug;
    public GameObject verses;
    public float flipTime = .25f;
    public float versesTime = 5f;
    public Ease animEase = Ease.Linear;

    Sequence seq;
    bool animInProgress = false;
    bool scorebugMode = true;

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.instance.gameEventDispatcher.AddListener(OnGameEvent);
    }

    void OnGameEvent(string type, GameEventData data)
    {
        if (animInProgress) return;
        if(type == GameEventType.SPAWN && data.playerID == 2 && data.teamID == 1)
        {
            animInProgress = true;
            DOTween.Sequence()
                .AppendCallback(() => Flip(false))
                .AppendInterval(versesTime + flipTime)
                .AppendCallback(() => Flip(true))
                .AppendInterval(flipTime)
                .AppendCallback(() => animInProgress = false);
            
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void Flip(bool toScorebug)
    {
        if (toScorebug == scorebugMode) return;
        scorebugMode = toScorebug;

        if (seq != null && !seq.IsComplete())
            seq.Complete(true);

        var fromGO = scorebugMode ? verses : scorebug;
        var toGO = scorebugMode ? scorebug : verses;
        
        seq = DOTween.Sequence()
            .Append(fromGO.transform.DORotateQuaternion(Quaternion.Euler(0f, 90f, 0f), flipTime / 2f).SetEase(animEase))
            .AppendCallback(() => { fromGO.SetActive(false); toGO.SetActive(true); })
            .Append(toGO.transform.DORotateQuaternion(Quaternion.Euler(0f, 0f, 0f), flipTime / 2f).SetEase(animEase));
        
    }
}
