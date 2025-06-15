using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkMask : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        GameObject playerObj = GameObject.Find("Player"); // 不加 "/"，通常這樣找就好
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogError("找不到名為 Player 的物件！");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == "MaskSprite" && playerController != null)
        {
            print("TURN");
            playerController.Flip();
        }
    }
}
