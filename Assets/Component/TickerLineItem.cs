using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class TickerThemeData
{
    public string type;
    public string message;
    public string headerColor;
    public string cabinet;
    public string scene;
}
public class TickerLineItem : MonoBehaviour
{

    GlobalFade fader;


    virtual public void StartLine()
    {
        fader = GetComponent<GlobalFade>();
        fader.alpha = 0f;

        DOTween.To(() => fader.alpha, x => fader.alpha = x, 1f, .5f);
        transform.DOMoveX(-2f, .5f).From();
    }
    virtual public void EndLine()
    {
        DOTween.To(() => fader.alpha, x => fader.alpha = x, 0f, .5f);
        transform.DOMoveX(2f, .5f).OnComplete(() => Destroy(this.gameObject));
    }

}

