using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Eduzo.Games.BalloonBurst
{
    public class BubbleBurstPracticeModeGameManager : MonoBehaviour
    {
        [SerializeField] private Button homeButton;
        [SerializeField] Button homeButton2, restartButton;
        [SerializeField] string homeSceneName;
        [SerializeField] private string practiceSceneName;
        [SerializeField] BubbleBurstGridBalloonController BalloonButtonPrefab;
        //[SerializeField] GridLayoutGroup grid;
        [SerializeField] VerticalLayoutGroup verticalParent;
        [SerializeField] TextMeshProUGUI ScoreUiText;
        public int Score { get; private set; } = 0;
        public static BubbleBurstPracticeModeGameManager Instance;

        int rows, col;

        //"PracticeNumberOfBalloons"
        //"TestNumberOfBalloons"

        [SerializeField] private GameObject practiceCompletePanel;

        public int TotalBalloons { get; private set; }
        private int poppedCount = 0;

        //
        [Header("Loading Settings")]
        [SerializeField] GameObject loadingScreen;
        [SerializeField] Slider loadingBar; 
        [SerializeField] private float targetWidth = 1753f;
        [SerializeField] private float loadDuration = 1.2f;

        [SerializeField] private CanvasGroup blackScreen;

        private List<int> practiceRounds = new List<int>();
        private int currentRoundIndex = 0;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            homeButton.onClick.AddListener(OnHomeClicked);
            homeButton2.onClick.AddListener(OnHomeClicked);
            restartButton.onClick.AddListener(OnRestartClicked);

            if (blackScreen != null)
            {
                blackScreen.alpha = 0f;
                blackScreen.gameObject.SetActive(false);
            }

            LoadRounds();
            StartRound();
        }
        private void StartRound()
        {
            TotalBalloons = practiceRounds[currentRoundIndex];

            int rounded = RoundToNearestEven(TotalBalloons);
            CalculateRowsAndColumns(rounded);

            InstantiateBallonsFirstTime();
        }
        /*public void StartGame()
        {
            TotalBalloons = PlayerPrefs.GetInt("PracticeNumberOfBalloons", 4);
            int totalBalloons2 = RoundToNearestEven(TotalBalloons);
            CalculateRowsAndColumns(totalBalloons2);
            //rows = PlayerPrefs.GetInt(RowsKey);
            //col = PlayerPrefs.GetInt(ColsKey);
            //ScoreUiText.text = "Score - " + Score;
            //ApplyGridSettings();
            InstantiateBallonsFirstTime();
        }*/
        private int RoundToNearestEven(int value)
        {
            if (value % 2 == 0)
                return value;

            int up = value + 1;

            return up;
        }

        private void LoadRounds()
        {
            practiceRounds.Clear();

            if (PlayerPrefs.HasKey("BalloonRounds"))
            {
                string json = PlayerPrefs.GetString("BalloonRounds");
                BubbleBurstMainMenuController.BalloonRoundList data =
                    JsonUtility.FromJson<BubbleBurstMainMenuController.BalloonRoundList>(json);

                if (data != null && data.balloonCounts.Count > 0)
                    practiceRounds.AddRange(data.balloonCounts);
            }

            if (practiceRounds.Count == 0)
                practiceRounds.Add(4); // fallback if theres any error

            currentRoundIndex = 0;
        }

        private void CalculateRowsAndColumns(int desiredTotal)
        {
            int bestRows = 2;
            int bestCols = 2;
            int bestArea = int.MaxValue;

            for (int r = 2; r <= 6; r++)
            {
                for (int c = 2; c <= 6; c++)
                {
                    int area = r * c;

                    // must be able to fit all balloons
                    if (area < desiredTotal)
                        continue;

                    // pick the smallest area that fits
                    if (area < bestArea)
                    {
                        bestArea = area;
                        bestRows = r;
                        bestCols = c;
                    }
                }
            }

            rows = bestRows;
            col = bestCols;
        }

        /*void ApplyGridSettings()
        {
            //rows = PlayerPrefs.GetInt(RowsKey);
            //col = PlayerPrefs.GetInt(ColsKey);

            if ((rows * col) % 2 != 0)
            {
                if (col > rows)
                    col = Mathf.Max(1, col - 1);
                else
                    rows = Mathf.Max(1, rows - 1);
            }

            if (grid == null) return;

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 1;

            int largest = Mathf.Max(rows, col);
            if (largest > 3)
                grid.cellSize = new Vector2(150f, 160f);
            else
                grid.cellSize = new Vector2(200f, 210f);
        }*/

        public void OnBalloonPopped()
        {
            poppedCount++;

            if (poppedCount >= TotalBalloons)
            {
                OnPracticeCompleted();
            }
        }

        private void OnPracticeCompleted()
        {
            bool hasNextRound = currentRoundIndex < practiceRounds.Count - 1;

            if (hasNextRound)
            {
                StartCoroutine(RoundTransition());
            }
            else
            {
                if (practiceCompletePanel != null)
                    practiceCompletePanel.SetActive(true);
            }
        }

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

                yield return new WaitForSeconds(0.4f);
            }

            currentRoundIndex++;


            // Cleanup old balloons
            foreach (Transform child in verticalParent.transform)
            {
                if (child.name != "Vines")
                    Destroy(child.gameObject);
            }
            Score = 0;
            StartRound();

            if (blackScreen != null)
            {
                yield return blackScreen
                    .DOFade(0f, 0.5f)
                    .SetEase(Ease.InQuad)
                    .WaitForCompletion();

                blackScreen.gameObject.SetActive(false);
            }

        }



        /*private void InstantiateBallonsFirstTime()
        {
            poppedCount = 0;
            Debug.Log(rows + " " + col);
            for (int i = 0; i < TotalBalloons; i++)
            {
                var button = Instantiate(BalloonButtonPrefab, grid.transform);
                button.SetBalloonColor();
            }
        }*/

        private void InstantiateBallonsFirstTime()
        {
            foreach (Transform child in verticalParent.transform)
            {   
                if(child.name != "Vines")
                    Destroy(child.gameObject);
            }

            poppedCount = 0;

            int spawned = 0;
            int largest = Mathf.Max(rows, col);
            
            for (int r = 0; r < rows && spawned < TotalBalloons; r++)
            {
                // Create a row container
                GameObject rowGO = new GameObject($"Row_{r}", typeof(RectTransform));
                rowGO.transform.SetParent(verticalParent.transform, false);

                HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleCenter;
                //hlg.spacing = grid.spacing.x;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                hlg.spacing = 10;
                ContentSizeFitter fitter = rowGO.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // Fill the row
                for (int c = 0; c < col && spawned < TotalBalloons; c++)
                {
                    var button = Instantiate(BalloonButtonPrefab, rowGO.transform);
                    button.SetBalloonColor();

                    RectTransform rt = button.GetComponent<RectTransform>();
                    LayoutElement le = button.GetComponent<LayoutElement>();
                    if (le == null) le = button.gameObject.AddComponent<LayoutElement>();

                    if (largest > 3)
                    {
                        le.preferredWidth = 150f;
                        le.preferredHeight = 160f;
                    }
                    else
                    {
                        le.preferredWidth = 200f;
                        le.preferredHeight = 210f;
                    }
                    spawned++;
                }
            }
            Canvas.ForceUpdateCanvases();

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                verticalParent.GetComponent<RectTransform>()
            );
        }


        public void ScoreUpdater()
        {
            Score++;
            ScoreUiText.text = "Score - " + Score;
        }


        void Update()
        {

        }
        void OnHomeClicked()
        {
            StartCoroutine(FillLoadingAndLoadScene(homeSceneName));
        }

        void OnRestartClicked()
        {
            StartCoroutine(FillLoadingAndLoadScene(practiceSceneName));
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
    }
}
