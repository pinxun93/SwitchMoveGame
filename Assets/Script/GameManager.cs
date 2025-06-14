using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("活動框設置")]
    public GameObject activityBox;
    public GameObject activityBoxUIContainer;
    public float minBoxSize = 1f;
    public float maxBoxSize = 10f;
    public RectTransform maskSprite; // UI上的遮罩 sprite

    [Header("角色檢測設置")]
    public GameObject player;
    public Text gameOverText;
    public GameObject gameOverPanel;

    [Header("Debug設置")]
    public bool debugMode = false;

    private bool isPaused = false;
    private bool isGameOver = false;
    private bool wasPaused = false;

    private Vector3 boxInitialSize;
    private Vector3 boxInitialPosition;

    public bool IsPaused => isPaused;
    public bool IsGameOver => isGameOver;

    private void Start()
    {
        if (player != null && player.transform.parent != null)
        {
            player.transform.SetParent(null);
            if (debugMode)
                Debug.Log("Player 已從父物件脫離");
        }

        if (activityBox != null)
        {
            boxInitialSize = activityBox.transform.localScale;
            boxInitialPosition = activityBox.transform.position;
            activityBox.SetActive(true);
        }

        activityBoxUIContainer?.SetActive(false);
        gameOverText?.gameObject.SetActive(false);
        gameOverPanel?.SetActive(false);

        Time.timeScale = 1f;
        isGameOver = false;
    }

    private void Update()
    {
        if (!isPaused && wasPaused)
        {
            CheckPlayerInBounds();
        }

        wasPaused = isPaused;
    }

    private void CheckPlayerInBounds()
    {
        if (player == null || activityBox == null) return;

        Vector3 boxPos = activityBox.transform.position;
        Vector3 boxScale = activityBox.transform.localScale;
        Vector3 playerPos = player.transform.position;

        float halfWidth = boxScale.x / 2f;
        float halfHeight = boxScale.y / 2f;

        bool isInX = playerPos.x >= (boxPos.x - halfWidth) && playerPos.x <= (boxPos.x + halfWidth);
        bool isInY = playerPos.y >= (boxPos.y - halfHeight) && playerPos.y <= (boxPos.y + halfHeight);
        bool isPlayerInBounds = isInX && isInY;

        if (!isPlayerInBounds)
        {
            GameOver();
        }

        if (debugMode)
        {
            Debug.Log($"[範圍檢查] 玩家: {playerPos} 活動框中心: {boxPos} 範圍X: {boxPos.x - halfWidth}~{boxPos.x + halfWidth} 範圍Y: {boxPos.y - halfHeight}~{boxPos.y + halfHeight} 結果: {isPlayerInBounds}");
        }
    }

    private void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverText != null)
        {
            gameOverText.text = "You lose!";
            gameOverText.gameObject.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (debugMode)
        {
            Debug.Log("❌ 遊戲結束：角色不在活動範圍！");
        }
    }

    public void RestartGame()
    {
        if (debugMode)
        {
            Debug.Log("Restart 被觸發");
        }

        isGameOver = false;
        isPaused = false;

        gameOverText?.gameObject.SetActive(false);
        gameOverPanel?.SetActive(false);

        if (activityBox != null)
        {
            activityBox.transform.localScale = boxInitialSize;
            activityBox.transform.position = boxInitialPosition;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (activityBox != null)
            activityBox.SetActive(true);

        if (activityBoxUIContainer != null)
            activityBoxUIContainer.SetActive(isPaused);

        if (debugMode)
            Debug.Log(isPaused ? "🟡 遊戲暫停中" : "🟢 遊戲恢復中");
    }

    public void MoveActivityBox(Vector3 newPosition)
    {
        if (activityBox != null)
        {
            activityBox.transform.position = newPosition;
        }

        if (maskSprite != null)
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(newPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                maskSprite.parent as RectTransform,
                screenPoint,
                null,
                out Vector2 localPoint
            );
            maskSprite.anchoredPosition = localPoint;
        }

        if (debugMode)
            Debug.Log("[移動] 活動框與 maskSprite 到: " + newPosition);
    }

    public void ResizeActivityBox(Vector3 newScale)
    {
        float clampedX = Mathf.Clamp(newScale.x, minBoxSize, maxBoxSize);
        float clampedY = Mathf.Clamp(newScale.y, minBoxSize, maxBoxSize);
        float clampedZ = Mathf.Clamp(newScale.z, minBoxSize, maxBoxSize);
        Vector3 clampedScale = new Vector3(clampedX, clampedY, clampedZ);

        if (activityBox != null)
        {
            activityBox.transform.localScale = clampedScale;
        }

        if (maskSprite != null)
        {
            maskSprite.sizeDelta = new Vector2(clampedScale.x * 100f, clampedScale.y * 100f); // 調整倍數視 UI 寬高比而定
        }

        if (debugMode)
            Debug.Log("[縮放] 活動框與 maskSprite: " + clampedScale);
    }
}

