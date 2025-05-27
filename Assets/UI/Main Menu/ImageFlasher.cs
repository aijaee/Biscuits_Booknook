using UnityEngine;
using UnityEngine.UI;

public class ImageFlasher : MonoBehaviour
{
    public Image targetImage;
    public float flashSpeed = 2f; 

    private void Reset()
    {
        targetImage = GetComponent<Image>();
    }

    void Update()
    {
        if (targetImage == null) return;

        float alpha = Mathf.PingPong(Time.time * flashSpeed, 1f);
        Color c = targetImage.color;
        c.a = alpha;
        targetImage.color = c;
    }
}