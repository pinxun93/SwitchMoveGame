using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementConstraint : MonoBehaviour
{
    public Transform maskObject;     // Mask遮罩物件
    public Camera uiCamera;          // UI相機 (如果Mask是UI元素)

    void LateUpdate()
    {
        if (maskObject == null)
            return;

        Bounds maskBounds;

        // 根據Mask的類型獲取相應的邊界
        RectTransform rectTransform = maskObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // UI元素的Mask
            maskBounds = GetRectTransformBounds(rectTransform, uiCamera);
        }
        else if (maskObject.GetComponent<Renderer>() != null)
        {
            // 有渲染器的Mask
            maskBounds = maskObject.GetComponent<Renderer>().bounds;
        }
        else if (maskObject.GetComponent<Collider>() != null)
        {
            // 有碰撞體的Mask
            maskBounds = maskObject.GetComponent<Collider>().bounds;
        }
        else
        {
            // 其他情況，嘗試根據Transform估算
            float width = maskObject.localScale.x;
            float height = maskObject.localScale.y;
            Vector3 center = maskObject.position;
            maskBounds = new Bounds(center, new Vector3(width, height, 1f));
        }

        // 獲取角色當前位置
        Vector3 characterPosition = transform.position;

        // 限制角色在Mask範圍內
        float clampedX = Mathf.Clamp(characterPosition.x, maskBounds.min.x, maskBounds.max.x);
        float clampedY = Mathf.Clamp(characterPosition.y, maskBounds.min.y, maskBounds.max.y);

        // 應用限制後的位置
        transform.position = new Vector3(clampedX, clampedY, characterPosition.z);
    }

    // 獲取RectTransform在世界座標中的邊界
    private Bounds GetRectTransformBounds(RectTransform rectTransform, Camera cam)
    {
        // 獲取RectTransform的四個角點
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // 如果提供了相機，則將UI坐標轉換為世界坐標
        if (cam != null && cam != Camera.main)
        {
            for (int i = 0; i < 4; i++)
            {
                // 將UI世界坐標轉換為屏幕坐標
                corners[i] = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                // 將屏幕坐標轉換為遊戲世界坐標
                corners[i] = Camera.main.ScreenToWorldPoint(new Vector3(corners[i].x, corners[i].y, 0));
            }
        }

        // 計算邊界
        Bounds bounds = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < 4; i++)
        {
            bounds.Encapsulate(corners[i]);
        }

        return bounds;
    }
}