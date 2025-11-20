using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestGameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Button homeButton;
    [SerializeField] Button homeButton2, restartButton;
    [SerializeField] string homeSceneName;
    [SerializeField] GridBalloonController BalloonButtonPrefab;
    [SerializeField] RectTransform spawnParent; 
    [SerializeField] int spawnCount = 10;


    [Header("Boundary References")]
    [SerializeField] Transform TopYCheck;
    [SerializeField] Transform BottomYCheck;
    [SerializeField] Transform LeftXCheck;
    [SerializeField] Transform RightXCheck;


    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private GameObject gameOverPanel;


    [Header("Test Rules")]
    [SerializeField] private float testDuration = 90f;
    [SerializeField] private int startingLives = 3;

    [Header("Lives UI")]
    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private float lifeLosePulse = 0.25f;

    [Header("Caterpillar")]
    [SerializeField] private Image caterpillarFaceImage;
    [SerializeField] private Image tailImagePrefab;
    [SerializeField] private Transform caterpillarContainer;
    [SerializeField] private Sprite[] tailSpriteVariants;
    [SerializeField] private int minTailCount = 12;
    [SerializeField] private int maxTailCount = 20;
    [SerializeField] private float tailSpacing = 24f;
    [SerializeField] private float tailPopPunch = 0.28f;


    [Header("End UI")]
    [SerializeField] private GameObject winPanel;           
    [SerializeField] private GameObject losePanel;          
    [SerializeField] private TextMeshProUGUI endScoreText;  
    [SerializeField] private Image[] stars;                 
    [SerializeField] private Sprite starFullSprite;
    [SerializeField] private Sprite starEmptySprite;

    // star thresholds (percentage)
    [SerializeField][Range(0, 100)] private int threeStarThreshold = 90;
    [SerializeField][Range(0, 100)] private int twoStarThreshold = 60;
    [SerializeField][Range(0, 100)] private int oneStarThreshold = 30;

    public int BurstCount { get; private set; } = 0; 
    public int MissCount { get; private set; } = 0;  
    private int lives;
    private float timeLeft;
    private bool testRunning = false;
    public const string Cols2Key = "Cols2";

    public static TestGameManager Instance;

    
    private List<Image> caterpillarTails = new List<Image>();
    private int targetTailCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        spawnCount = PlayerPrefs.GetInt(Cols2Key);

        homeButton.onClick.AddListener(OnHomeClicked);
        homeButton2.onClick.AddListener(OnHomeClicked);
        restartButton.onClick.AddListener(OnRestartClicked);
    }

    private void Start()
    {
        StartTest();
    }

    public void StartTest()
    {
        BurstCount = 0;
        MissCount = 0;

        if (hearts != null && hearts.Length > 0)
            lives = Mathf.Min(startingLives, hearts.Length);
        else
            lives = startingLives;

        timeLeft = testDuration;
        testRunning = true; 
       
        UpdateLivesUI();
        SetupCaterpillar();
        SpawnBalloonsFirstTime();
        StartCoroutine(TestTimer());
    }

    private IEnumerator TestTimer()
    {
        while (testRunning && timeLeft > 0f)
        {
            UpdateTimerText();
            yield return null;
            timeLeft -= Time.deltaTime;
        }
        testRunning = false;
        EndTest();
    }

    private void UpdateLivesUI()
    {
        if (hearts == null || hearts.Length == 0) return;
        int heartCount = hearts.Length;

        int clampedLives = Mathf.Clamp(lives, 0, heartCount);

        for (int i = 0; i < heartCount; i++)
        {
            if (hearts[i] == null) continue;

            if (i < clampedLives)
                hearts[i].sprite = fullHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        if (timerSlider != null)
        {
            timerSlider.value = timeLeft / testDuration;
        }
    }

    private int GetAccuracyPercent()
    {
        int total = targetTailCount;
        if (total == 0) return 0;
        float acc = (float)BurstCount / total * 100f;
        return Mathf.RoundToInt(acc);
    }
    
    private void SpawnBalloonsFirstTime()
    {
        if (spawnParent == null || BalloonButtonPrefab == null ||
        TopYCheck == null || BottomYCheck == null || LeftXCheck == null || RightXCheck == null)
        {
            Debug.LogError("Spawn setup incomplete - assign spawnParent, prefab, and the four checks.");
            return;
        }

        Canvas.ForceUpdateCanvases();

        Vector3 topWorld = TopYCheck.position;
        Vector3 bottomWorld = BottomYCheck.position;
        Vector3 leftWorld = LeftXCheck.position;
        Vector3 rightWorld = RightXCheck.position;

        Vector3 topLocal = spawnParent.InverseTransformPoint(topWorld);
        Vector3 bottomLocal = spawnParent.InverseTransformPoint(bottomWorld);
        Vector3 leftLocal = spawnParent.InverseTransformPoint(leftWorld);
        Vector3 rightLocal = spawnParent.InverseTransformPoint(rightWorld);


        float minX = Mathf.Min(leftLocal.x, rightLocal.x);
        float maxX = Mathf.Max(leftLocal.x, rightLocal.x);

        if (maxX <= minX) maxX = minX + 1f;

        float[] xs = new float[spawnCount];
        if (spawnCount == 1)
        {
            xs[0] = (minX + maxX) * 0.5f;
        }
        else
        {
            float step = (maxX - minX) / (spawnCount - 1);
            for (int i = 0; i < spawnCount; i++) xs[i] = minX + step * i;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            float spawnX = xs[i] + Random.Range(-25, 25);
            spawnX = Mathf.Clamp(spawnX, minX, maxX);

            var balloon = Instantiate(BalloonButtonPrefab, spawnParent, false);
            balloon.gameObject.SetActive(true);

            RectTransform rt = balloon.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            if (Mathf.Approximately(rt.rect.width, 0f) || Mathf.Approximately(rt.rect.height, 0f))
                rt.sizeDelta = new Vector2(200f, 210f); 

            rt.localPosition = new Vector3(spawnX, bottomLocal.y, rt.localPosition.z);

            balloon.SetBalloonColor(isTest: true);
            balloon.StartFloating(spawnX, bottomLocal.y, topLocal.y);
        }
    }

    public void CountUpdater()
    {
        BurstCount++;
        PopCaterpillarTail();
    }
    
    public void BalloonMissed()
    {
        MissCount++;
        lives = Mathf.Max(0, lives - 1);  
        UpdateLivesUI();

        if (hearts != null && hearts.Length > 0 && fullHeart != null && emptyHeart != null)
        {
            int idx = Mathf.Clamp(lives, 0, hearts.Length - 1);
            if (idx >= 0 && idx < hearts.Length)
            {
                hearts[idx].transform.DOPunchScale(Vector3.one * 0.2f, lifeLosePulse, 6, 1f);
            }
        }

        if (lives <= 0)
        {
            testRunning = false;
            EndTest();
        }
    }
    
    private void EndTest()
    {
        testRunning = false;
        UpdateLivesUI();

        int acc = GetAccuracyPercent();
        int score = BurstCount;

        bool isWin = acc >= oneStarThreshold; 
                                         
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        gameOverPanel.SetActive(true);

        if (isWin)
        {
            winPanel.SetActive(true);
            winPanel.transform.DOPunchScale(Vector3.one * 0.2f, 0.35f, 6, 1f);
        }
        else
        {
            losePanel.SetActive(true);
            losePanel.transform.DOPunchScale(Vector3.one * 0.2f, 0.35f, 6, 1f);
        }

        if (endScoreText != null)
            endScoreText.text = $"Score: {score}\nAccuracy: {acc}%";

        UpdateStarsByPercent(acc);

        var all = FindObjectsOfType<GridBalloonController>();
        foreach (var b in all)
        {
            b.StopAllCoroutines();
            b.StopAllTweensAndDisable(); 
        }
    }

    void OnHomeClicked()
    {

        SceneManager.LoadScene(homeSceneName);
    }
    void OnRestartClicked()
    {
        SceneManager.LoadScene(2);
    }

    private void SetupCaterpillar()
    {
        foreach (var t in caterpillarTails)
            if (t != null) Destroy(t.gameObject);
        caterpillarTails.Clear();

        if (caterpillarContainer == null || tailImagePrefab == null)
            return;

        targetTailCount = Random.Range(minTailCount, maxTailCount + 1);

        RectTransform prefabRT = tailImagePrefab.GetComponent<RectTransform>();
        float baseWidth = (prefabRT != null && !Mathf.Approximately(prefabRT.rect.width, 0f))
                            ? prefabRT.rect.width
                            : 20f;

        float startX = 50f; 

        for (int i = 0; i < targetTailCount; i++)
        {
            Image tail = Instantiate(tailImagePrefab, caterpillarContainer, false);
            tail.gameObject.SetActive(true);
            tail.name = "Tail_" + i;

            if (tailSpriteVariants != null && tailSpriteVariants.Length > 0)
                tail.sprite = tailSpriteVariants[Random.Range(0, tailSpriteVariants.Length)];

            float x = startX + (i + 1) * tailSpacing;
            tail.rectTransform.anchoredPosition = new Vector2(x, 0f);

            caterpillarTails.Add(tail);
            tail.transform.SetAsFirstSibling();
        }

        RectTransform parentRT = caterpillarContainer.GetComponent<RectTransform>();
        float newWidth = (targetTailCount+1) * tailSpacing;
        parentRT.sizeDelta = new Vector2(newWidth, parentRT.sizeDelta.y);
        caterpillarFaceImage.transform.SetAsLastSibling();

    }

    public void PopCaterpillarTail()
    {
        if (caterpillarTails == null || caterpillarTails.Count == 0)
        {
            return;
        }

        int lastIndex = caterpillarTails.Count - 1;
        Image tail = caterpillarTails[lastIndex];
        caterpillarTails.RemoveAt(lastIndex);

        if (tail == null) return;

        try
        {
            tail.transform.DOPunchScale(Vector3.one * 0.35f, tailPopPunch, 8, 1f)
                .OnComplete(() => { tail.gameObject.SetActive(false); Destroy(tail.gameObject, 0.05f); });
        }
        catch
        {
            tail.gameObject.SetActive(false);
            Destroy(tail.gameObject, 0.05f);
        }

        if (caterpillarTails.Count == 0)
        {
            testRunning = false;
            EndTest();
        }
    }


    private void UpdateStarsByPercent(int percent)
    {
        if (stars == null || stars.Length == 0) return;
        int starCount = 0;
        if (percent >= threeStarThreshold) starCount = 3;
        else if (percent >= twoStarThreshold) starCount = 2;
        else if (percent >= oneStarThreshold) starCount = 1;
        else starCount = 0;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;
            stars[i].sprite = (i < starCount) ? starFullSprite : starEmptySprite;
            if (i < starCount)
            {
                stars[i].transform.DOPunchScale(Vector3.one * 0.2f, 0.35f, 6, 1f); 
            }
        }
    }


}
