using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("���ʮس]�m")]
    public GameObject activityBox;
    public GameObject activityBoxUIContainer; // �]�tX�BY�b�ƶ���UI�e��
    public float minBoxSize = 1f;
    public float maxBoxSize = 10f;

    [Header("Debug�]�m")]
    public bool debugMode = false;

    private bool isPaused = false;
    private Vector3 boxInitialSize;
    private Vector3 boxInitialPosition;

    public bool IsPaused { get { return isPaused; } }

    private void Start()
    {
        // �O�s���ʮت���l�]�m
        if (activityBox != null)
        {
            boxInitialSize = activityBox.transform.localScale;
            boxInitialPosition = activityBox.transform.position;
            activityBox.SetActive(false);
        }

        // ����UI�e��
        if (activityBoxUIContainer != null)
        {
            activityBoxUIContainer.SetActive(false);
        }

        // �Ұʮɶ�
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // �Ȱ��C��
            Time.timeScale = 0f;

            // ��ܬ��ʮةMUI�e��
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
                Debug.Log("�C���w�Ȱ�");
            }
        }
        else
        {
            // �~��C��
            Time.timeScale = 1f;

            // ���ì��ʮةMUI�e��
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
                Debug.Log("�C���w�~��");
            }
        }
    }
}