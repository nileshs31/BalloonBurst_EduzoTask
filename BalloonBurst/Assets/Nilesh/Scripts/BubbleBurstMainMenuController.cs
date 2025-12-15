using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;


namespace Eduzo.Games.BalloonBurst
{
    public class BubbleBurstMainMenuController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] TMP_InputField rowsInput;
        [SerializeField] TMP_InputField colsInput;
        [SerializeField] TMP_InputField cols2Input;
        [SerializeField] Button practiceButton;
        [SerializeField] Button testButton;
        [SerializeField] Button quitButton;
        [SerializeField] GameObject loadingScreen;
        [SerializeField] Slider loadingBar;

        [Header("Config")]
        [SerializeField] private string praticeSceneName;
        [SerializeField] private string testSceneName;

        [Header("Loading Settings")]
        //[SerializeField]
        [SerializeField] private float targetWidth = 1753f;
        [SerializeField] private float loadDuration = 1.2f;

        private const int MinSize = 4;
        private const int MaxSize = 36;

        private const int Min2Size = 5;
        private const int Max2Size = 22;

        private const int DefaultRows = 2;
        
        //private const int DefaultCols = 2;

        private const int Default2Cols = 5;

        //"PracticeNumberOfBalloons"
        //"TestNumberOfBalloons"


        private void Awake()
        {

            int rows = Mathf.Clamp(PlayerPrefs.GetInt("PracticeNumberOfBalloons", DefaultRows), MinSize, MaxSize);
            //int cols = Mathf.Clamp(PlayerPrefs.GetInt(ColsKey, DefaultCols), MinSize, MaxSize);

            int cols2 = Mathf.Clamp(PlayerPrefs.GetInt("TestNumberOfBalloons", Default2Cols), Min2Size, Max2Size);

            rowsInput.text = rows.ToString();

            cols2Input.text = cols2.ToString();

            rowsInput.onEndEdit.AddListener(s => rowsInput.text = ClampToRange(s, MinSize, MaxSize).ToString());
            //colsInput.onEndEdit.AddListener(s => colsInput.text = ClampToRange(s, MinSize, MaxSize).ToString());
            cols2Input.onEndEdit.AddListener(s => cols2Input.text = ClampToRange(s, Min2Size, Max2Size).ToString());

            practiceButton.onClick.AddListener(OnPraticeClicked);
            testButton.onClick.AddListener(OnTestClicked);
            quitButton.onClick.AddListener(OnQuitClicked);

        }

        private void OnPraticeClicked()
        {
            int rows = ClampToRange(rowsInput.text, MinSize, MaxSize);
           // int cols = ClampToRange(colsInput.text, MinSize, MaxSize);

            PlayerPrefs.SetInt("PracticeNumberOfBalloons", rows);
            PlayerPrefs.Save();

            StartCoroutine(FillLoadingAndLoadScene(praticeSceneName));
        }

        private void OnTestClicked()
        {
            int cols = ClampToRange(cols2Input.text, Min2Size, Max2Size);
            PlayerPrefs.SetInt("TestNumberOfBalloons", cols);
            PlayerPrefs.Save();

            StartCoroutine(FillLoadingAndLoadScene(testSceneName));
        }

        private void OnQuitClicked()
        {
            Application.Quit();
        }

        private static int ClampToRange(string s, int min, int max)
        {
            if (!int.TryParse(KeepDigits(s), out int v)) v = min;
            return Mathf.Clamp(v, min, max);
        }

        private static string KeepDigits(string s)
        {
            if (string.IsNullOrEmpty(s)) return "0";
            System.Text.StringBuilder sb = new System.Text.StringBuilder(s.Length);
            foreach (char c in s)
                if (char.IsDigit(c)) sb.Append(c);
            return sb.Length == 0 ? "0" : sb.ToString();
        }

        private IEnumerator FillLoadingAndLoadScene(string sceneName)
        {
            practiceButton.interactable = testButton.interactable = false;
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