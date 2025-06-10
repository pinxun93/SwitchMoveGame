using UnityEngine;

public class MovingPlatformHandler : MonoBehaviour
{
    [Header("Debug設置")]
    public bool debugMode = false;

    private GameManager gameManager;
    private GameObject currentPlayer;
    private Vector3 playerPositionBeforePause;
    private bool wasPlayerOnPlatform = false;
    private bool hasSavedPlayerPosition = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null && debugMode)
            Debug.LogWarning("MovingPlatformHandler: 找不到 GameManager！");
    }

    private void Update()
    {
        if (gameManager != null)
        {
            // 遊戲剛暫停時，保存玩家位置並解除關係
            if (gameManager.IsPaused && !hasSavedPlayerPosition && currentPlayer != null)
            {
                SavePlayerPositionAndDetach();
            }
            // 遊戲暫停期間，持續強制玩家回到保存的位置
            else if (gameManager.IsPaused && hasSavedPlayerPosition && currentPlayer != null)
            {
                ForcePlayerToSavedPosition();
            }
            // 遊戲恢復時，重置狀態
            else if (!gameManager.IsPaused && hasSavedPlayerPosition)
            {
                ResumePlayerMovement();
            }
        }
    }

    private void SavePlayerPositionAndDetach()
    {
        if (currentPlayer != null)
        {
            playerPositionBeforePause = currentPlayer.transform.position;
            hasSavedPlayerPosition = true;

            // 強制解除父子關係
            if (currentPlayer.transform.parent == transform)
            {
                currentPlayer.transform.SetParent(null);
            }

            // 停止玩家的物理運動
            Rigidbody2D playerRb = currentPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.isKinematic = true; // 暫停期間停用物理
            }

            if (debugMode)
                Debug.Log($"暫停時保存玩家位置: {playerPositionBeforePause}，並解除物理");
        }
    }

    private void ForcePlayerToSavedPosition()
    {
        if (currentPlayer != null)
        {
            // 強制玩家保持在保存的位置
            currentPlayer.transform.position = playerPositionBeforePause;

            // 確保沒有父子關係
            if (currentPlayer.transform.parent != null)
            {
                currentPlayer.transform.SetParent(null);
            }

            // 確保速度為零
            Rigidbody2D playerRb = currentPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
            }
        }
    }

    private void ResumePlayerMovement()
    {
        if (currentPlayer != null)
        {
            // 恢復玩家的物理
            Rigidbody2D playerRb = currentPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
            }

            // 如果玩家仍在平台觸發器內，重新建立父子關係
            Collider2D platformCollider = GetComponent<Collider2D>();
            Collider2D playerCollider = currentPlayer.GetComponent<Collider2D>();

            if (platformCollider != null && playerCollider != null &&
                platformCollider.bounds.Intersects(playerCollider.bounds))
            {
                currentPlayer.transform.SetParent(transform);
                wasPlayerOnPlatform = true;
            }

            if (debugMode)
                Debug.Log("遊戲恢復，重新啟用玩家物理");
        }

        hasSavedPlayerPosition = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.gameObject;

            // 只有在遊戲未暫停時才建立父子關係
            if (gameManager == null || !gameManager.IsPaused)
            {
                other.transform.SetParent(transform);
                wasPlayerOnPlatform = true;

                if (debugMode)
                    Debug.Log($"Player 進入 {gameObject.name}，建立父子關係");
            }
            else if (debugMode)
            {
                Debug.Log($"Player 進入 {gameObject.name}，但遊戲暫停中，不建立父子關係");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.gameObject;

            // 如果遊戲暫停，確保玩家不被拖動
            if (gameManager != null && gameManager.IsPaused)
            {
                if (other.transform.parent == transform)
                {
                    other.transform.SetParent(null);
                }

                // 如果已保存位置，強制回到該位置
                if (hasSavedPlayerPosition)
                {
                    other.transform.position = playerPositionBeforePause;
                }
            }
            // 遊戲恢復且玩家仍在平台上時重新建立關係
            else if (gameManager != null && !gameManager.IsPaused &&
                     other.transform.parent != transform && wasPlayerOnPlatform)
            {
                other.transform.SetParent(transform);
                if (debugMode)
                    Debug.Log($"遊戲恢復，重新建立 Player 與 {gameObject.name} 的父子關係");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 玩家離開時解除所有關係和保存的狀態
            if (other.transform.parent == transform)
            {
                other.transform.SetParent(null);
            }

            // 恢復玩家物理（如果被停用）
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.isKinematic)
            {
                playerRb.isKinematic = false;
            }

            wasPlayerOnPlatform = false;
            hasSavedPlayerPosition = false;

            if (currentPlayer == other.gameObject)
                currentPlayer = null;

            if (debugMode)
                Debug.Log($"Player 離開 {gameObject.name}，清理所有狀態");
        }
    }

    // 公開方法，讓其他腳本可以強制解除玩家關係
    public void ForceDetachAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.transform.parent == transform)
            {
                player.transform.SetParent(null);
            }

            // 恢復物理
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.isKinematic)
            {
                playerRb.isKinematic = false;
            }
        }

        currentPlayer = null;
        wasPlayerOnPlatform = false;
        hasSavedPlayerPosition = false;

        if (debugMode)
            Debug.Log($"強制清理 {gameObject.name} 的所有玩家關係");
    }

    private void OnDisable()
    {
        ForceDetachAllPlayers();
    }
}