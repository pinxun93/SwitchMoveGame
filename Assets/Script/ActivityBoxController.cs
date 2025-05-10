using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActivityBoxController : MonoBehaviour
{
    [Header("UI����")]
    public Slider xSizeSlider;
    public Slider ySizeSlider;
    public RectTransform slidersPanel; // �ϥ�RectTransform�өw��UI���O
    public float sliderOffsetY = 100f; // �ƶ����O�P���ʮت����������q

    [Header("�վ�]�m")]
    public float minSize = 1f;
    public float maxSize = 10f;

    [Header("Debug�]�m")]
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

        // �p�G�S���I�����A�K�[�@��
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true; // �]��Ĳ�o���H�קK���z�v�T
        }
    }

    private void Start()
    {
        // �]�m�ƶ���l�ȩM�d��
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
        // �T�O�u���b�Ȱ��Ҧ��U�~��ާ@���ʮ�
        if (!gameManager.IsPaused)
            return;

        // ��s�ƶ����O��m�A�Ϩ���H���ʮ�
        UpdateSlidersPanelPosition();

        // �B�z����޿�
        HandleDragging();

        // �]���ڭ̨ϥΤFEventSystem�A�ҥH�ݭn�T�OUI�����u����C�����󤬰�
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // �p�G���w�bUI�����W�A���B�z�C�����󪺩��
            return;
        }
    }

    private void UpdateSlidersPanelPosition()
    {
        if (slidersPanel != null)
        {
            // �N���ʮت��@�ɧ����ഫ���̹�����
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(mainCamera, transform.position);

            // �]�m�ƶ����O����m�b���ʮؤU��
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
        // �}�l���
        if (Input.GetMouseButtonDown(0))
        {
            // �N�ƹ��I����m�q�̹��y���ഫ���@�ɮy��
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z; // �O��z�Ȥ���

            // �ˬd�ƹ��I���O�_�b���ʮؤ�
            Collider2D hit = Physics2D.OverlapPoint(new Vector2(mousePos.x, mousePos.y));

            if (hit != null && hit.transform == transform)
            {
                isDragging = true;
                offset = transform.position - mousePos;

                if (debugMode)
                {
                    Debug.Log("�}�l��ʬ��ʮ�");
                }
            }
        }

        // �������
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging && debugMode)
            {
                Debug.Log("������ʬ��ʮ�");
            }
            isDragging = false;
        }

        // �p�G���b��ʡA��s���ʮئ�m
        if (isDragging)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z; // �O��z�Ȥ���
            transform.position = mousePos + offset;
        }
    }

    // �q�LX�b�ƶ��վ㬡�ʮؼe��
    private void UpdateXSize(float value)
    {
        Vector3 newScale = transform.localScale;
        newScale.x = value;
        transform.localScale = newScale;

        // ��s�I�����j�p
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(1f, 1f); // ���m�j�p
            boxCollider.size = new Vector2(value, boxCollider.size.y); // �]�m�s�j�p
        }

        if (debugMode)
        {
            Debug.Log("��s���ʮؼe��: " + value);
        }
    }

    // �q�LY�b�ƶ��վ㬡�ʮذ���
    private void UpdateYSize(float value)
    {
        Vector3 newScale = transform.localScale;
        newScale.y = value;
        transform.localScale = newScale;

        // ��s�I�����j�p
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(1f, 1f); // ���m�j�p
            boxCollider.size = new Vector2(boxCollider.size.x, value); // �]�m�s�j�p
        }

        if (debugMode)
        {
            Debug.Log("��s���ʮذ���: " + value);
        }
    }

    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            // ø�s���ʮؽd��A���U�ո�
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}