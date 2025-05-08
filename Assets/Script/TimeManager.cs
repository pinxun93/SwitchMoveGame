using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public PlayerController player;
    private bool isPaused = false;

    void Update()
    {
        // ¥kÁä¤Á´«¼È°±»P«ì´_
        if (Input.GetMouseButtonDown(1))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            player.SetInputEnabled(!isPaused);
        }

        // ¥ªÁäÂIÀ»¡AÄ²µo¸õÅD¡]¶È«D¼È°±®É¡^
        if (!isPaused && Input.GetMouseButtonDown(0))
        {
            player.Jump();
        }
    }
}
