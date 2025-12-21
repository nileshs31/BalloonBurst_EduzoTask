using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



namespace Eduzo.Games.BalloonBurst
{
    [System.Serializable]
    public class BubbleBurstTestResult
    {
        public int questionAsked;          // target tail bubbles
        public int userResponse;       // user response = bubbles burst
        public int score;           // percentage score
        public bool isCorrect;          // tailCount == bubblesBurst
        public float timeSpentSeconds; // testDuration - timeLeft
    }

    [System.Serializable]
    public class BubbleBurstTestSessionResult
    {
        public List<BubbleBurstTestResult> rounds = new List<BubbleBurstTestResult>();

        public int totalQuestions;   // sum of all targetTailCount
        public int totalResponses;   // sum of all BurstCount
        public float accuracy;       // derived at end
    }


    public class BubbleBurstTestModeGameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Button homeButton;
        [SerializeField] Button homeButton2, restartButton;
        [SerializeField] string homeSceneName;
        [SerializeField] private string testSceneName;
        [SerializeField] BubbleBurstGridBalloonController BalloonButtonPrefab;
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
        [SerializeField] private int maxTailCount = 20; //22
        [SerializeField] private float tailSpacing = 35f;
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
        public bool testRunning = false;

        public static BubbleBurstTestModeGameManager Instance;


        private List<Image> caterpillarTails = new List<Image>();
        private int targetTailCount = 0;

        [Header("Loading Settings")]
        [SerializeField] GameObject loadingScreen;
        [SerializeField] Slider loadingBar;
        [SerializeField] private float targetWidth = 1753f;
        [SerializeField] private float loadDuration = 1.2f;

        [SerializeField] private GameObject popTextPrefab;

        private List<int> testRounds = new List<int>();
        private int currentRoundIndex = 0;

        public CanvasGroup blackScreen;

