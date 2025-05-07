using UnityEngine;
using UnityEngine.UI;


public class UIActivityControl : MonoBehaviour
{
    public Transform activityBox;  // ©ì¶i§Aªº ActivityBox
    public Slider xSlider;
    public Slider ySlider;

    void Update()
    {
        if (activityBox != null && xSlider != null && ySlider != null)
        {
            activityBox.position = new Vector3(xSlider.value, ySlider.value, 0f);
        }
    }
}
