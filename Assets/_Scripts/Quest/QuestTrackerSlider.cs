using UnityEngine;

public class QuestTrackerSlider : MonoBehaviour
{
    [Header("Slide Settings")]
    public RectTransform trackerRect;
    public Vector2 hiddenPosition;
    public Vector2 shownPosition;
    public float slideSpeed = 500f;
    public KeyCode toggleKey = KeyCode.Tab;

    private bool isShown = false;

    private void Start()
    {
        if (trackerRect == null)
            trackerRect = GetComponent<RectTransform>();

        trackerRect.anchoredPosition = hiddenPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isShown = !isShown;
        }

        Vector2 target = isShown ? shownPosition : hiddenPosition;
        trackerRect.anchoredPosition = Vector2.MoveTowards(
            trackerRect.anchoredPosition,
            target,
            slideSpeed * Time.deltaTime
        );
    }
}