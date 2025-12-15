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
        [SerializeField] GridLayoutGroup grid;
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

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            homeButton.onClick.AddListener(OnHomeClicked);
            homeButton2.onClick.AddListener(OnHomeClicked);
            restartButton.onClick.AddListener(OnRestartClicked);

            int totalBalloons = PlayerPrefs.GetInt("PracticeNumberOfBalloons", 4);
            totalBalloons = RoundToNearestEven(totalBalloons); 
            CalculateRowsAndColumns(totalBalloons);
            //rows = PlayerPrefs.GetInt(RowsKey);
            //col = PlayerPrefs.GetInt(ColsKey);
            ScoreUiText.text = "Score - " + Score;
            ApplyGridSettings();
            InstantiateBallonsFirstTime();
        }
        private int RoundToNearestEven(int value)
        {
            if (value % 2 == 0)
                return value;

            int down = value - 1;
            int up = value + 1;

            if (up <= 36)
                return up;

            return down;
        }

        private void CalculateRowsAndColumns(int desiredTotal)
        {
            int bestRows = 2;
            int bestCols = 2;
            int bestArea = 0;

            // Try all valid grids (2–6 only)
            for (int r = 2; r <= 6; r++)
            {
                for (int c = 2; c <= 6; c++)
                {
                    int area = r * c;

                    if (area > desiredTotal)
                        continue;

                    if (area > bestArea)
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

        void ApplyGridSettings()
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
            grid.constraintCount = col;

            int largest = Mathf.Max(rows, col);
            if (largest > 3)
                grid.cellSize = new Vector2(150f, 160f);
            else
                grid.cellSize = new Vector2(200f, 210f);
        }

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
            if (practiceCompletePanel != null)
                practiceCompletePanel.SetActive(true);
        }
        private void InstantiateBallonsFirstTime()
        {
            TotalBalloons = rows * col;
            poppedCount = 0;

            for (int i = 0; i < TotalBalloons; i++)
            {
                var button = Instantiate(BalloonButtonPrefab, grid.transform);
                button.SetBalloonColor();
            }
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
