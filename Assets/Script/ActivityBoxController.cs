using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActivityBoxController : MonoBehaviour
{
    [Header("UI元素")]
    public Slider xSizeSlider;
    public Slider ySizeSlider;
    public RectTransform slidersPanel; // 使用RectTransform來定位UI面板
    public float sliderOffsetY = 100f; // 滑塊面板與活動框的垂直偏移量

    [Header("調整設置")]
    public float minSize = 1f;
    public float maxSize = 10f;

    [Header("Debug設置")]
    public bool debugMode = false;

    private GameManager gameManager;
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        mainCamera = Camera.main;
        boxCollider = GetComponent<BoxCollider2D>();

        // 如果沒有碰撞器，添加一個
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true; // 設為觸發器以避免物理影響
        }
    }

    private void Start()
    {
        // 設置滑塊初始值和範圍
        if (xSizeSlider != null)
        {
            xSizeSlider.minValue = minSize;
            xSizeSlider.maxValue = maxSize;
            xSizeSlider.value = transform.localScale.x;
            xSizeSlider.onValueChanged.AddListener(UpdateXSize);
        }

        if (ySizeSlider != null)
        {
            ySizeSlider.minValue = minSize;
            ySizeSlider.maxValue = maxSize;
            ySizeSlider.value = transform.localScale.y;
            ySizeSlider.onValueChanged.AddListener(UpdateYSize);
        }
    }

    private void Update()
    {
        // 確保只有在暫停模式下才能操作活動框
        if (!gameManager.IsPaused)
            return;

        // 更新滑塊面板位置，使其跟隨活動框
        UpdateSlidersPanelPosition();

        // 處理拖動邏輯
        HandleDragging();

        // 因為我們使用了EventSystem，所以需要確保UI互動優先於遊戲物件互動
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // 如果指針在UI元素上，不處理遊戲物件的拖動
            return;
        }
    }

    private void UpdateSlidersPanelPosition()
    {
        if (slidersPanel != null)
        {
            // 將活動框的世界坐標轉換為屏幕坐標
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(mainCamera, transform.position);

            // 設置滑塊面板的位置在活動框下方
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                slidersPanel.parent as RectTransform,
                new Vector2(screenPosition.x, screenPosition.y - sliderOffsetY),
                null,
                out localPoint);

            slidersPanel.anchoredPosition = localPoint;
        }
    }

    private void HandleDragging()
    {
        // 開始拖動
        if (Input.GetMouseButtonDown(0))
        {
            // 將滑鼠點擊位置從屏幕座標轉換為世界座標
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z; // 保持z值不變

            // 檢查滑鼠點擊是否在活動框內
            Collider2D hit = Physics2D.OverlapPoint(new Vector2(mousePos.x, mousePos.y));

            if (hit != null && hit.transform == transform)
            {
                isDragging = true;
                offset = transform.position - mousePos;

                if (debugMode)
                {
                    Debug.Log("開始拖動活動框");
                }
            }
        }

        // 結束拖動
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging && debugMode)
            {
                Debug.Log("結束拖動活動框");
            }
            isDragging = false;
        }

        // 如果正在拖動，更新活動框位置
        if (isDragging)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z; // 保持z值不變
            transform.position = mousePos + offset;
        }
    }

    // 通過X軸滑塊調整活動框寬度
    private void UpdateXSize(float value)
    {
        Vector3 newScale = transform.localScale;
        newScale.x = value;
        transform.localScale = newScale;

        // 更新碰撞器大小
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(1f, 1f); // 重置大小
            boxCollider.size = new Vector2(value, boxCollider.size.y); // 設置新大小
        }

        if (debugMode)
        {
            Debug.Log("更新活動框寬度: " + value);
        }
    }

    // 通過Y軸滑塊調整活動框高度
    private void UpdateYSize(float value)
    {
        Vector3 newScale = transform.localScale;
        newScale.y = value;
        transform.localScale = newScale;

        // 更新碰撞器大小
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(1f, 1f); // 重置大小
            boxCollider.size = new Vector2(boxCollider.size.x, value); // 設置新大小
        }

        if (debugMode)
        {
            Debug.Log("更新活動框高度: " + value);
        }
    }

    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            // 繪製活動框範圍，幫助調試
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}