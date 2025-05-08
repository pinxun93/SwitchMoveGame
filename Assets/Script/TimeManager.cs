using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public PlayerController player;
    private bool isPaused = false;

    void Update()
    {
        // �k������Ȱ��P��_
        if (Input.GetMouseButtonDown(1))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            player.SetInputEnabled(!isPaused);
        }

        // �����I���AĲ�o���D�]�ȫD�Ȱ��ɡ^
        if (!isPaused && Input.GetMouseButtonDown(0))
        {
            player.Jump();
        }
    }
}
