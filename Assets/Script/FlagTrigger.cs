using UnityEngine;

public class FlagTrigger : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
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
            Debug.Log($"✅ [FlagTrigger] {gameObject.name} 設置為 Flag 標籤");
        }

        // 確保 Flag 有碰撞器
        Collider2D flagCollider = GetComponent<Collider2D>();
        if (flagCollider == null)
        {
            flagCollider = gameObject.AddComponent<BoxCollider2D>();
            Debug.Log($"✅ [FlagTrigger] {gameObject.name} 添加了 BoxCollider2D");
        }

        // 設置為觸發器
        flagCollider.isTrigger = true;

        Debug.Log($"✅ [FlagTrigger] {gameObject.name} 初始化完成");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mask"))
        {
            Debug.Log($"🎯 [FlagTrigger] {gameObject.name} 碰到了 Mask: {other.name}");

            if (gameManager != null)
            {
                if (gameManager.IsPaused)
                {
                    Debug.Log($"⏸️ [FlagTrigger] 遊戲暫停中，不執行傳送");
                    return;
                }
                if (gameManager.IsGameOver)
                {
                    Debug.Log($"💀 [FlagTrigger] 遊戲已結束，不執行傳送");
                    return;
                }
                // 新增：检查是否正在拖动
                if (gameManager.IsDraggingActivityBox)
                {
                    Debug.Log($"🖱️ [FlagTrigger] 正在拖動活動框，不執行傳送");
                    return;
                }

                Debug.Log($"🚀 [FlagTrigger] 觸發玩家傳送！");
                gameManager.TeleportPlayerToMask();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Mask") && gameManager != null && gameManager.debugMode)
        {
            Debug.Log($"↩️ [FlagTrigger] {gameObject.name} 離開了 Mask: {other.name}");
        }
    }

    // 手動測試功能
    private void OnMouseDown()
    {
        if (gameManager != null && gameManager.debugMode)
        {
            Debug.Log($"🖱️ [FlagTrigger] {gameObject.name} 被點擊 - 手動觸發傳送測試");

            // 尋找場景中的 Mask 物件
            GameObject[] maskObjects = GameObject.FindGameObjectsWithTag("Mask");
            if (maskObjects.Length > 0)
            {
                Debug.Log($"🎯 [FlagTrigger] 找到 {maskObjects.Length} 個 Mask 物件，觸發傳送");
                gameManager.TeleportPlayerToMask();
            }
            else
            {
                Debug.LogWarning($"❌ [FlagTrigger] 找不到 Mask 標籤的物件");
            }
        }
    }

    // 顯示 Flag 的碰撞範圍（在場景視圖中）
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}