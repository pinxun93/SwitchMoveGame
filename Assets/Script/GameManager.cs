﻿using UnityEngine;
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
    private Collider boxCollider;
    private Collider playerCollider;

    [Header("範圍檢查")]
    public Collider2D maskCollider;

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
            {
                Debug.Log("Player 已從父物件脫離，避免被 Mask 移動影響");
            }
        }

        if (player != null)
        {
            Debug.Log("Player 的目前父物件是: " + player.transform.parent?.name);
            player.transform.SetParent(null);
            Debug.Log("Player 已脫離父物件！新的父物件是: " + player.transform.parent?.name);
        }

        // 保存活動框的初始設置
        if (activityBox != null)
        {
            boxInitialSize = activityBox.transform.localScale;
            boxInitialPosition = activityBox.transform.position;
            activityBox.SetActive(true);

            // 獲取活動框的碰撞器
            boxCollider = activityBox.GetComponent<Collider>();
            if (boxCollider == null)
            {
                Debug.LogWarning("活動框沒有Collider組件！請添加一個Collider。");
            }
        }

        // 獲取玩家碰撞器
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
            {
                Debug.LogWarning("玩家沒有Collider組件！請添加一個Collider。");
            }
        }
    

        // 隱藏UI容器和遊戲結束文字
        if (activityBoxUIContainer != null)
        {
            activityBoxUIContainer.SetActive(false);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 啟動時間
        Time.timeScale = 1f;
        isGameOver = false;
    }

    private void Update()
    {
        // 只在遊戲進行中檢測角色位置
        if (!isPaused && !isGameOver)
        {
            CheckPlayerInBounds();
        }

        // 檢查從暫停 → 恢復的瞬間，玩家是否仍在 Mask 內
        if (!IsPaused && wasPaused)
        {
            CheckPlayerInsideMask();
        }
        wasPaused = IsPaused;
    }

    private void CheckPlayerInsideMask()
    {
        if (maskCollider == null || player == null) return;

        Vector2 playerPos = player.transform.position;

        // 使用 Collider2D 的 OverlapPoint 判斷玩家是否在 Mask 範圍內
        bool isInside = maskCollider.OverlapPoint(playerPos);

        if (!isInside)
        {
            Debug.LogWarning("玩家不在 Mask 範圍內，遊戲結束！");
            GameOver();
        }
        else if (debugMode)
        {
            Debug.Log("玩家仍在 Mask 範圍內，繼續遊戲");
        }
    }

    private void CheckPlayerInBounds()
    {
        if (player == null || boxCollider == null)
            return;

        // 檢查玩家是否在活動框範圍內
        bool isPlayerInBounds = boxCollider.bounds.Contains(player.transform.position);

        // 如果使用更精確的碰撞檢測
        if (playerCollider != null)
        {
            isPlayerInBounds = boxCollider.bounds.Intersects(playerCollider.bounds);
        }

        if (!isPlayerInBounds)
        {
            GameOver();
        }

        if (debugMode)
        {
            Debug.Log($"玩家位置: {player.transform.position}, 在範圍內: {isPlayerInBounds}");
        }
    }

    private void GameOver()
    {
        if (isGameOver) return; // 防止重複觸發

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
        Debug.Log("Restart 按下了！");

        isGameOver = false;
        isPaused = false;

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

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
        if (isGameOver) return; // 遊戲結束時不能暫停/繼續

        isPaused = !isPaused;
        if (isPaused)
        {
            // 暫停遊戲
            Time.timeScale = 0f;
            // 顯示活動框和UI容器
            if (activityBox != null)
            {
                activityBox.SetActive(true);
            }
            if (activityBoxUIContainer != null)
            {
                activityBoxUIContainer.SetActive(true);
            }
            if (debugMode)
            {
                Debug.Log("遊戲已暫停");
            }
        }
        else
        {
            // 繼續遊戲
            Time.timeScale = 1f;
            // 隱藏活動框和UI容器
            if (activityBox != null)
            {
                activityBox.SetActive(true);
            }
            if (activityBoxUIContainer != null)
            {
                activityBoxUIContainer.SetActive(false);
            }
            if (debugMode)
            {
                Debug.Log("遊戲已繼續");
            }
        }
    }

    // 提供給外部調用的方法，用於移動活動框而不影響角色位置
    public void MoveActivityBox(Vector3 newPosition)
    {
        if (activityBox != null && isPaused)
        {
            activityBox.transform.position = newPosition;

            if (debugMode)
            {
                Debug.Log($"活動框移動到: {newPosition}");
            }
        }
    }

    // 提供給外部調用的方法，用於調整活動框大小
    public void ResizeActivityBox(Vector3 newScale)
    {
        if (activityBox != null && isPaused)
        {
            // 限制縮放範圍
            float clampedX = Mathf.Clamp(newScale.x, minBoxSize, maxBoxSize);
            float clampedY = Mathf.Clamp(newScale.y, minBoxSize, maxBoxSize);
            float clampedZ = Mathf.Clamp(newScale.z, minBoxSize, maxBoxSize);

            Vector3 clampedScale = new Vector3(clampedX, clampedY, clampedZ);
            activityBox.transform.localScale = clampedScale;

            if (debugMode)
            {
                Debug.Log($"活動框縮放到: {clampedScale}");
            }
        }
    }
}