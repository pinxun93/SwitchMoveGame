using UnityEngine;
using UnityEngine.UI;

public class UIActivityControl : MonoBehaviour
{
    [Header("元件參考")]
    public Transform mask;                    // 活動框（被拖動的物件）
    public Slider widthSlider;
    public Slider heightSlider;
    public RectTransform sliderPanel;         // UI滑桿Panel（跟著mask左下角）
    public Canvas canvas;

    [Header("滑動面板設定")]
    public float sliderOffsetY = 50f;         // 與物件距離

    private bool isDragging = false;
    private Vector3 lastMouseWorldPos;

    void Start()
    {
        if (mask != null)
        {
            mask.localScale = new Vector3(4f, 4f, 4f); // 初始大小
        }

        if (widthSlider != null)
        {
            widthSlider.minValue = 1f;
            widthSlider.maxValue = 10f;
            widthSlider.value = mask != null ? mask.localScale.x : 3f;
            widthSlider.onValueChanged.AddListener(UpdateWidth);
        }

        if (heightSlider != null)
        {
            heightSlider.minValue = 1f;
            heightSlider.maxValue = 10f;
            heightSlider.value = mask != null ? mask.localScale.y : 3f;
            heightSlider.onValueChanged.AddListener(UpdateHeight);
        }

        if (sliderPanel != null)
            sliderPanel.gameObject.SetActive(false);

        if (mask != null && mask.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = mask.gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = false; // ✅ 關掉 Trigger，讓它成為實體碰撞
        }
    }

    void Update()
    {
        // 切換暫停狀態並顯示/隱藏面板
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = (Time.timeScale == 0) ? 1 : 0;

            if (sliderPanel != null)
                sliderPanel.gameObject.SetActive(Time.timeScale == 0);
        }

        // 拖拉模式下縮放
        if (Time.timeScale == 0 && mask != null)
        {
            float width = widthSlider != null ? widthSlider.value : mask.localScale.x;
            float height = heightSlider != null ? heightSlider.value : mask.localScale.y;
            mask.localScale = new Vector3(width, height, 1f);
        }

        // 滑鼠拖曳啟動
        if (Time.timeScale == 0 && Input.GetMouseButtonDown(0) && mask != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            Collider2D col = mask.GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(mouseWorldPos))
            {
                isDragging = true;
                lastMouseWorldPos = mouseWorldPos;
            }
        }

        // 停止拖曳
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // 拖曳中更新位置
        if (isDragging && Time.timeScale == 0 && mask != null)
        {
            Vector3 currentMouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentMouseWorldPos.z = 0f;
            Vector3 delta = currentMouseWorldPos - lastMouseWorldPos;
            mask.position += delta;
            lastMouseWorldPos = currentMouseWorldPos;
        }
    }

    void LateUpdate()
    {
        // 滑動面板跟隨物件
        if (Time.timeScale != 0 || sliderPanel == null || mask == null)
            return;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, mask.position);
        Vector2 localPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            sliderPanel.parent as RectTransform,
            new Vector2(screenPos.x, screenPos.y - sliderOffsetY),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPos
        );

        sliderPanel.anchoredPosition = localPos;
    }

    private void UpdateWidth(float value)
    {
        if (mask != null)
        {
            Vector3 scale = mask.localScale;
            scale.x = value;
            mask.localScale = scale;
        }
    }

    private void UpdateHeight(float value)
    {
        if (mask != null)
        {
            Vector3 scale = mask.localScale;
            scale.y = value;
            mask.localScale = scale;
        }
    }
}