using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityBoxController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool isVertical = false;

    void Update()
    {
        // 切換移動方向（右鍵）
        if (Input.GetMouseButtonDown(1))
        {
            isVertical = !isVertical;
        }

        // 活動框移動（左鍵）
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 dir = isVertical ? Vector3.up : Vector3.right;
            //transform.position += dir * moveSpeed;
        }
    }
}
