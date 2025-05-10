using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIActivityControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Transform activityBox;      // 活動框物件
    public Slider widthSlider;         // 控制寬度的滑桿
    public Slider heightSlider;        // 控制高度的滑桿

    private bool isDragging = false;   // 是否正在拖動
    private Vector2 lastMousePosition; // 上一次滑鼠位置

    void Update()
    {
        // 只有在「暫停狀態」下，才能調整活動框大小
        if (Time.timeScale == 0 && activityBox != null)
        {
            // 保留原始代碼，使用滑桿控制活動框大小
            float width = widthSlider != null ? widthSlider.value : activityBox.localScale.x;
            float height = heightSlider != null ? heightSlider.value : activityBox.localScale.y;
            activityBox.localScale = new Vector3(width, height, 1f);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 只有右鍵點擊才開始拖動
        if (eventData.button == PointerEventData.InputButton.Right && Time.timeScale == 0 && activityBox != null)
        {
            isDragging = true;
            lastMousePosition = eventData.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 釋放右鍵時停止拖動
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            isDragging = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 如果是右鍵拖動，且在暫停狀態下
        if (isDragging && Time.timeScale == 0 && activityBox != null)
        {
            Vector2 currentMousePosition = eventData.position;
            Vector2 difference = currentMousePosition - lastMousePosition;

            // 根據活動框的類型選擇適當的移動方式
            if (activityBox.GetComponent<RectTransform>() != null)
            {
                // 如果是UI元素，直接修改anchoredPosition
                RectTransform rectTransform = activityBox.GetComponent<RectTransform>();
                rectTransform.anchoredPosition += difference;
            }
            else
            {
                // 如果是一般GameObject，轉換為世界坐標移動
                Vector3 worldDifference = Camera.main.ScreenToWorldPoint(new Vector3(currentMousePosition.x, currentMousePosition.y, activityBox.position.z)) -
                                         Camera.main.ScreenToWorldPoint(new Vector3(lastMousePosition.x, lastMousePosition.y, activityBox.position.z));

                activityBox.position += new Vector3(worldDifference.x, worldDifference.y, 0);
            }

            lastMousePosition = currentMousePosition;
        }
    }
}