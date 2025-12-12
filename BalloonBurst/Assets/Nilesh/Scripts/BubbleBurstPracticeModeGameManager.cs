using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Eduzo.Games.GameName
{
    public class BubbleBurstPracticeModeGameManager : MonoBehaviour
    {
        [SerializeField] private Button homeButton;
        [SerializeField] private string homeSceneName;
        [SerializeField] BubbleBurstGridBalloonController BalloonButtonPrefab;
        [SerializeField] GridLayoutGroup grid;
        [SerializeField] TextMeshProUGUI ScoreUiText;
        public int Score { get; private set; } = 0;
        public static BubbleBurstPracticeModeGameManager Instance;

        int rows, col;

        // PlayerPrefs keys
        public const string RowsKey = "Rows";
        public const string ColsKey = "Cols";

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
            rows = PlayerPrefs.GetInt(RowsKey);
            col = PlayerPrefs.GetInt(ColsKey);
            ScoreUiText.text = "Score - " + Score;
            ApplyGridSettings();
            InstantiateBallonsFirstTime();
        }

        void ApplyGridSettings()
        {
            rows = PlayerPrefs.GetInt(RowsKey);
            col = PlayerPrefs.GetInt(ColsKey);

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
        private void InstantiateBallonsFirstTime()
        {
            int totalCards = rows * col;

            for (int i = 0; i < totalCards; i++)
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
