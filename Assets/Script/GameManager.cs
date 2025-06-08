using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("活動框設置")]
    public GameObject activityBox;
    public GameObject activityBoxUIContainer; // 包含X、Y軸滑塊的UI容器
    public float minBoxSize = 1f;
    public float maxBoxSize = 10f;

    [Header("角色檢測設置")]
    public GameObject player; // 玩家角色
    public Text gameOverText; // 顯示"You lose!"的UI文字
    public GameObject gameOverPanel; // 遊戲結束面板（可選）

    [Header("Debug設置")]
    public bool debugMode = false;

    private bool isPaused = false;
    private bool isGameOver = false;
    private Vector3 boxInitialSize;
    private Vector3 boxInitialPosition;

    private bool wasPaused = false;

    public bool IsPaused { get { return isPaused; } }
    public bool IsGameOver { get { return isGameOver; } }

    private void Start()
    {
        // 斷開 Player 的父物件（防止跟著 Mask 移動）
        if (player != null && player.transform.parent != null)
        {
            player.transform.SetParent(null);
            if (debugMode)
                Debug.Log("Player 已從父物件脫離，避免被 Mask 移動影響");
        }

        // 保存活動框的初始設置
        if (activityBox != null)
        {
            boxInitialSize = activityBox.transform.localScale;
            boxInitialPosition = activityBox.transform.position;
            activityBox.SetActive(true);
        }

        // 隱藏UI容器和遊戲結束文字
        if (activityBoxUIContainer != null)
            activityBoxUIContainer.SetActive(false);

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // 啟動時間
        Time.timeScale = 1f;
        isGameOver = false;
    }

    private void Update()
    {
        if (!isPaused && !isGameOver)
        {
            CheckPlayerInBounds();
        }

        wasPaused = IsPaused;
    }

    // 以活動框的位置和大小判斷玩家是否在範圍內
    private void CheckPlayerInBounds()
    {
        if (player == null || activityBox == null)
            return;

        Vector3 boxPos = activityBox.transform.position;
        Vector3 boxScale = activityBox.transform.localScale;

        // 假設活動框的中心為 boxPos，尺寸為 boxScale (假設是正方形/長方形)
        // 判斷玩家位置是否在活動框的X與Y區間內

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
            Debug.Log($"玩家位置: {playerPos}, 活動框中心: {boxPos}, 範圍X: [{boxPos.x - halfWidth}, {boxPos.x + halfWidth}], 範圍Y: [{boxPos.y - halfHeight}, {boxPos.y + halfHeight}], 在範圍內: {isPlayerInBounds}");
        }
    }

    private void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        // 暫停遊戲
        Time.timeScale = 0f;

        // 顯示遊戲結束訊息
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
            Debug.Log("遊戲結束：角色離開了活動範圍！");
        }
    }

    public void RestartGame()
    {
        if (debugMode)
        {
            Debug.Log("Restart 按下了！");
        }

        isGameOver = false;
        isPaused = false;

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (activityBox != null)
        {
            activityBox.transform.localScale = boxInitialSize;
            activityBox.transform.position = boxInitialPosition;
        }

        Time.timeScale = 1f;

        if (debugMode)
        {
            Debug.Log("遊戲已重新開始");
        }

        // 重新載入場景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f;

            if (activityBox != null)
                activityBox.SetActive(true);

            if (activityBoxUIContainer != null)
                activityBoxUIContainer.SetActive(true);

            if (debugMode)
                Debug.Log("遊戲已暫停");
        }
        else
        {
            Time.timeScale = 1f;

            if (activityBox != null)
                activityBox.SetActive(true);

            if (activityBoxUIContainer != null)
                activityBoxUIContainer.SetActive(false);

            if (debugMode)
                Debug.Log("遊戲已繼續");
        }
    }

    public void MoveActivityBox(Vector3 newPosition)
    {
        if (activityBox != null && isPaused)
        {
            activityBox.transform.position = newPosition;

            if (debugMode)
                Debug.Log($"活動框移動到: {newPosition}");
        }
    }

    public void ResizeActivityBox(Vector3 newScale)
    {
        if (activityBox != null && isPaused)
        {
            float clampedX = Mathf.Clamp(newScale.x, minBoxSize, maxBoxSize);
            float clampedY = Mathf.Clamp(newScale.y, minBoxSize, maxBoxSize);
            float clampedZ = Mathf.Clamp(newScale.z, minBoxSize, maxBoxSize);

            Vector3 clampedScale = new Vector3(clampedX, clampedY, clampedZ);
            activityBox.transform.localScale = clampedScale;

            if (debugMode)
                Debug.Log($"活動框縮放到: {clampedScale}");
        }
    }
}
