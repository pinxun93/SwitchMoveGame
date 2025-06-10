using UnityEngine;

public class MovingPlatformHandler : MonoBehaviour
{
    [Header("Debug�]�m")]
    public bool debugMode = false;

    private GameManager gameManager;
    private GameObject currentPlayer;
    private Vector3 playerPositionBeforePause;
    private bool wasPlayerOnPlatform = false;
    private bool hasSavedPlayerPosition = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null && debugMode)
            Debug.LogWarning("MovingPlatformHandler: �䤣�� GameManager�I");
    }

    private void Update()
    {
        if (gameManager != null)
        {
            // �C����Ȱ��ɡA�O�s���a��m�øѰ����Y
            if (gameManager.IsPaused && !hasSavedPlayerPosition && currentPlayer != null)
            {
                SavePlayerPositionAndDetach();
            }
            // �C���Ȱ������A����j��a�^��O�s����m
            else if (gameManager.IsPaused && hasSavedPlayerPosition && currentPlayer != null)
            {
                ForcePlayerToSavedPosition();
            }
            // �C����_�ɡA���m���A
            else if (!gameManager.IsPaused && hasSavedPlayerPosition)
            {
                ResumePlayerMovement();
            }
        }
    }

    private void SavePlayerPositionAndDetach()
    {
        if (currentPlayer != null)
        {
            playerPositionBeforePause = currentPlayer.transform.position;
            hasSavedPlayerPosition = true;

            // �j��Ѱ����l���Y
            if (currentPlayer.transform.parent == transform)
            {
                currentPlayer.transform.SetParent(null);
            }

            // ����a�����z�B��
            Rigidbody2D playerRb = currentPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.isKinematic = true; // �Ȱ��������Ϊ��z
            }

            if (debugMode)
                Debug.Log($"�Ȱ��ɫO�s���a��m: {playerPositionBeforePause}�A�øѰ����z");
        }
    }

    private void ForcePlayerToSavedPosition()
    {
        if (currentPlayer != null)
        {
            // �j��a�O���b�O�s����m
            currentPlayer.transform.position = playerPositionBeforePause;

            // �T�O�S�����l���Y
            if (currentPlayer.transform.parent != null)
            {
                currentPlayer.transform.SetParent(null);
            }

            // �T�O�t�׬��s
            Rigidbody2D playerRb = currentPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
            }
        }
    }

    private void ResumePlayerMovement()
    {
        if (currentPlayer != null)
        {
            // ��_���a�����z
            Rigidbody2D playerRb = currentPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
            }

            // �p�G���a���b���xĲ�o�����A���s�إߤ��l���Y
            Collider2D platformCollider = GetComponent<Collider2D>();
            Collider2D playerCollider = currentPlayer.GetComponent<Collider2D>();

            if (platformCollider != null && playerCollider != null &&
                platformCollider.bounds.Intersects(playerCollider.bounds))
            {
                currentPlayer.transform.SetParent(transform);
                wasPlayerOnPlatform = true;
            }

            if (debugMode)
                Debug.Log("�C����_�A���s�ҥΪ��a���z");
        }

        hasSavedPlayerPosition = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.gameObject;

            // �u���b�C�����Ȱ��ɤ~�إߤ��l���Y
            if (gameManager == null || !gameManager.IsPaused)
            {
                other.transform.SetParent(transform);
                wasPlayerOnPlatform = true;

                if (debugMode)
                    Debug.Log($"Player �i�J {gameObject.name}�A�إߤ��l���Y");
            }
            else if (debugMode)
            {
                Debug.Log($"Player �i�J {gameObject.name}�A���C���Ȱ����A���إߤ��l���Y");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.gameObject;

            // �p�G�C���Ȱ��A�T�O���a���Q���
            if (gameManager != null && gameManager.IsPaused)
            {
                if (other.transform.parent == transform)
                {
                    other.transform.SetParent(null);
                }

                // �p�G�w�O�s��m�A�j��^��Ӧ�m
                if (hasSavedPlayerPosition)
                {
                    other.transform.position = playerPositionBeforePause;
                }
            }
            // �C����_�B���a���b���x�W�ɭ��s�إ����Y
            else if (gameManager != null && !gameManager.IsPaused &&
                     other.transform.parent != transform && wasPlayerOnPlatform)
            {
                other.transform.SetParent(transform);
                if (debugMode)
                    Debug.Log($"�C����_�A���s�إ� Player �P {gameObject.name} �����l���Y");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ���a���}�ɸѰ��Ҧ����Y�M�O�s�����A
            if (other.transform.parent == transform)
            {
                other.transform.SetParent(null);
            }

            // ��_���a���z�]�p�G�Q���Ρ^
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.isKinematic)
            {
                playerRb.isKinematic = false;
            }

            wasPlayerOnPlatform = false;
            hasSavedPlayerPosition = false;

            if (currentPlayer == other.gameObject)
                currentPlayer = null;

            if (debugMode)
                Debug.Log($"Player ���} {gameObject.name}�A�M�z�Ҧ����A");
        }
    }

    // ���}��k�A����L�}���i�H�j��Ѱ����a���Y
    public void ForceDetachAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.transform.parent == transform)
            {
                player.transform.SetParent(null);
            }

            // ��_���z
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.isKinematic)
            {
                playerRb.isKinematic = false;
            }
        }

        currentPlayer = null;
        wasPlayerOnPlatform = false;
        hasSavedPlayerPosition = false;

        if (debugMode)
            Debug.Log($"�j��M�z {gameObject.name} ���Ҧ����a���Y");
    }

    private void OnDisable()
    {
        ForceDetachAllPlayers();
    }
}