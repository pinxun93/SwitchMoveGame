using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public Transform activityBox;
    public Vector2 boxSize = new Vector2(4, 4);

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector3 boxCenter = activityBox.position;
        Vector2 min = boxCenter - (Vector3)(boxSize / 2);
        Vector2 max = boxCenter + (Vector3)(boxSize / 2);

        float moveX = Input.GetAxisRaw("Horizontal");
        Vector2 newPos = transform.position + new Vector3(moveX * moveSpeed * Time.deltaTime, 0);

        //限制角色只能在活動框範圍內移動
        if (newPos.x > min.x && newPos.x < max.x)
        {
            transform.position = newPos;
        }

        //跳躍（需站在地上）
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        isGrounded = false;
    }
}
