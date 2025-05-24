using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkMask : MonoBehaviour
{

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.name == "MaskSprite")
        {
            print("TURN");
            GameObject.Find("/Player").GetComponent<PlayerController>().Flip();
        }
    }
}
