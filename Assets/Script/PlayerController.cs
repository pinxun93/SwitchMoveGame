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
    public float groundCheckRadius = 0.2f;

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
    }

    private void Update()
    {
        // 判斷是否著地 - 包含普通地面和Mask
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer | maskLayer);

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

        // 只有當角色從側面碰到Mask且不是站在Mask上時才轉向
        if (hitMaskSide && !IsStandingOnMask())
        {
            Flip();
            if (debugMode)
                Debug.Log("從側面碰到 Mask，轉向！");
        }
    }

    // 檢查角色是否站在Mask上
    private bool IsStandingOnMask()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, maskLayer);
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

    // 碰撞檢測 - 當碰到Goal時觸發
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
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
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


