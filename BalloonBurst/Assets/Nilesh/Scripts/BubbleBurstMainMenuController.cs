using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Eduzo.Games.BalloonBurst
{
    public class BubbleBurstMainMenuController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] Button practiceButton;
        [SerializeField] Button testButton;
        [SerializeField] Button quitButton;
        [SerializeField] Button addInputField;
        [SerializeField] GameObject loadingScreen;
        [SerializeField] Slider loadingBar;

        [Header("Config")]
        [SerializeField] private string praticeSceneName;
        [SerializeField] private string testSceneName;

        [Header("Loading Settings")]
        [SerializeField] private float targetWidth = 1753f;
        [SerializeField] private float loadDuration = 1.2f;

        private const int MinSize = 4;
        private const int MaxSize = 36;

        [Header("Number of Balloons Settings")]
        [SerializeField] private Transform inputParent;
        [SerializeField] private TMP_InputField inputPrefab;

        [System.Serializable]
        public class BalloonRoundList
        {
            public List<int> balloonCounts = new List<int>();
        }

        private void Awake()
        {
            practiceButton.onClick.AddListener(OnPraticeClicked);
            testButton.onClick.AddListener(OnTestClicked);
            quitButton.onClick.AddListener(OnQuitClicked); 
            addInputField.onClick.AddListener(AddNewInputField);
            inputPrefab.onEndEdit.AddListener(
                s => inputPrefab.text = ClampToRange(s, MinSize, MaxSize).ToString()
            );
        }
        private void AddNewInputField()
        {
            TMP_InputField newInput = Instantiate(inputPrefab, inputParent);
            newInput.text = MinSize.ToString();

            newInput.onEndEdit.AddListener(
                s => newInput.text = ClampToRange(s, MinSize, MaxSize).ToString()
            );

            addInputField.transform.SetAsLastSibling();
        }
        public static void SaveBalloonRounds(List<int> values)
        {
            BalloonRoundList data = new BalloonRoundList
            {
                balloonCounts = values
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("BalloonRounds", json);
            PlayerPrefs.Save();
        }
        private List<int> CollectBalloonRounds()
        {
            List<int> rounds = new List<int>();

            foreach (TMP_InputField input in inputParent.GetComponentsInChildren<TMP_InputField>())
            {
                int value = ClampToRange(input.text, MinSize, MaxSize);
                rounds.Add(value);
            }

            return rounds;
        }
        private void OnPraticeClicked()
        {
            List<int> rounds = CollectBalloonRounds();
            DebugLogRounds(rounds);
            SaveBalloonRounds(rounds);

            StartCoroutine(FillLoadingAndLoadScene(praticeSceneName));
        }

        private void OnTestClicked()
        {
            List<int> rounds = CollectBalloonRounds();
            DebugLogRounds(rounds);
            SaveBalloonRounds(rounds);

            StartCoroutine(FillLoadingAndLoadScene(testSceneName));
        }
        private void DebugLogRounds(List<int> rounds)
        {
            string log = "Balloon Rounds: ";
            for (int i = 0; i < rounds.Count; i++)
            {
                log += rounds[i];
                if (i < rounds.Count - 1) log += ", ";
            }
            Debug.Log(log);
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