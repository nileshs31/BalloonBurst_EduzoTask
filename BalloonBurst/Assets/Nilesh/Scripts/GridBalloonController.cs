using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum BalloonColor { Green, Red, Pink, Purple}

public class GridBalloonController : MonoBehaviour
{
    [Header("PFX")]
    [SerializeField] GameObject pfxGreen;
    [SerializeField] GameObject pfxRed, pfxPink, pfxPurple;
    [Header("UI")]
    [SerializeField] GameObject imageObject;
    [SerializeField] Sprite[] sprites;
    [SerializeField] public Image spriteImage;
    [SerializeField] TextMeshProUGUI number;
    BalloonColor balloonColor;
    GameObject currentPfx; 
    
    [Header("Floating (Test Mode)")]
    [SerializeField] private float minRiseDuration = 3f;
    [SerializeField] private float maxRiseDuration = 6f;
    [SerializeField] private float minRespawnDelay = 0.6f;
    [SerializeField] private float maxRespawnDelay = 1.8f;
    [SerializeField] private float horizontalAmplitude = 18f;   // how far left/right it waves
    [SerializeField] private float horizontalPeriod = 1.4f;     // how fast the waviness is


    private RectTransform rt;
    private float cachedStartX, cachedBottomY, cachedTopY;
    private Tween horizontalTween;
    private Tween verticalTween;
    private void Awake()
    {

        rt = GetComponent<RectTransform>();
    }
    public void SetBalloonColor(bool isTest = false)
    {
        var k = Random.Range(0, sprites.Length);
        spriteImage.sprite = sprites[k];
        switch (k)
        {
            case 0:
                balloonColor = BalloonColor.Green;
                number.color = Color.green;
                currentPfx = pfxGreen;
                break;
            case 1:
                balloonColor = BalloonColor.Red;
                number.color = Color.red;
                currentPfx = pfxRed;
                break;
            case 2:
                balloonColor = BalloonColor.Pink;
                number.color = new Color(1f, 0.4f, 0.7f);
                currentPfx = pfxPink;
                break;
            case 3:
                balloonColor = BalloonColor.Purple;
                number.color = new Color(0.6f, 0.3f, 0.9f);
                currentPfx = pfxPurple;
                break;
            default:
                break;
        }
        
        imageObject.SetActive(true);

        if (isTest)
        {
            var btn = GetComponent<Button>();
            btn.image = null;
            btn.GetComponent<Image>().enabled = false;
            btn.colors = MakeAlphaColors(btn.colors);
            number.color = new Color(number.color.r, number.color.g, number.color.b, 0f);
            btn.transition = Selectable.Transition.None;


        }
    }

    IEnumerator WaitThenActiveAgain()
    {
        yield return new WaitForSeconds(1.5f);
        number.gameObject.SetActive(false);
        currentPfx.SetActive(false);
        SetBalloonColor();
        yield return new WaitForSeconds(0.25f);
        GetComponent<Button>().interactable = true;
    }
    void AnimateNumberPop()
    {
        number.gameObject.SetActive(true);
        CanvasGroup cg = number.GetComponent<CanvasGroup>();
        if (cg == null) cg = number.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        RectTransform rt = number.rectTransform;
        rt.anchoredPosition = new Vector2(0, 25f);
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOAnchorPosY(-150f, 1f).SetEase(Ease.OutCubic));
        seq.Join(cg.DOFade(0f, 0.75f));
        seq.OnComplete(() =>
        {
            number.gameObject.SetActive(false);
            rt.anchoredPosition = Vector2.zero;
        });
    }

    public void BalloonButtonPressed()
    {
        GetComponent<Button>().interactable = false;
        imageObject.SetActive(false);
        currentPfx.SetActive(true);
        if (PracticeGameManager.Instance != null)
        {
            PracticeGameManager.Instance.ScoreUpdater();
            number.text = "" + PracticeGameManager.Instance.Score;
            AnimateNumberPop();
            StopAllCoroutines();
            StartCoroutine(WaitThenActiveAgain());
        }
        else
        {
            TestGameManager.Instance.CountUpdater();
            StopAllCoroutines();
            //StartCoroutine(WaitThenFlyAgain());
            //handle testscene here later
        }

    }

    public void StartFloating(float spawnX, float bottomY, float topY)
    {
        cachedStartX = spawnX;
        cachedBottomY = bottomY;
        cachedTopY = topY;

        if (rt == null) rt = GetComponent<RectTransform>();

        gameObject.SetActive(true);
        rt.anchoredPosition = new Vector2(spawnX, bottomY);
        rt.localScale = Vector3.one;

        rt.DOKill();
        horizontalTween?.Kill();
        verticalTween?.Kill();

        // Random durations
        float riseDuration = Random.Range(minRiseDuration, maxRiseDuration);
        float horizTarget = spawnX + Random.Range(-horizontalAmplitude, horizontalAmplitude);

        // Horizontal wobble
        horizontalTween = rt.DOAnchorPosX(horizTarget, horizontalPeriod)
                            .SetEase(Ease.InOutSine)
                            .SetLoops(-1, LoopType.Yoyo);

        // Vertical rise to top;
        verticalTween = rt.DOAnchorPosY(topY, riseDuration)
                          .SetEase(Ease.OutSine)
                          .OnComplete(() =>
                          {
                              Debug.Log($"{name} reached top at x={rt.anchoredPosition.x:F1}, y={rt.anchoredPosition.y:F1}");
                              // stop horizontal movement and recycle
                              horizontalTween?.Kill();
                              //OnReachedTop();
                          });
    }

    //color helper - nilesh
    private ColorBlock MakeAlphaColors(ColorBlock original)
    {
        ColorBlock cb = original;
        Color a0 = new Color(1f, 1f, 1f, 0f); // to make the color transparent

        cb.normalColor = a0;
        cb.highlightedColor = a0;
        cb.pressedColor = a0;
        cb.selectedColor = a0;
        cb.disabledColor = a0;

        return cb;
    }

}
