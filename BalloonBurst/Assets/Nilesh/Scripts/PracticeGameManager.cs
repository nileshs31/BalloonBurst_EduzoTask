using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PracticeGameManager : MonoBehaviour
{
    [SerializeField] private Button homeButton;
    [SerializeField] private string homeSceneName;
    [SerializeField] GridBalloonController BalloonButtonPrefab;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] TextMeshProUGUI ScoreUiText;
    public int Score { get; private set; } = 0;
    public static PracticeGameManager Instance;
    
    int rows, col;

    // PlayerPrefs keys
    public const string RowsKey = "Rows";
    public const string ColsKey = "Cols";

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

        SceneManager.LoadScene(homeSceneName);
    }
}
