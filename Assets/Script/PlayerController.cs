using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動設置")]
    public float moveSpeed = 5f;
    public LayerMask wallLayer;
    public Transform wallCheckRight;
    public Transform wallCheckLeft;
    public float wallCheckRadius = 0.2f;

    [Header("跳躍設置")]
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Debug設置")]
    public bool debugMode = false;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isFacingRight = true;
    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 確保一開始向右移動
        isFacingRight = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void Update()
    {
        Debug.Log("Is Grounded: " + isGrounded);
        // 檢測是否著地 - 即使在暫停模式也檢測，但不應用物理效果
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 右鍵控制暫停/繼續，無論遊戲狀態如何都能檢測
        if (Input.GetMouseButtonDown(1))
        {
            gameManager.TogglePause();

            if (debugMode)
            {
                Debug.Log("暫停狀態: " + gameManager.IsPaused);
            }
        }

        // 左鍵跳躍 - 無論暫停與否都檢測，但實際只在非暫停時執行
        if (Input.GetMouseButtonDown(0))
        {
            if (!gameManager.IsPaused && isGrounded)
            {
                Jump();
                if (debugMode)
                {
                    Debug.Log("跳躍！");
                }
            }
        }

        // 只有在遊戲正常運行時才進行移動和檢測牆壁
        if (!gameManager.IsPaused)
        {
            // 檢測碰到牆壁
            CheckWallCollision();

            // 自動移動
            AutoMove();
        }
    }

    private void AutoMove()
    {
        // 根據朝向自動移動
        float direction = isFacingRight ? 1f : -1f;

        // 更新速度，保持y方向的速度不變，改變x方向速度
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
    }

    private void CheckWallCollision()
    {
        bool hitWallRight = false;
        bool hitWallLeft = false;

        // 檢測右側是否碰到牆壁
        if (wallCheckRight != null)
        {
            hitWallRight = Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer);
        }

        // 檢測左側是否碰到牆壁
        if (wallCheckLeft != null)
        {
            hitWallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, wallLayer);
        }

        // 如果碰到對應方向的牆壁，則轉向
        if ((isFacingRight && hitWallRight) || (!isFacingRight && hitWallLeft))
        {
            Flip();
        }
    }

    private void Flip()
    {
        // 改變朝向
        isFacingRight = !isFacingRight;

        // 翻轉角色精靈
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    private void Jump()
    {
        // 保持水平速度，設置垂直速度為跳躍力
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    // 在編輯器中顯示檢測範圍，幫助設置
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
    }
}
