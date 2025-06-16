using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkMask : MonoBehaviour
{
    private PlayerController playerController;
    private GameManager gameManager;

    void Start()
    {
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogError("找不到名為 Player 的物件！");
        }

        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj != null)
        {
            gameManager = gmObj.GetComponent<GameManager>();
        }
        else
        {
            Debug.LogError("找不到 GameManager！");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == "MaskSprite" && playerController != null)
        {
            // 🔒 加入暫停時不執行
            if (gameManager != null && gameManager.IsPaused)
            {
                Debug.Log("暫停中，跳過 Flip()");
                return;
            }

            Debug.Log("TURN");
            playerController.Flip();
        }
    }
}
