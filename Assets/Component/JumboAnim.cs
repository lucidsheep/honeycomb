using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

public class JumboAnim : MonoBehaviour
{
	public int priority = 0;
	public float animTime = 1;
	public int sideID = 0;

	public SpriteRenderer bg;
	public TextMeshPro titleTxt;
	public TextMeshPro subtitleTxt;

	protected GlobalFade alpha;
	// Use this for initialization
	void Awake()
	{
		alpha = GetComponent<GlobalFade>();
	}

	// Update is called once per frame
	void Update()
	{
			
	}

	public virtual void StartAnim()
    {
		var actualAnimTime = animTime - .6f;
		transform.localPosition = new Vector3((sideID == 0 ? -6f : 6f), -6.8f, 0f);
		DOTween.Sequence()
			.Append(transform.DOLocalMoveX(sideID == 0 ? -13f : 13f, .25f).SetEase(Ease.Linear))
			.Join(DOTween.To(() => alpha.alpha, x => alpha.alpha = x, 0f, .25f).From())
			.AppendInterval(actualAnimTime)
			.Append(DOTween.To(() => alpha.alpha, x => alpha.alpha = x, 0f, .25f))
			.Join(transform.DOLocalMoveX(sideID == 0 ? -6f : 6f, .25f).SetEase(Ease.Linear));
			//.AppendCallback(() => Destroy(this.gameObject));

	}
}

