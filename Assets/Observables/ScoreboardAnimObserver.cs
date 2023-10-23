using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ScoreboardAnimObserver : KQObserver
{
	public enum AnimStyle { SWEEP, BLINK, NONE }
	ScoreboardObserver observer;
	public GameObject[] anims;
	public GameObject spawnArea;
	public AnimStyle animStyle = AnimStyle.SWEEP;
	public float animX = 0f;
	public float animScale = 1f;
	public float queenHeight = .31f;
	public float snailHeight = 0f;
	public float berryHeight = -.8f;
	// Use this for initialization
	void Start()
	{
		observer = GetComponent<ScoreboardObserver>();
		targetID = observer.targetID;
		observer.berryCount.onChange.AddListener((b, a) => { if (a > b) DoAnim(1); });
		//observer.queenCount.onChange.AddListener((b, a) => { if (a > b) DoAnim(-1); });
		observer.snailCount.onChange.AddListener((b, a) => { if (a > b && a % 10 == 0) DoAnim(0); });
	}

	void DoAnim(int pos)
    {
		if (animStyle == AnimStyle.NONE) return;

		var yPos = pos == -1 ? queenHeight : pos == 1 ? berryHeight : snailHeight;
		var anim = Instantiate(anims[team], spawnArea.transform);
		anim.transform.localPosition = new Vector3(targetID == 0 ? animX : -animX, yPos, 0f);
		anim.transform.localScale = Vector3.one * animScale;

		if (animStyle == AnimStyle.SWEEP)
		{
			anim.transform.localRotation = Quaternion.Euler(0f, 0f, targetID == 0 ? 90f : 270f);
			anim.transform.DOLocalMoveX(.72f * (targetID == 0 ? -4f : 4f), .5f).SetEase(Ease.OutQuad)
				.OnComplete(() => Destroy(anim.gameObject));
		} else if(animStyle == AnimStyle.BLINK)
        {
			var sr = anim.GetComponent<SpriteRenderer>();
			sr.color = new Color(1f, 1f, 1f, 0f);
			sr.DOColor(Color.white, 1f);
			DOTween.Sequence()
				.Append(sr.DOColor(Color.white, .1f).SetEase(Ease.InQuad))
				.Append(sr.DOColor(new Color(1f, 1f, 1f, 0f), .1f).SetEase(Ease.InQuad))
				.Append(sr.DOColor(Color.white, .1f).SetEase(Ease.InQuad))
				.AppendInterval(.3f)
				.Append(sr.DOColor(new Color(1f,1f,1f,0f), .5f).SetEase(Ease.InQuad))
				.AppendCallback(() => Destroy(anim));
        }

    }
	// Update is called once per frame
	void Update()
	{
			
	}
}

