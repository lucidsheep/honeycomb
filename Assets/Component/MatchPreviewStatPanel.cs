using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class MatchPreviewStatPanel : MonoBehaviour
{
	public TextMeshPro blueStatText, goldStatText, centerText;
	public GameObject leftPanel, rightPanel;

	Sequence sequence;
	float windowWidth = 0f;
	// Use this for initialization
	void Start()
	{
		windowWidth = rightPanel.transform.localPosition.x;
	}

	public void SetDisplay(string center, string blue, string gold)
    {

		sequence = DOTween.Sequence()
			.Append(centerText.DOColor(new Color(1f, 1f, 1f, 0f), 1f))
			.Join(leftPanel.transform.DOLocalMoveX(0f, 1f).SetEase(Ease.InBack))
			.Join(rightPanel.transform.DOLocalMoveX(0f, 1f).SetEase(Ease.InBack))
			.AppendCallback(() =>
			{
				centerText.text = center;
				blueStatText.text = blue;
				goldStatText.text = gold;
			})
			.Append(centerText.DOColor(Color.white, 1f))
			.Join(leftPanel.transform.DOLocalMoveX(-windowWidth, 1f).SetEase(Ease.OutBack))
			.Join(rightPanel.transform.DOLocalMoveX(windowWidth, 1f).SetEase(Ease.OutBack));

	}

	public void HideDisplay()
    {
		if (sequence != null && !sequence.IsComplete())
			sequence.Kill();
    }
}

