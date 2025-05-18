using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActivityBoxController : MonoBehaviour
{
    [Header("UI元素")]
    public Slider xSizeSlider;
    public Slider ySizeSlider;
    public RectTransform sliderPanel; // 使用RectTransform來定位UI面板
    public float sliderOffsetY = 100f; // 與活動框的距離

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

        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
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

        // ✅ 初始滑塊位置固定在底部偏下
        if (sliderPanel != null)
        {
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(mainCamera, transform.position);
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                sliderPanel.parent as RectTransform,
                new Vector2(screenPosition.x, screenPosition.y - sliderOffsetY),
                null,
                out localPoint
            );
            sliderPanel.anchoredPosition = localPoint;
            sliderPanel.gameObject.SetActive(false); // 一開始不顯示
        }
    }

    private bool wasPaused = false; // 記錄上一次是否為暫停狀態

    private void Update()
    {
        bool isPaused = gameManager.IsPaused;

        // 更新滑塊面板可見性
        if (sliderPanel != null)
        {
            sliderPanel.gameObject.SetActive(isPaused);

            // 每次剛進入暫停狀態時更新滑塊位置
            if (isPaused && !wasPaused)
            {
                UpdateSliderPanelPosition();
            }
        }

        wasPaused = isPaused;

        if (!isPaused)
            return;

        HandleDragging();

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;

            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.transform == transform)
            {
                isDragging = true;
                offset = transform.position - mousePos;

                if (debugMode) Debug.Log("開始拖動活動框");
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging && debugMode) Debug.Log("結束拖動活動框");
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            transform.position = mousePos + offset;
        }
    }

    private void UpdateXSize(float value)
    {
        Vector3 newScale = transform.localScale;
        newScale.x = value;
        transform.localScale = newScale;

        if (boxCollider != null)
            boxCollider.size = new Vector2(value, boxCollider.size.y);

        if (debugMode) Debug.Log("更新寬度: " + value);
    }

    private void UpdateYSize(float value)
    {
        Vector3 newScale = transform.localScale;
        newScale.y = value;
        transform.localScale = newScale;

        if (boxCollider != null)
            boxCollider.size = new Vector2(boxCollider.size.x, value);

        if (debugMode) Debug.Log("更新高度: " + value);
    }

    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }



    private void UpdateSliderPanelPosition()
    {
        if (sliderPanel != null && mainCamera != null)
        {
            // 取得左下角在世界座標的點
            Vector3 bottomLeftWorld = transform.position
                - new Vector3(transform.localScale.x / 2f, transform.localScale.y / 2f, 0f);

            // 轉換成螢幕座標
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, bottomLeftWorld);

            // 將螢幕座標轉換為本地 UI 座標
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                sliderPanel.parent as RectTransform,
                screenPos,
                null, // 因為是 Overlay 不需要 Camera
                out Vector2 localPos
            );

            // 設定面板位置
            sliderPanel.anchoredPosition = localPos;
        }
    }
}
