using UnityEngine;

public class FlagTrigger : MonoBehaviour
{
    [Header("Debug設置")]
    public bool debugMode = false;

    private GameManager gameManager;
    private bool wasGamePaused = false; // 追蹤遊戲暫停狀態

    private void Start()
    {
        if (debugMode)
            Debug.Log($"🏁 [FlagTrigger] {gameObject.name} 初始化");

        // 尋找 GameManager
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError($"❌ [FlagTrigger] {gameObject.name} 找不到 GameManager！");
            return;
        }

        // 確保 Flag 有正確的標籤
        if (!gameObject.CompareTag("Flag"))
        {
            gameObject.tag = "Flag";
            if (debugMode)
                Debug.Log($"✅ [FlagTrigger] {gameObject.name} 設置為 Flag 標籤");
        }

        // 確保 Flag 有碰撞器
        Collider2D flagCollider = GetComponent<Collider2D>();
        if (flagCollider == null)
        {
            flagCollider = gameObject.AddComponent<BoxCollider2D>();
            if (debugMode)
                Debug.Log($"✅ [FlagTrigger] {gameObject.name} 添加了 BoxCollider2D");
        }

        // 設置為觸發器
        flagCollider.isTrigger = true;

        if (debugMode)
            Debug.Log($"✅ [FlagTrigger] {gameObject.name} 初始化完成");
    }

    private void Update()
    {
        if (gameManager == null) return;

        // 檢測遊戲狀態變化
        bool isCurrentlyPaused = gameManager.IsPaused;

        // 當從暫停狀態恢復到繼續狀態時的處理
        if (wasGamePaused && !isCurrentlyPaused)
        {
            if (debugMode)
                Debug.Log($"🔄 [FlagTrigger] 遊戲從暫停恢復，重新啟用檢測");
        }
        // 當從繼續狀態變為暫停狀態時的處理
        else if (!wasGamePaused && isCurrentlyPaused)
        {
            if (debugMode)
                Debug.Log($"⏸️ [FlagTrigger] 遊戲暫停，停止檢測");
        }

        wasGamePaused = isCurrentlyPaused;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只有當碰撞的是 Mask 時才處理
        if (!other.CompareTag("Mask")) return;

        if (debugMode)
            Debug.Log($"🎯 [FlagTrigger] {gameObject.name} 碰到了 Mask: {other.name}");

        // 檢查 GameManager 是否存在
        if (gameManager == null)
        {
            Debug.LogError($"❌ [FlagTrigger] GameManager 不存在，無法執行傳送");
            return;
        }

        // 🔑 關鍵：只有在遊戲繼續時才能觸發傳送
        if (gameManager.IsPaused)
        {
            if (debugMode)
                Debug.Log($"⏸️ [FlagTrigger] 遊戲暫停中，不執行傳送");
            return;
        }

        // 檢查遊戲是否已結束
        if (gameManager.IsGameOver)
        {
            if (debugMode)
                Debug.Log($"💀 [FlagTrigger] 遊戲已結束，不執行傳送");
            return;
        }

        // 檢查是否正在拖動活動框
        if (gameManager.IsDraggingActivityBox)
        {
            if (debugMode)
                Debug.Log($"🖱️ [FlagTrigger] 正在拖動活動框，不執行傳送");
            return;
        }

        // 通過所有檢查，執行傳送
        if (debugMode)
            Debug.Log($"🚀 [FlagTrigger] 所有條件滿足，觸發玩家傳送！");

        gameManager.TeleportPlayerToMask();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 持續接觸時也要遵循相同規則
        if (!other.CompareTag("Mask")) return;

        // 如果遊戲暫停或結束，不做任何處理
        if (gameManager != null && (gameManager.IsPaused || gameManager.IsGameOver))
        {
            return;
        }

        // 可以在這裡添加持續接觸時的邏輯（如果需要的話）
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Mask") && gameManager != null && debugMode)
        {
            Debug.Log($"↩️ [FlagTrigger] {gameObject.name} Mask 離開了: {other.name}");
        }
    }

    // 手動測試功能（僅在 Debug 模式下可用）
    private void OnMouseDown()
    {
        if (!debugMode || gameManager == null) return;

        Debug.Log($"🖱️ [FlagTrigger] {gameObject.name} 被點擊 - 手動觸發傳送測試");

        // 手動觸發也要遵循相同的規則
        if (gameManager.IsPaused)
        {
            Debug.LogWarning($"❌ [FlagTrigger] 無法手動觸發：遊戲暫停中");
            return;
        }

        if (gameManager.IsGameOver)
        {
            Debug.LogWarning($"❌ [FlagTrigger] 無法手動觸發：遊戲已結束");
            return;
        }

        if (gameManager.IsDraggingActivityBox)
        {
            Debug.LogWarning($"❌ [FlagTrigger] 無法手動觸發：正在拖動活動框");
            return;
        }

        Debug.Log($"🎯 [FlagTrigger] 手動觸發傳送");
        gameManager.TeleportPlayerToMask();
    }

    // 顯示 Flag 的碰撞範圍（在場景視圖中）
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // 根據遊戲狀態改變顏色
            if (gameManager != null)
            {
                if (gameManager.IsPaused)
                    Gizmos.color = Color.yellow; // 暫停時顯示黃色
                else if (gameManager.IsGameOver)
                    Gizmos.color = Color.red;    // 遊戲結束時顯示紅色
                else
                    Gizmos.color = Color.green;  // 正常狀態顯示綠色
            }
            else
            {
                Gizmos.color = Color.gray; // 沒有 GameManager 時顯示灰色
            }

            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }

    // 公開方法：讓其他腳本可以查詢當前是否可以觸發傳送
    public bool CanTriggerTeleport()
    {
        if (gameManager == null) return false;

        return !gameManager.IsPaused &&
               !gameManager.IsGameOver &&
               !gameManager.IsDraggingActivityBox;
    }

    // 公開方法：強制觸發傳送（忽略某些限制，但仍檢查核心狀態）
    public void ForceTriggerTeleport()
    {
        if (gameManager == null)
        {
            Debug.LogError($"❌ [FlagTrigger] 無法強制觸發：GameManager 不存在");
            return;
        }

        // 即使是強制觸發，也不能在暫停或遊戲結束時執行
        if (gameManager.IsPaused || gameManager.IsGameOver)
        {
            Debug.LogWarning($"❌ [FlagTrigger] 無法強制觸發：遊戲暫停或已結束");
            return;
        }

        if (debugMode)
            Debug.Log($"⚡ [FlagTrigger] 強制觸發傳送");

        gameManager.TeleportPlayerToMask();
    }
}