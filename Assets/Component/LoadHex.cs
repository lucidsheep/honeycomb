using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LoadHex : MonoBehaviour
{
	public GameObject outHex;
	public SpriteRenderer inHex;
	public Color[] colorCycle;
	Tweener colorAnim1, colorAnim2;
	Sequence rotSeq;

	Color top, bottom;
	Color topColor { get { return top; } set { top = value; inHex.material.SetColor("_FillColor", top); } }
	Color botColor { get { return bottom; } set { bottom = value; inHex.material.SetColor("_BlankColor", bottom); } }

	int topIndex = 0;
	int botIndex = 1;

	// Use this for initialization
	void Start()
	{
		topColor = colorCycle[0];
		botColor = colorCycle[1];

		NextColor();

		rotSeq = DOTween.Sequence().Append(outHex.transform.DORotate(new Vector3(0f, 0f,outHex.transform.localRotation.eulerAngles.z - 60f), 0.4f).SetEase(Ease.InBack)).AppendInterval(.6f).SetLoops(-1);
	}

	// Update is called once per frame
	void Update()
	{
			
	}

	void NextColor()
    {
		topIndex = botIndex;
		botIndex = (botIndex + 1 >= colorCycle.Length ? 0 : botIndex + 1);
		colorAnim1 = DOTween.To(() => topColor, x => topColor = x, colorCycle[topIndex], 1f).SetEase(Ease.Linear);
		colorAnim2 = DOTween.To(() => botColor, x => botColor = x, colorCycle[botIndex], 1f).SetEase(Ease.Linear)
			.OnComplete(() => NextColor());
	}

    private void OnDestroy()
    {
		rotSeq.Kill();
		colorAnim1.Kill();
		colorAnim2.Kill();
    }
}

