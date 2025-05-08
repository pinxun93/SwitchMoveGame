using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool inputEnabled = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetInputEnabled(bool value)
    {
        inputEnabled = value;
    }

    public void Jump()
    {
        if (inputEnabled && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        isGrounded = false;
    }
}
