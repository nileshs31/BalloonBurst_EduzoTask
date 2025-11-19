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

    public void SetBalloonColor()
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
    }

    public void BalloonButtonPressed()
    {
        GetComponent<Button>().interactable = false;
        imageObject.SetActive(false);
        currentPfx.SetActive(true);
        PracticeGameManager.Instance.ScoreUpdater();
        number.text = ""+PracticeGameManager.Instance.Score; 
        AnimateNumberPop();
        StopAllCoroutines();
        StartCoroutine(WaitThenActiveAgain());

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
        rt.anchoredPosition = new Vector2(0,25f);
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOAnchorPosY(-150f, 1f).SetEase(Ease.OutCubic));
        seq.Join(cg.DOFade(0f, 0.75f));
        seq.OnComplete(() =>
        {
            number.gameObject.SetActive(false);
            rt.anchoredPosition = Vector2.zero;
        });
    }

}
