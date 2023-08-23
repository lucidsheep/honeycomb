using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ScoreboardAnimObserver : KQObserver
{
	ScoreboardObserver observer;
	public GameObject[] anims;
	public GameObject spawnArea;
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
		var yPos = pos == -1 ? .31f : pos == 1 ? -.8f : 0f;
		var anim = Instantiate(anims[team], spawnArea.transform);
		anim.transform.localPosition = new Vector3(targetID == 0 ? 0f : -0f, yPos, 0f);
		anim.transform.localScale = new Vector3(1f, 1f, 1f);
		anim.transform.localRotation = Quaternion.Euler(0f, 0f, targetID == 0 ? 90f : 270f);
		anim.transform.DOLocalMoveX(.72f * (targetID == 0 ? -4f : 4f), .5f).SetEase(Ease.OutQuad)
			.OnComplete(() => Destroy(anim.gameObject));

    }
	// Update is called once per frame
	void Update()
	{
			
	}
}

