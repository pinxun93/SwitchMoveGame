using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityBoxController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool isVertical = false;

    void Update()
    {
        // �������ʤ�V�]�k��^
        if (Input.GetMouseButtonDown(1))
        {
            isVertical = !isVertical;
        }

        // ���ʮز��ʡ]����^
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 dir = isVertical ? Vector3.up : Vector3.right;
            //transform.position += dir * moveSpeed;
        }
    }
}
