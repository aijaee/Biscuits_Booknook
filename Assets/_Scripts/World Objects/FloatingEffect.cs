using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public float floatStrength = 0.25f;
    public float floatSpeed = 2f;
    public Transform shadow;

    private Vector3 startPos;
    private float randomOffset;

    void Start()
    {
        startPos = transform.localPosition;

        randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float sine = Mathf.Sin(Time.time * floatSpeed + randomOffset);
        float newY = startPos.y + sine * floatStrength;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        if (shadow != null)
        {
            float scale = 1f - (sine * 0.1f);
            shadow.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = shadow.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(0.6f, 0.9f, 1f - (sine + 1f) / 2f);
                sr.color = c;
            }
        }
    }
}