        private BubbleBurstTestSessionResult currentSessionResult;
        private bool roundResultSaved = false;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (blackScreen != null)
            {
                blackScreen.alpha = 0f;
                blackScreen.gameObject.SetActive(false);
            }
            LoadRounds();
            targetTailCount = testRounds[currentRoundIndex];
            homeButton.onClick.AddListener(OnHomeClicked);
            homeButton2.onClick.AddListener(OnHomeClicked);
            restartButton.onClick.AddListener(OnRestartClicked);
            currentSessionResult = new BubbleBurstTestSessionResult();

        }
        private void LoadRounds()
        {
            testRounds.Clear();

            if (PlayerPrefs.HasKey("BalloonRounds"))
            {
                string json = PlayerPrefs.GetString("BalloonRounds");
                BubbleBurstMainMenuController.BalloonRoundList data =
                    JsonUtility.FromJson<BubbleBurstMainMenuController.BalloonRoundList>(json);

                if (data != null && data.balloonCounts.Count > 0)
                    testRounds.AddRange(data.balloonCounts);
            }

            // Safety fallback
            if (testRounds.Count == 0)
                testRounds.Add(5);

            currentRoundIndex = 0;
        }

        private void Start()
        {
            StartTest();
        }

        public void StartTest()
        {
            spawnCount = Random.Range(5, 8);
            BurstCount = 0;
            MissCount = 0;
            roundResultSaved = false;
            targetTailCount = testRounds[currentRoundIndex];

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

            if (timeLeft <= 0f && testRunning)
            {
                testRunning = false;
                EndTest();
            }
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

            if(BurstCount == targetTailCount)
            {
                testRunning = false;
                EndTest();
            }
            //PopCaterpillarTail();
        }

        public void SpawnPopText(Vector3 worldPosition)
        {
            GameObject go = Instantiate(popTextPrefab, spawnParent, false);
            RectTransform rt = go.GetComponent<RectTransform>();

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                spawnParent,
                RectTransformUtility.WorldToScreenPoint(null, worldPosition),
                null,
                out localPos
            );

            rt.anchoredPosition = localPos;

            TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = BurstCount.ToString();
            }
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
            SaveTestResults(); // saving the testResults here
            UpdateLivesUI();

            var all = FindObjectsOfType<BubbleBurstGridBalloonController>();
            foreach (var b in all)
            {
                b.StopAllCoroutines();
                b.StopAllTweensAndDisable();
            }

            bool hasNextRound = currentRoundIndex < testRounds.Count - 1;

            if (hasNextRound)
            {
                StartCoroutine(RoundTransition());
            }
            else
            {
                ShowFinalGameOver();
            }

        }
        private void ShowFinalGameOver()
        {
            FinalizeSessionResults();

            int acc = Mathf.RoundToInt(currentSessionResult.accuracy);
            int score = currentSessionResult.totalResponses;
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

            string json = JsonUtility.ToJson(currentSessionResult);
            PlayerPrefs.SetString("LastTestSessionResult", json);
            PlayerPrefs.Save();
        }
        /*private IEnumerator LoadNextRound()
        {
            // optional fade / delay
            yield return new WaitForSeconds(0.8f);

            currentRoundIndex++;

            // cleanup
            foreach (var b in FindObjectsOfType<BubbleBurstGridBalloonController>())
                Destroy(b.gameObject);

            foreach (var t in caterpillarTails)
                if (t) Destroy(t.gameObject);
            caterpillarTails.Clear();

            StartTest();
        }*/

        private IEnumerator RoundTransition()
        {
            if (blackScreen != null)
            {
                blackScreen.gameObject.SetActive(true);
                blackScreen.alpha = 0f;

                yield return blackScreen
                    .DOFade(1f, 0.35f)
                    .SetEase(Ease.OutQuad)
                    .WaitForCompletion();

                yield return new WaitForSeconds(0.5f);
            }

            currentRoundIndex++;

            foreach (var b in FindObjectsOfType<BubbleBurstGridBalloonController>())
                Destroy(b.gameObject);

            foreach (var t in caterpillarTails)
                if (t) Destroy(t.gameObject);
            caterpillarTails.Clear();

            if (blackScreen != null)
            {
                yield return blackScreen
                    .DOFade(0f, 0.5f)
                    .SetEase(Ease.InQuad)
                    .WaitForCompletion();

                blackScreen.gameObject.SetActive(false);
            }

            StartTest();
        }


        public void SaveTestResults()
        {
            if (roundResultSaved)
                return;

            roundResultSaved = true;

            BubbleBurstTestResult result = new BubbleBurstTestResult
            {
                questionAsked = targetTailCount,
                userResponse = BurstCount,
                score = GetAccuracyPercent(),
                isCorrect = (BurstCount == targetTailCount),
                timeSpentSeconds = testDuration - timeLeft
            };

            currentSessionResult.rounds.Add(result);

            Debug.Log($"Saved round {currentSessionResult.rounds.Count} | Asked {targetTailCount}");
        }


        private void FinalizeSessionResults()
        {
            int totalAsked = 0;
            int totalPopped = 0;

            foreach (var r in currentSessionResult.rounds)
            {
                totalAsked += r.questionAsked;
                totalPopped += r.userResponse;
            }

            currentSessionResult.totalQuestions = totalAsked;
            currentSessionResult.totalResponses = totalPopped;

            if (totalAsked > 0)
                currentSessionResult.accuracy = (float)totalPopped / totalAsked * 100f;
            else
                currentSessionResult.accuracy = 0f;
        }

        [ContextMenu("View Last Test Session Result")]
        public void ViewTestResults()
        {
            if (!PlayerPrefs.HasKey("LastTestSessionResult"))
            {
                Debug.Log("No session result found.");
                return;
            }

            string json = PlayerPrefs.GetString("LastTestSessionResult");
            BubbleBurstTestSessionResult session =
                JsonUtility.FromJson<BubbleBurstTestSessionResult>(json);

            Debug.Log($"SESSION RESULT\n" +
                      $"Rounds Played: {session.rounds.Count}\n" +
                      $"Total Asked: {session.totalQuestions}\n" +
                      $"Total Popped: {session.totalResponses}\n" +
                      $"Accuracy: {session.accuracy:0.##}%");

            for (int i = 0; i < session.rounds.Count; i++)
            {
                var r = session.rounds[i];
                Debug.Log(
                    $"Round {i + 1} | Asked: {r.questionAsked}, " +
                    $"Popped: {r.userResponse}, " +
                    $"Acc: {r.score}%, " +
                    $"Correct: {r.isCorrect}, " +
                    $"Time: {r.timeSpentSeconds:0.0}s"
                );
            }
        }


        [ContextMenu("Clear Last Test Result")]
        public void ClearTestResults()
        {
            PlayerPrefs.DeleteKey("LastTestResult");
            PlayerPrefs.Save();
            Debug.Log("Last test result cleared.");
        }

        void OnHomeClicked()
        {
            StartCoroutine(FillLoadingAndLoadScene(homeSceneName));
        }

        private IEnumerator FillLoadingAndLoadScene(string sceneName)
        {
            homeButton.interactable = false;
            if (loadingScreen != null) loadingScreen.SetActive(true);
            loadingBar.value = 0;

            var async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.001f, loadDuration);
                loadingBar.value = (Mathf.Lerp(0f, 1, t));
                yield return null;
            }

            while (async.progress < 0.9f) yield return null;

            yield return new WaitForSecondsRealtime(0.12f);
            async.allowSceneActivation = true;
        }

        void OnRestartClicked()
        {
            StartCoroutine(FillLoadingAndLoadScene(testSceneName));
        }

        private void SetupCaterpillar()
        {
            foreach (var t in caterpillarTails)
                if (t != null) Destroy(t.gameObject);
            caterpillarTails.Clear();

            if (caterpillarContainer == null || tailImagePrefab == null)
                return;

            //targetTailCount = Random.Range(minTailCount, maxTailCount + 1);

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

                tail.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = i + 1 + "";
                

                if (tailSpriteVariants != null && tailSpriteVariants.Length > 0)
                    tail.sprite = tailSpriteVariants[Random.Range(0, tailSpriteVariants.Length)];

                float x = startX + (i + 1) * tailSpacing;
                tail.rectTransform.anchoredPosition = new Vector2(x, 0f);

                caterpillarTails.Add(tail);
                tail.transform.SetAsFirstSibling();
            }

            RectTransform parentRT = caterpillarContainer.GetComponent<RectTransform>();
            float newWidth = (targetTailCount + 1) * tailSpacing;
            parentRT.sizeDelta = new Vector2(newWidth, parentRT.sizeDelta.y);
            caterpillarFaceImage.transform.SetAsLastSibling();
            caterpillarTails[caterpillarTails.Count - 1].transform.GetChild(0).gameObject.SetActive(true);
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
                    .OnComplete(() => { tail.gameObject.SetActive(false); Destroy(tail.gameObject, 0.05f); 
                        if (!caterpillarTails[caterpillarTails.Count - 1].transform.GetChild(0).gameObject.activeSelf)
                        {
                            caterpillarTails[caterpillarTails.Count - 1].transform.GetChild(0).gameObject.SetActive(true);
                        }
                    });

                
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
}
