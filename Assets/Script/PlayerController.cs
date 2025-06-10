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
    public Transform maskCheckTop;
    public Transform maskCheckBottom;
    public float maskCheckRadius = 0.2f;

    [Header("跳躍設置")]
    public float jumpForce = 10f;
    public LayerMask groundLayer;
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
    private Collider2D playerCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>();

        if (playerCollider == null)
            Debug.LogError("Player 物件及其子物件都沒有 Collider2D！");
        else if (debugMode)
            Debug.Log("找到 Player 的 Collider2D：" + playerCollider.GetType().Name);

        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
            Debug.LogError("找不到 GameManager！");
    }

    private void Start()
    {
        if (transform.parent != null)
        {
            transform.SetParent(null);
            if (debugMode)
                Debug.Log("解除 Player 的父物件");
        }
    }

    private void Update()
    {
        isGrounded = IsGrounded();

        if (Input.GetMouseButtonDown(1))
        {
            gameManager.TogglePause();
            if (debugMode)
                Debug.Log("暫停狀態: " + gameManager.IsPaused);
        }

        if (Input.GetMouseButtonDown(0) && !gameManager.IsPaused && isGrounded && !IsTouchingMaskAbove())
        {
            Jump();
            if (debugMode)
                Debug.Log("跳躍！");
        }

        if (!gameManager.IsPaused && !hasReachedGoal)
        {
            CheckWallCollision();
            CheckMaskSideCollision();
            AutoMove();
        }
    }

    private bool IsGrounded()
    {
        bool onGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool onMask = maskCheckBottom != null &&
                      Physics2D.OverlapCircle(maskCheckBottom.position, maskCheckRadius, maskLayer);

        if (debugMode)
        {
            Debug.Log("OnGround: " + onGround);
            Debug.Log("OnMask: " + onMask);
        }

        return onGround || onMask;
    }

    private bool IsTouchingMaskAbove()
    {
        if (maskCheckTop == null) return false;

        bool topBlocked = Physics2D.OverlapCircle(maskCheckTop.position, maskCheckRadius, maskLayer);

        if (debugMode)
            Debug.Log("上方遮擋: " + topBlocked);

        return topBlocked;
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
            hitWall = Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer);
        else if (!isFacingRight && wallCheckLeft != null)
            hitWall = Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, wallLayer);

        if (hitWall)
            Flip();
    }

    private void CheckMaskSideCollision()
    {
        bool hitMaskSide = false;
        if (isFacingRight && maskCheckRight != null)
            hitMaskSide = Physics2D.OverlapCircle(maskCheckRight.position, maskCheckRadius, maskLayer);
        else if (!isFacingRight && maskCheckLeft != null)
            hitMaskSide = Physics2D.OverlapCircle(maskCheckLeft.position, maskCheckRadius, maskLayer);

        if (hitMaskSide && !IsStandingOnMask())
        {
            Flip();
            if (debugMode)
                Debug.Log("從側面碰到 Mask，轉向！");
        }
    }

    private bool IsStandingOnMask()
    {
        return maskCheckBottom != null &&
               Physics2D.OverlapCircle(maskCheckBottom.position, maskCheckRadius, maskLayer);
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        if (spriteRenderer != null)
            spriteRenderer.flipX = !isFacingRight;
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
            Invoke(nameof(LoadNextLevel), goalDelayTime);
        }
    }

    private void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(nextSceneIndex);
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
        if (maskCheckBottom != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(maskCheckBottom.position, maskCheckRadius);
        }
        if (maskCheckTop != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(maskCheckTop.position, maskCheckRadius);
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
