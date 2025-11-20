using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestGameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Button homeButton;
    [SerializeField] string homeSceneName;
    [SerializeField] GridBalloonController BalloonButtonPrefab;
    [SerializeField] RectTransform spawnParent; 
    [SerializeField] int spawnCount = 10;
    [SerializeField] float RefWidth = 1920;
    [SerializeField] float RefHeight = 1080;

    //[Header("UI")]
    //[SerializeField] TextMeshProUGUI ScoreUiText;

    [Header("Spawn Padding")]
    [SerializeField] private float horizontalPadding = 40f;
    [SerializeField] private float bottomOffset = 20f;
    [SerializeField] private float topOffset = 20f;
    public int BurstCount { get; private set; } = 0;
    public static TestGameManager Instance;


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        homeButton.onClick.AddListener(OnHomeClicked);
        Debug.Log(spawnParent.rect.width);
        SpawnBalloonsFirstTime();
    }


    private void SpawnBalloonsFirstTime()
    {
        if (spawnParent == null)
            return;

        float t = Mathf.InverseLerp(550, 3850, spawnParent.rect.width);
        horizontalPadding = Mathf.Lerp(130, 520, t);
        bottomOffset = Mathf.Lerp(150, 500, t);

        float halfWidth = spawnParent.rect.width * 0.5f ;
        float halfHeight = spawnParent.rect.height * 0.5f;


        float nileshCoefficient = 1920 / spawnParent.rect.width;
        float minX = ((0 - halfWidth) + horizontalPadding * nileshCoefficient) * nileshCoefficient;
        float maxX = ((spawnParent.rect.width - halfWidth) - horizontalPadding * nileshCoefficient) * nileshCoefficient;

        float nileshCoefficientY = 1080 / spawnParent.rect.height;

        float minY = ((0 - halfHeight) - bottomOffset* nileshCoefficientY) * nileshCoefficientY;
        float maxY = (minY * 1.5f);

        float topY = ((spawnParent.rect.height - halfHeight) - topOffset / nileshCoefficientY) * nileshCoefficientY;
        if (spawnParent.rect.width < RefWidth)
        {
            nileshCoefficient = spawnParent.rect.width / 1920;
            minX = ((0 - halfWidth) + horizontalPadding * nileshCoefficient) / nileshCoefficient;
            maxX = ((spawnParent.rect.width - halfWidth) - horizontalPadding * nileshCoefficient) / nileshCoefficient;

            nileshCoefficientY =  spawnParent.rect.height / 1080;
            
            minY = ((0 - halfHeight) - bottomOffset* nileshCoefficientY) / nileshCoefficientY;
            maxY = (minY * 1.5f);

            topY = ((spawnParent.rect.height - halfHeight) - topOffset* nileshCoefficientY) / nileshCoefficientY;
        }
        

        float[] xs = new float[spawnCount];
        if (spawnCount == 1)
        {
            xs[0] = (minX + maxX) * 0.5f;
        }
        else
        {
            float step = (maxX - minX) / (spawnCount - 1);
            for (int i = 0; i < spawnCount; i++)
                xs[i] = minX + step * i;
        }


        for (int i = 0; i < spawnCount; i++)
        {
            float spawnX = xs[i];
            spawnX = Mathf.Clamp(spawnX, minX, maxX);
            float spawnY = Random.Range(minY, maxY);

            var balloon = Instantiate(BalloonButtonPrefab, spawnParent, false);

            RectTransform rt = balloon.GetComponent<RectTransform>();

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;

            rt.sizeDelta = new Vector2(200, 210); 

            rt.anchoredPosition = new Vector2(spawnX, spawnY);

            balloon.SetBalloonColor(isTest: true);

            balloon.StartFloating(spawnX, spawnY, topY);
        }
    }

    public void CountUpdater()
    {
        BurstCount++;
        Debug.Log(BurstCount);
        //ScoreUiText.text = "Score - " + Score;
    }
    void OnHomeClicked()
    {

        SceneManager.LoadScene(homeSceneName);
    }
}
