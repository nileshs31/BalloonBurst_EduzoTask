using DG.Tweening;
using UnityEngine;

namespace Eduzo.Games.BalloonBurst
{
    public class BubbleBurstPopText : MonoBehaviour
    {
        void Start()
        {
            RectTransform rt = GetComponent<RectTransform>();
            CanvasGroup cg = gameObject.AddComponent<CanvasGroup>();

            float delayTime = 0.5f;
            float animDuration = 0.35f;

            Vector3 startPos = rt.localPosition;
            Vector3 endPos = startPos + Vector3.down * 50f;

            cg.alpha = 1f;
            rt.localScale = Vector3.one * 0.9f;

            Sequence seq = DOTween.Sequence();

            // Scale punch first (visible immediately)
            seq.Append(
                rt.DOPunchScale(Vector3.one * 0.2f, 0.25f, 6, 1f)
            );

            // Hold still
            seq.AppendInterval(delayTime);

            // Move + fade together
            seq.Append(
                rt.DOLocalMove(endPos, animDuration)
                  .SetEase(Ease.OutCubic)
            );

            seq.Join(
                cg.DOFade(0f, animDuration)
            );

            seq.OnComplete(() => Destroy(gameObject));
        }
    }
}