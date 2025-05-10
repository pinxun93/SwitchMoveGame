using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("活動框設置")]
    public GameObject activityBox;
    public GameObject activityBoxUIContainer; // 包含X、Y軸滑塊的UI容器
    public float minBoxSize = 1f;
    public float maxBoxSize = 10f;

    [Header("Debug設置")]
    public bool debugMode = false;

    private bool isPaused = false;
    private Vector3 boxInitialSize;
    private Vector3 boxInitialPosition;

    public bool IsPaused { get { return isPaused; } }

    private void Start()
    {
        // 保存活動框的初始設置
        if (activityBox != null)
        {
            boxInitialSize = activityBox.transform.localScale;
            boxInitialPosition = activityBox.transform.position;
            activityBox.SetActive(false);
        }

        // 隱藏UI容器
        if (activityBoxUIContainer != null)
        {
            activityBoxUIContainer.SetActive(false);
        }

        // 啟動時間
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // 暫停遊戲
            Time.timeScale = 0f;

            // 顯示活動框和UI容器
            if (activityBox != null)
            {
                activityBox.SetActive(true);
            }

            if (activityBoxUIContainer != null)
            {
                activityBoxUIContainer.SetActive(true);
            }

            if (debugMode)
            {
                Debug.Log("遊戲已暫停");
            }
        }
        else
        {
            // 繼續遊戲
            Time.timeScale = 1f;

            // 隱藏活動框和UI容器
            if (activityBox != null)
            {
                activityBox.SetActive(false);
            }

            if (activityBoxUIContainer != null)
            {
                activityBoxUIContainer.SetActive(false);
            }

            if (debugMode)
            {
                Debug.Log("遊戲已繼續");
            }
        }
    }
}