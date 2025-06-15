using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkMask : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        GameObject playerObj = GameObject.Find("Player"); // ���[ "/"�A�q�`�o�˧�N�n
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogError("�䤣��W�� Player ������I");
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
