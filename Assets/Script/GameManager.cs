using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("���ʮس]�m")]
    public GameObject activityBox;
    public GameObject activityBoxUIContainer; // �]�tX�BY�b�ƶ���UI�e��
    public float minBoxSize = 1f;
    public float maxBoxSize = 10f;

    [Header("�����˴��]�m")]
    public GameObject player; // ���a����
    public Text gameOverText; // ���"You lose!"��UI��r
    public GameObject gameOverPanel; // �C���������O�]�i��^

    [Header("Debug�]�m")]
    public bool debugMode = false;

    private bool isPaused = false;
    private bool isGameOver = false;
    private Vector3 boxInitialSize;
    private Vector3 boxInitialPosition;
    private Collider boxCollider;
    private Collider playerCollider;

    public bool IsPaused { get { return isPaused; } }
    public bool IsGameOver { get { return isGameOver; } }

    private void Start()
    {
        // �O�s���ʮت���l�]�m
        if (activityBox != null)
        {
            boxInitialSize = activityBox.transform.localScale;
            boxInitialPosition = activityBox.transform.position;
            activityBox.SetActive(true);

            // ������ʮت��I����
            boxCollider = activityBox.GetComponent<Collider>();
            if (boxCollider == null)
            {
                Debug.LogWarning("���ʮبS��Collider�ե�I�вK�[�@��Collider�C");
            }
        }

        // ������a�I����
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
            {
                Debug.LogWarning("���a�S��Collider�ե�I�вK�[�@��Collider�C");
            }
        }

        // ����UI�e���M�C��������r
        if (activityBoxUIContainer != null)
        {
            activityBoxUIContainer.SetActive(false);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // �Ұʮɶ�
        Time.timeScale = 1f;
        isGameOver = false;
    }

    private void Update()
    {
        // �u�b�C���i�椤�˴������m
        if (!isPaused && !isGameOver)
        {
            CheckPlayerInBounds();
        }
    }

    private void CheckPlayerInBounds()
    {
        if (player == null || boxCollider == null)
            return;

        // �ˬd���a�O�_�b���ʮؽd��
        bool isPlayerInBounds = boxCollider.bounds.Contains(player.transform.position);

        // �p�G�ϥΧ��T���I���˴�
        if (playerCollider != null)
        {
            isPlayerInBounds = boxCollider.bounds.Intersects(playerCollider.bounds);
        }

        if (!isPlayerInBounds)
        {
            GameOver();
        }

        if (debugMode)
        {
            Debug.Log($"���a��m: {player.transform.position}, �b�d��: {isPlayerInBounds}");
        }
    }

    private void GameOver()
    {
        if (isGameOver) return; // �����Ĳ�o

        isGameOver = true;

        // �Ȱ��C��
        Time.timeScale = 0f;

        // ��ܹC�������T��
        if (gameOverText != null)
        {
            gameOverText.text = "You lose!";
            gameOverText.gameObject.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (debugMode)
        {
            Debug.Log("�C�������G�������}�F���ʽd��I");
        }
    }

    public void RestartGame()
    {
        // ���m�C�����A
        isGameOver = false;
        isPaused = false;

        // ���ùC������UI
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // ���m���ʮئ�m�M�j�p
        if (activityBox != null)
        {
            activityBox.transform.localScale = boxInitialSize;
            activityBox.transform.position = boxInitialPosition;
        }

        // ��_�ɶ�
        Time.timeScale = 1f;

        if (debugMode)
        {
            Debug.Log("�C���w���s�}�l");
        }
    }

    public void TogglePause()
    {
        if (isGameOver) return; // �C�������ɤ���Ȱ�/�~��

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
                activityBox.SetActive(true);
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

    // ���ѵ��~���եΪ���k�A�Ω󲾰ʬ��ʮئӤ��v�T�����m
    public void MoveActivityBox(Vector3 newPosition)
    {
        if (activityBox != null && isPaused)
        {
            activityBox.transform.position = newPosition;

            if (debugMode)
            {
                Debug.Log($"���ʮز��ʨ�: {newPosition}");
            }
        }
    }

    // ���ѵ��~���եΪ���k�A�Ω�վ㬡�ʮؤj�p
    public void ResizeActivityBox(Vector3 newScale)
    {
        if (activityBox != null && isPaused)
        {
            // �����Y��d��
            float clampedX = Mathf.Clamp(newScale.x, minBoxSize, maxBoxSize);
            float clampedY = Mathf.Clamp(newScale.y, minBoxSize, maxBoxSize);
            float clampedZ = Mathf.Clamp(newScale.z, minBoxSize, maxBoxSize);

            Vector3 clampedScale = new Vector3(clampedX, clampedY, clampedZ);
            activityBox.transform.localScale = clampedScale;

            if (debugMode)
            {
                Debug.Log($"���ʮ��Y���: {clampedScale}");
            }
        }
    }
}