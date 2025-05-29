using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("移動設置")]
    public float moveSpeed = 5f;
    public LayerMask wallLayer;
    public Transform wallCheckRight;
    public Transform wallCheckLeft;
    public float wallCheckRadius = 0.2f;

    [Header("Mask 偵測設置")]
    public LayerMask maskLayer;
    public Transform maskCheckRight;
    public Transform maskCheckLeft;
    public float maskCheckRadius = 0.2f;

    [Header("跳躍設置")]
    public float jumpForce = 10f;
    public LayerMask groundLayer;         // 普通地面Layer
    public Transform groundCheck;
    public float rayLength = 0.15f;       // 判斷腳下地面用射線長度

    [Header("關卡設置")]
    public string nextSceneName = "";
    public float goalDelayTime = 1f;

    [Header("Debug設置")]
    public bool debugMode = false;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool hasReachedGoal = false;
    private bool isFacingRight = true;
    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        isFacingRight = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }

        if (debugMode)
        {
            Debug.Log($"[Awake] Rigidbody2D.gravityScale = {rb.gravityScale}, simulated = {rb.simulated}");
        }
    }

    private void Update()
    {
        CheckGrounded();

        // 顯示物理狀態（偵錯用）
        if (debugMode)
        {
            Debug.Log($"[Update] Velocity.y = {rb.velocity.y}, TimeScale = {Time.timeScale}, IsGrounded = {isGrounded}");
        }

        // 暫停
        if (Input.GetMouseButtonDown(1))
        {
            gameManager.TogglePause();
            if (debugMode)
                Debug.Log("暫停狀態: " + gameManager.IsPaused);
        }

        // 跳躍
        if (Input.GetMouseButtonDown(0))
        {
            if (!gameManager.IsPaused && isGrounded)
            {
                Jump();
                if (debugMode)
                    Debug.Log("跳躍！");
            }
        }

        if (!gameManager.IsPaused && !hasReachedGoal)
        {
            CheckWallCollision();
            CheckMaskSideCollision();
            AutoMove();
        }
    }
    private void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, rayLength, groundLayer | maskLayer);
        isGrounded = (hit.collider != null);

        if (debugMode)
        {
            Debug.DrawRay(groundCheck.position, Vector2.down * rayLength, isGrounded ? Color.green : Color.red);
            Debug.Log("Raycast Grounded: " + isGrounded);
        }
    }

    private void AutoMove()
    {
        float direction = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
    }

    private void CheckWallCollision()
    {
        bool hitWall = false;

        if (isFacingRight && wallCheckRight != null)
        {
            hitWall = Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer);
        }
        else if (!isFacingRight && wallCheckLeft != null)
        {
            hitWall = Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, wallLayer);
        }

        if (hitWall)
        {
            Flip();
        }
    }

    private void CheckMaskSideCollision()
    {
        bool hitMaskSide = false;

        if (isFacingRight && maskCheckRight != null)
        {
            hitMaskSide = Physics2D.OverlapCircle(maskCheckRight.position, maskCheckRadius, maskLayer);
        }
        else if (!isFacingRight && maskCheckLeft != null)
        {
            hitMaskSide = Physics2D.OverlapCircle(maskCheckLeft.position, maskCheckRadius, maskLayer);
        }

        if (hitMaskSide && !IsStandingOnMask())
        {
            Flip();
            if (debugMode)
                Debug.Log("從側面碰到 Mask，轉向！");
        }
    }

    private bool IsStandingOnMask()
    {
        // 直接用剛剛的 raycast 判斷，避免重複用 OverlapCircle
        return isGrounded && Physics2D.Raycast(groundCheck.position, Vector2.down, rayLength, maskLayer);
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Goal") && !hasReachedGoal)
        {
            hasReachedGoal = true;
            if (debugMode)
                Debug.Log("碰到Goal！準備進入下一關");

            rb.velocity = Vector2.zero;
            Invoke("LoadNextLevel", goalDelayTime);
        }
    }

    private void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                if (debugMode)
                    Debug.Log("已經是最後一關！");
                SceneManager.LoadScene(0);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, rayLength);
        }

        if (wallCheckRight != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheckRight.position, wallCheckRadius);
        }

        if (wallCheckLeft != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheckLeft.position, wallCheckRadius);
        }

        if (maskCheckRight != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(maskCheckRight.position, maskCheckRadius);
        }

        if (maskCheckLeft != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(maskCheckLeft.position, maskCheckRadius);
        }
    }
}
