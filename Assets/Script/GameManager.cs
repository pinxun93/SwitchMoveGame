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

    [Header("角色檢測設置")]
    public GameObject player; // 標籤應該是 "Player"
    public Text gameOverText;
    public GameObject gameOverPanel;

    [Header("Debug設置")]
    public bool debugMode = false;

    [Header("恢復遊戲緩衝設置")]
    public float resumeBufferDuration = 2f; // 恢復遊戲後的緩衝時間（秒）

    private bool isPaused = false;
    private bool isGameOver = false;
    private bool shouldCheckDeathOnResume = false; // 標記是否需要在恢復時檢查死亡

    private Vector3 boxInitialSize;
    private Vector3 boxInitialPosition;

    // 簡化的拖動功能
    private bool isDraggingBox = false;
    private Camera cam;

    // 恢復遊戲緩衝機制
    private float resumeBufferTime = 0f;
    private bool isInResumeBuffer = false; // 新增：標記是否在恢復緩衝期

    public bool IsPaused => isPaused;
    public bool IsGameOver => isGameOver;

    private void Start()
    {
        Debug.Log("🎮 [GameManager] GameManager 已啟動！");

        cam = Camera.main;

        // 檢查玩家標籤
        if (player != null)
        {
            if (!player.CompareTag("Player"))
            {
                player.tag = "Player";
                Debug.Log(" [GameManager] 設置 Player 標籤");
            }

            if (player.transform.parent != null)
            {
                player.transform.SetParent(null);
                Debug.Log("[GameManager] Player 已從父物件脫離");
            }
        }
        else
        {
            Debug.LogError(" [GameManager] Player 物件未設置！");
        }

        if (activityBox != null)
        {
            boxInitialSize = activityBox.transform.localScale;
            boxInitialPosition = activityBox.transform.position;
            activityBox.SetActive(true);

            // 確保 activityBox 有 Mask 標籤和碰撞器
            if (!activityBox.CompareTag("Mask"))
            {
                activityBox.tag = "Mask";
                Debug.Log("[GameManager] 設置 ActivityBox 為 Mask 標籤");
            }

            if (activityBox.GetComponent<Collider2D>() == null)
            {
                var collider = activityBox.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                Debug.Log(" [GameManager] 為 ActivityBox 添加了 Collider2D");
            }
        }
        else
        {
            Debug.LogError(" [GameManager] ActivityBox 未設置！");
        }

        activityBoxUIContainer?.SetActive(false);
        gameOverText?.gameObject.SetActive(false);
        gameOverPanel?.SetActive(false);

        Time.timeScale = 1f;
        isGameOver = false;
        resumeBufferTime = 0f;
        isInResumeBuffer = false;
        shouldCheckDeathOnResume = false;
    }

    private void Update()
    {
        // 在遊戲結束時，跳過所有邏輯
        if (isGameOver)
        {
            return;
        }

        // 更新恢復緩衝時間（只有在非暫停狀態下才減少緩衝時間）
        if (resumeBufferTime > 0f && !isPaused)
        {
            resumeBufferTime -= Time.deltaTime;
            isInResumeBuffer = true;

            if (debugMode)
            {
                Debug.Log($" [GameManager] 緩衝期剩餘: {resumeBufferTime:F2}秒");
            }
        }
        else if (isInResumeBuffer && !isPaused)
        {
            // 緩衝期剛結束
            isInResumeBuffer = false;

            // 如果有延遲的死亡檢查，現在執行
            if (shouldCheckDeathOnResume)
            {
                shouldCheckDeathOnResume = false;
                Debug.Log("[GameManager] 緩衝期結束，執行延遲的死亡檢查");

                if (!IsPlayerInActivityBox())
                {
                    ExecuteGameOver();
                    return;
                }
                else
                {
                    Debug.Log("[GameManager] 延遲死亡檢查通過，玩家在安全範圍內");
                }
            }
        }

        // 處理活動框拖動（只在暫停時）
        HandleActivityBoxDragging();

        // 檢查玩家範圍（只在遊戲進行中且不在緩衝期且不暫停）
        if (!isPaused && !isGameOver && !isInResumeBuffer)
        {
            CheckPlayerInBounds();
        }

        // 測試用：按空白鍵手動切換暫停
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(" [GameManager] Space 鍵被按下 - 手動切換暫停");
            TogglePause();
        }
    }

    private void HandleActivityBoxDragging()
    {
        if (!isPaused || activityBox == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = activityBox.transform.position.z;

            // 檢查是否點擊到活動框
            Collider2D boxCollider = activityBox.GetComponent<Collider2D>();
            if (boxCollider != null && boxCollider.OverlapPoint(mouseWorldPos))
            {
                isDraggingBox = true;
                if (debugMode)
                    Debug.Log(" [GameManager] 開始拖動活動框");
            }
        }

        if (Input.GetMouseButton(0) && isDraggingBox)
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = activityBox.transform.position.z;
            activityBox.transform.position = mouseWorldPos;

            // 修復：在拖動活動框時，不要立即檢查死亡，而是標記需要在恢復時檢查
            if (debugMode && !IsPlayerInActivityBox())
            {
                Debug.Log(" [GameManager] 拖動中檢測到玩家在框外，標記延遲檢查");
                shouldCheckDeathOnResume = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDraggingBox)
            {
                if (debugMode)
                    Debug.Log("️ [GameManager] 停止拖動活動框");

                // 修復：拖動結束時，檢查玩家是否在框外，並標記延遲檢查
                if (!IsPlayerInActivityBox())
                {
                    shouldCheckDeathOnResume = true;
                    if (debugMode)
                        Debug.Log(" [GameManager] 拖動結束，玩家在框外，標記延遲死亡檢查");
                }
            }
            isDraggingBox = false;
        }
    }

    // 公開方法：供 FlagTrigger 調用的傳送功能
    public void TeleportPlayerToMask()
    {
        if (player == null || activityBox == null)
        {
            Debug.LogError(" [GameManager] 傳送失敗 - Player 或 ActivityBox 為空");
            return;
        }

        // 修復：不管什麼狀態都不傳送，只有在遊戲進行中才傳送
        if (isPaused || isGameOver)
        {
            if (debugMode)
                Debug.Log($" [GameManager] 傳送條件不符合 - 暫停: {isPaused}, 遊戲結束: {isGameOver}");
            return;
        }

        // 修復：如果正在拖動活動框，也不執行傳送
        if (isDraggingBox)
        {
            if (debugMode)
                Debug.Log(" [GameManager] 正在拖動活動框，不執行傳送");
            return;
        }

        Vector3 maskPosition = activityBox.transform.position;
        player.transform.position = maskPosition;

        // 傳送後清除緩衝時間和死亡標記，立即恢復正常邊界檢查
        resumeBufferTime = 0f;
        isInResumeBuffer = false;
        shouldCheckDeathOnResume = false;

        Debug.Log($" [GameManager] 玩家已傳送到 Mask 位置: {maskPosition}，清除緩衝期和死亡標記");
    }

    private void CheckPlayerInBounds()
    {
        if (player == null || activityBox == null) return;

        // 確保在非暫停狀態下才檢查邊界
        if (isPaused || isGameOver || isInResumeBuffer)
        {
            if (debugMode)
                Debug.Log($" [GameManager] 跳過邊界檢查 - 暫停: {isPaused}, 遊戲結束: {isGameOver}, 緩衝期: {isInResumeBuffer}");
            return;
        }

        // 修復：確保不在拖動狀態下執行邊界檢查
        if (isDraggingBox)
        {
            if (debugMode)
                Debug.Log(" [GameManager] 跳過邊界檢查 - 正在拖動活動框");
            return;
        }

        // 修復：使用 GameOver() 方法而不是直接調用 ExecuteGameOver()
        if (!IsPlayerInActivityBox())
        {
            if (debugMode)
                Debug.Log(" [GameManager] 玩家超出範圍，調用 GameOver()");
            GameOver(); // 這裡改為調用 GameOver() 方法
        }

        if (debugMode && Time.frameCount % 60 == 0) // 每60幀檢查一次
        {
            Vector3 playerPos = player.transform.position;
            Vector3 boxPos = activityBox.transform.position;
            bool inBounds = IsPlayerInActivityBox();
            Debug.Log($"[範圍檢查] 玩家: {playerPos} 活動框: {boxPos} 在範圍內: {inBounds}");
        }
    }

    private bool IsPlayerInActivityBox()
    {
        if (player == null || activityBox == null) return false;

        Vector3 boxPos = activityBox.transform.position;
        Vector3 boxScale = activityBox.transform.localScale;
        Vector3 playerPos = player.transform.position;

        float halfWidth = boxScale.x / 2f;
        float halfHeight = boxScale.y / 2f;

        bool isInX = playerPos.x >= (boxPos.x - halfWidth) && playerPos.x <= (boxPos.x + halfWidth);
        bool isInY = playerPos.y >= (boxPos.y - halfHeight) && playerPos.y <= (boxPos.y + halfHeight);

        return isInX && isInY;
    }

    private void GameOver()
    {
        if (isGameOver) return;

        if (debugMode)
        {
            Debug.Log($" [GameManager] GameOver 被調用 - 暫停: {isPaused}, 緩衝期: {isInResumeBuffer}, 拖動中: {isDraggingBox}");
        }

        // 修復：如果正在拖動活動框，不執行遊戲結束，只標記延遲檢查
        if (isDraggingBox)
        {
            shouldCheckDeathOnResume = true;
            if (debugMode)
                Debug.Log(" [GameManager] 拖動中，僅標記延遲死亡判定，不執行遊戲結束");
            return;
        }

        // 如果遊戲暫停中，標記需要在恢復時檢查死亡，但不立即執行
        if (isPaused)
        {
            shouldCheckDeathOnResume = true;
            if (debugMode)
                Debug.Log(" [GameManager] 遊戲暫停中，僅標記延遲死亡判定，不執行遊戲結束");
            return;
        }

        // 如果在恢復緩衝期中，也標記延遲檢查
        if (isInResumeBuffer)
        {
            shouldCheckDeathOnResume = true;
            if (debugMode)
                Debug.Log(" [GameManager] 緩衝期中，僅標記延遲死亡判定，不執行遊戲結束");
            return;
        }

        // 執行真正的遊戲結束邏輯
        if (debugMode)
            Debug.Log(" [GameManager] 執行遊戲結束邏輯");
        ExecuteGameOver();
    }

    private void ExecuteGameOver()
    {
        if (isGameOver) return;

        // 額外的安全檢查：如果在暫停狀態或拖動狀態，絕對不執行遊戲結束
        if (isPaused || isDraggingBox)
        {
            if (debugMode)
                Debug.Log($" [GameManager] ExecuteGameOver 被阻止：暫停中: {isPaused}, 拖動中: {isDraggingBox}");
            shouldCheckDeathOnResume = true;
            return;
        }

        // 如果在緩衝期，也不執行
        if (isInResumeBuffer)
        {
            if (debugMode)
                Debug.Log(" [GameManager] ExecuteGameOver 被阻止：緩衝期中");
            shouldCheckDeathOnResume = true;
            return;
        }

        isGameOver = true;
        Time.timeScale = 0f;

        // 清除緩衝狀態
        resumeBufferTime = 0f;
        isInResumeBuffer = false;
        shouldCheckDeathOnResume = false;

        if (gameOverText != null)
        {
            gameOverText.text = "You lose!";
            gameOverText.gameObject.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log(" [GameManager] 遊戲結束：角色不在活動範圍！");
    }

    public void RestartGame()
    {
        Debug.Log(" [GameManager] 重新開始遊戲");

        isGameOver = false;
        isPaused = false;
        isDraggingBox = false;
        resumeBufferTime = 0f;
        isInResumeBuffer = false;
        shouldCheckDeathOnResume = false;

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

        bool wasPausedBefore = isPaused;
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (activityBox != null)
            activityBox.SetActive(true);

        if (activityBoxUIContainer != null)
            activityBoxUIContainer.SetActive(isPaused);

        // 如果從暫停恢復到遊戲狀態，設置緩衝時間
        if (wasPausedBefore && !isPaused)
        {
            resumeBufferTime = resumeBufferDuration;
            isInResumeBuffer = true;

            // 修復：停止拖動狀態
            isDraggingBox = false;

            Debug.Log($"[GameManager] 恢復遊戲，設置 {resumeBufferDuration} 秒緩衝期，停止拖動狀態");

            // 如果在暫停期間標記了需要檢查死亡，在緩衝期結束後檢查
            if (shouldCheckDeathOnResume)
            {
                Debug.Log("[GameManager] 標記了延遲死亡檢查，將在緩衝期後執行");
            }
        }

        Debug.Log($" [GameManager] 暫停狀態: {wasPausedBefore} -> {isPaused}");
    }

    public void MoveActivityBox(Vector3 newPosition)
    {
        if (activityBox != null)
        {
            activityBox.transform.position = newPosition;
            if (debugMode)
                Debug.Log($" [GameManager] 活動框移動到: {newPosition}");
        }
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
            if (debugMode)
                Debug.Log($" [GameManager] 活動框縮放到: {clampedScale}");
        }
    }

    // 新增：提供給其他腳本安全觸發遊戲結束檢查的方法
    public void TriggerGameOverCheck()
    {
        if (debugMode)
            Debug.Log(" [GameManager] 外部觸發遊戲結束檢查");

        // 只有在非暫停、非遊戲結束、非緩衝期、非拖動狀態才檢查
        if (!isPaused && !isGameOver && !isInResumeBuffer && !isDraggingBox)
        {
            CheckPlayerInBounds();
        }
        else
        {
            if (debugMode)
                Debug.Log($" [GameManager] 跳過外部觸發的檢查 - 暫停: {isPaused}, 遊戲結束: {isGameOver}, 緩衝期: {isInResumeBuffer}, 拖動中: {isDraggingBox}");
        }
    }

    // 新增：強制設置暫停狀態的方法（用於調試）
    public void ForceSetPauseState(bool pause)
    {
        if (debugMode)
            Debug.Log($" [GameManager] 強制設置暫停狀態: {pause}");

        isPaused = pause;
        Time.timeScale = pause ? 0f : 1f;

        // 修復：強制設置時也要處理拖動狀態
        if (pause)
        {
            isDraggingBox = false;
        }
    }

    // 新增：檢查是否正在拖動活動框的公開屬性
    public bool IsDraggingActivityBox => isDraggingBox;
}