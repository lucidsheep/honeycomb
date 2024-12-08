using UnityEngine;
using DG.Tweening;
using System.Security.Cryptography.X509Certificates;
public class WarriorPip : MonoBehaviour
{
    public SpriteRenderer bg, fill, anim;

    public bool isFilled {get { return _progress > 0f;}}
    float _progress = 0f;
    float progress {
        get { return _progress;}
        set { _progress = value; SetFill(value);}
    }

    bool completed = false;
    void SetFill(float pct)
    {
		fill.material.SetFloat("_Fill", pct);
    }

    Tweener fillAnim, finishAnim;
    public void StartFill()
    {
        if(isFilled) return;

        _progress = 0.01f;
        if(fillAnim != null && !fillAnim.IsComplete())
            fillAnim.Complete();
        fillAnim = DOTween.To(() => progress, x => progress = x, 1f, 2.5f).SetEase(Ease.Linear);
    }

    public void CompleteFill()
    {
        if(completed) return;
        
        if(fillAnim != null && !fillAnim.IsComplete())
            fillAnim.Complete();
        progress = 1.0f;
        completed = true;
        DoFinishAnim();
    }
    public void RemoveFill()
    {
        if(fillAnim != null && !fillAnim.IsComplete())
            fillAnim.Complete();
        progress = 0f;
        completed = false;
        DoFinishAnim();
    }

    void DoFinishAnim()
    {
        if(finishAnim != null && !finishAnim.IsComplete())
            finishAnim.Complete();
        anim.color = Color.white;
        anim.transform.localScale = Vector3.one;
        anim.transform.DOScale(3f, .5f).SetEase(Ease.OutQuad);
        anim.DOColor(new Color(1f,1f,1f,0f), .5f).SetEase(Ease.OutQuad);
    }
}