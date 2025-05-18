using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementConstraint : MonoBehaviour
{
    public Transform maskObject;     // Mask�B�n����
    public Camera uiCamera;          // UI�۾� (�p�GMask�OUI����)

    void LateUpdate()
    {
        if (maskObject == null)
            return;

        Bounds maskBounds;

        // �ھ�Mask������������������
        RectTransform rectTransform = maskObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // UI������Mask
            maskBounds = GetRectTransformBounds(rectTransform, uiCamera);
        }
        else if (maskObject.GetComponent<Renderer>() != null)
        {
            // ����V����Mask
            maskBounds = maskObject.GetComponent<Renderer>().bounds;
        }
        else if (maskObject.GetComponent<Collider>() != null)
        {
            // ���I���骺Mask
            maskBounds = maskObject.GetComponent<Collider>().bounds;
        }
        else
        {
            // ��L���p�A���ծھ�Transform����
            float width = maskObject.localScale.x;
            float height = maskObject.localScale.y;
            Vector3 center = maskObject.position;
            maskBounds = new Bounds(center, new Vector3(width, height, 1f));
        }

        // ��������e��m
        Vector3 characterPosition = transform.position;

        // �����bMask�d��
        float clampedX = Mathf.Clamp(characterPosition.x, maskBounds.min.x, maskBounds.max.x);
        float clampedY = Mathf.Clamp(characterPosition.y, maskBounds.min.y, maskBounds.max.y);

        // ���έ���᪺��m
        transform.position = new Vector3(clampedX, clampedY, characterPosition.z);
    }

    // ���RectTransform�b�@�ɮy�Ф������
    private Bounds GetRectTransformBounds(RectTransform rectTransform, Camera cam)
    {
        // ���RectTransform���|�Ө��I
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // �p�G���ѤF�۾��A�h�NUI�����ഫ���@�ɧ���
        if (cam != null && cam != Camera.main)
        {
            for (int i = 0; i < 4; i++)
            {
                // �NUI�@�ɧ����ഫ���̹�����
                corners[i] = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                // �N�̹������ഫ���C���@�ɧ���
                corners[i] = Camera.main.ScreenToWorldPoint(new Vector3(corners[i].x, corners[i].y, 0));
            }
        }

        // �p�����
        Bounds bounds = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < 4; i++)
        {
            bounds.Encapsulate(corners[i]);
        }

        return bounds;
    }
}