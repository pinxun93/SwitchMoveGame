using UnityEngine;
using UnityEngine.UI;


public class UIActivityControl : MonoBehaviour
{
    public Transform activityBox;
    public Slider widthSlider;
    public Slider heightSlider;

    void Update()
    {
        if (Time.timeScale == 0 && activityBox != null)
        {
            float width = widthSlider != null ? widthSlider.value : activityBox.localScale.x;
            float height = heightSlider != null ? heightSlider.value : activityBox.localScale.y;

            activityBox.localScale = new Vector3(width, height, 1f); // 只改大小，不改位置
        }
    }
}
