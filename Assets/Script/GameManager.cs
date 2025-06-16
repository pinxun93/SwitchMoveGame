using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [Header("活動框設置")]
    public GameObject activityBox;
    public GameObject activityBoxUIContainer;
    public float minBoxSize = 1f;
    public float maxBoxSize = 10f;
    public RectTransform maskSprite;

    [Header("角色檢測設置")]
    public GameObject player;
    public Text gameOverText;
    public GameObject gameOverPanel;

    [Header("Debug設置")]
    public bool debugMode = false;

    private bool isPaused = false;
    private bool isGameOver = false;
    private bool isDraggingActivityBox = false;
    private bool isDraggingMask = false;

    private bool boundsCheckEnabled = true;
    private bool allowBoundsCheck = true;
    private float boundsCheckDelay = 0.2f;
    private float boundsCheckTimer = 0f;

    private float lastPauseTime = 0f;
    private float pauseProtectionDuration = 0.5f;

    private Vector3 boxInitialSize;
    private Vector3 boxInitialPosition;

    public bool IsPaused => isPaused;
    public bool IsGameOver => isGameOver;
    public bool IsDraggingActivityBox => isDraggingActivityBox;

    private void Start()
    {
        if (player != null && player.transform.parent != null)
        {
            player.transform.SetParent(null);
            if (debugMode) Debug.Log("Player 已從父物件脫離");
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

        InitializeGameState();
    }

    private void InitializeGameState()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;
        boundsCheckEnabled = true;
        allowBoundsCheck = true;
        boundsCheckTimer = 0f;
        lastPauseTime = 0f;

        if (debugMode)
            Debug.Log("🟢 [初始化] 遊戲狀態已重置");
    }

    private void Update()
    {
        if (ShouldPerformBoundsCheck())
        {
            if (boundsCheckTimer <= 0f)
            {
                CheckPlayerInBounds();
            }
            else
            {
                boundsCheckTimer -= Time.unscaledDeltaTime;
                if (debugMode && boundsCheckTimer > 0f)
                    Debug.Log($"[延遲檢測] 剩餘時間: {boundsCheckTimer:F2}秒");
            }
        }
        else if (debugMode)
        {
            LogSkipReason();
        }
    }

    private bool ShouldPerformBoundsCheck()
    {
        if (isPaused || isGameOver || isDraggingActivityBox)
            return false;

        if (!boundsCheckEnabled || !allowBoundsCheck)
            return false;

        if (Time.realtimeSinceStartup - lastPauseTime < pauseProtectionDuration)
        {
            if (debugMode)
                Debug.Log("[時間保護] 暫停後保護時間內，跳過檢測");
            return false;
        }

        if (player == null || activityBox == null)
            return false;

        return true;
    }

    private void LogSkipReason()
    {
        string reason = "";
        if (isPaused) reason += "遊戲暫停 ";
        if (isGameOver) reason += "遊戲結束 ";
        if (isDraggingActivityBox) reason += "拖動中 ";
        if (!boundsCheckEnabled) reason += "檢測停用 ";
        if (!allowBoundsCheck) reason += "不允許檢測 ";
        if (Time.realtimeSinceStartup - lastPauseTime < pauseProtectionDuration) reason += "時間保護 ";

        if (!string.IsNullOrEmpty(reason))
        {
            Debug.Log($"[範圍檢測] 跳過檢測，原因: {reason.Trim()}");
        }
    }

    private void CheckPlayerInBounds()
    {
        if (!ShouldPerformBoundsCheck())
        {
            if (debugMode)
                Debug.Log("[範圍檢查] 最終安全檢查失敗，跳過檢測");
            return;
        }

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
            if (ShouldPerformBoundsCheck())
            {
                if (debugMode)
                    Debug.Log("❌ [範圍檢查] 玩家超出範圍，準備觸發遊戲結束");
                GameOver();
            }
            else if (debugMode)
            {
                Debug.Log("[範圍檢查] 玩家超出範圍但遊戲狀態已改變，取消遊戲結束");
            }
        }

        if (debugMode)
        {
            Debug.Log($"[範圍檢查] 玩家: {playerPos} | 活動框: {boxPos} | 範圍X: [{boxPos.x - halfWidth}, {boxPos.x + halfWidth}] | 範圍Y: [{boxPos.y - halfHeight}, {boxPos.y + halfHeight}] | 結果: {isPlayerInBounds}");
        }
    }

    private void GameOver()
    {
        if (isGameOver || isPaused || isDraggingActivityBox)
        {
            if (debugMode)
                Debug.Log("❌ [GameOver] 被阻止：遊戲狀態不允許結束");
            return;
        }

        if (Time.realtimeSinceStartup - lastPauseTime < pauseProtectionDuration)
        {
            if (debugMode)
                Debug.Log("❌ [GameOver] 被阻止：仍在時間保護期內");
            return;
        }

        isGameOver = true;
        StopAllBoundsCheck();
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

    private void StopAllBoundsCheck()
    {
        boundsCheckEnabled = false;
        allowBoundsCheck = false;
        boundsCheckTimer = 0f;

        if (debugMode)
            Debug.Log("🛑 [停止檢測] 所有範圍檢測已停止");
    }

    private void StartBoundsCheck(float delay = 0f)
    {
        if (isPaused || isGameOver)
        {
            if (debugMode)
                Debug.LogWarning("[啟動檢測] 無法啟動：遊戲暫停或已結束");
            return;
        }

        boundsCheckEnabled = true;
        allowBoundsCheck = true;
        boundsCheckTimer = delay;

        if (debugMode)
            Debug.Log($"🟢 [啟動檢測] 範圍檢測已啟動，延遲: {delay} 秒");
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        lastPauseTime = Time.realtimeSinceStartup;

        if (isPaused)
        {
            StopAllBoundsCheck();
            if (debugMode)
                Debug.Log("🟡 遊戲暫停 - 所有範圍檢測已停止");
        }
        else
        {
            // ✅ 回來後立即進行一次檢查
            Invoke(nameof(CheckPlayerInBoundsAfterResume), 0.05f);

            StartBoundsCheck(boundsCheckDelay);
            if (debugMode)
                Debug.Log($"🟢 遊戲恢復 - 將在 {boundsCheckDelay} 秒後開始範圍檢測");
        }

        if (activityBox != null)
            activityBox.SetActive(true);

        if (activityBoxUIContainer != null)
            activityBoxUIContainer.SetActive(isPaused);
    }

    private void CheckPlayerInBoundsAfterResume()
    {
        if (!isPaused && !isGameOver)
        {
            if (debugMode)
                Debug.Log("[恢復後檢查] 執行玩家是否在活動框內的檢查");

            CheckPlayerInBounds();
        }
    }

    public void RestartGame()
    {
        if (debugMode)
        {
            Debug.Log("🔄 Restart 被觸發");
        }

        InitializeGameState();

        gameOverText?.gameObject.SetActive(false);
        gameOverPanel?.SetActive(false);

        if (activityBox != null)
        {
            activityBox.transform.localScale = boxInitialSize;
            activityBox.transform.position = boxInitialPosition;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
            maskSprite.sizeDelta = new Vector2(clampedScale.x * 100f, clampedScale.y * 100f);
        }

        if (debugMode)
            Debug.Log("[縮放] 活動框與 maskSprite: " + clampedScale);
    }

    public void TeleportPlayerToMask()
    {
        if (isPaused || isGameOver)
        {
            if (debugMode)
                Debug.LogWarning("無法傳送：遊戲暫停或已結束");
            return;
        }

        if (player == null || activityBox == null)
        {
            if (debugMode)
                Debug.LogWarning("無法傳送：Player 或 ActivityBox 為空");
            return;
        }

        StopAllBoundsCheck();

        Vector3 maskCenter = activityBox.transform.position;
        player.transform.position = new Vector3(maskCenter.x, maskCenter.y, player.transform.position.z);

        StartBoundsCheck(boundsCheckDelay);

        if (debugMode)
            Debug.Log($"[傳送] 玩家已傳送到活動框中心: {maskCenter}");
    }

    public void SetDraggingState(bool isDragging)
    {
        isDraggingActivityBox = isDragging;

        if (isDragging)
        {
            StopAllBoundsCheck();
            if (debugMode)
                Debug.Log("[拖拽狀態] 開始拖拽 - 所有範圍檢測已停止");
        }
        else
        {
            if (!isPaused && !isGameOver)
            {
                StartBoundsCheck(boundsCheckDelay);
                if (debugMode)
                    Debug.Log($"[拖拽狀態] 結束拖拽 - 將在 {boundsCheckDelay} 秒後開始範圍檢測");
            }
        }
    }

    public void EnableBoundsCheck(float delay = 0f) => StartBoundsCheck(delay);
    public void DisableBoundsCheck() => StopAllBoundsCheck();
    public bool CanCheckBounds() => ShouldPerformBoundsCheck();
    public void ResetBoundsCheckState()
    {
        if (!isPaused && !isGameOver)
        {
            StartBoundsCheck(0f);
            if (debugMode)
                Debug.Log("[重置] 範圍檢測狀態已重置");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter == maskSprite.gameObject)
        {
            isDraggingMask = true;
            SetDraggingState(true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDraggingMask)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                maskSprite.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );
            maskSprite.anchoredPosition = localPoint;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0f;
            MoveActivityBox(worldPos);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDraggingMask)
        {
            isDraggingMask = false;
            SetDraggingState(false);
        }
    }
}
