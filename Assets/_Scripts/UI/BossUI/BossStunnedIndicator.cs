using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BossStunnedIndicator : MonoBehaviour
{
    [Header("Assign in Inspector or Auto-Find")]
    public BossStatsMovement bossCtrl;     
    public Camera mainCamera;           
    public Canvas canvas;              
    public RectTransform arrowUI;         
    public float distanceFromCenter = 100f;

    private Vector2 screenCenter;

    void Awake()
    {
        if (bossCtrl    == null) bossCtrl    = FindObjectOfType<BossStatsMovement>();
        if (mainCamera  == null) mainCamera  = Camera.main;
        if (canvas      == null) canvas      = GameObject.Find("GameUI Canvas")?.GetComponent<Canvas>() 
                                             ?? FindObjectOfType<Canvas>();
        if (arrowUI     != null) arrowUI.gameObject.SetActive(false);

        screenCenter = new Vector2(Screen.width, Screen.height) * 0.5f;
    }

    void Update()
    {
        if (bossCtrl == null || arrowUI == null)
            return;

        bool isStunned = bossCtrl.CurrentState == BossStatsMovement.BossState.Stunned;
        arrowUI.gameObject.SetActive(isStunned);
        if (!isStunned) return;

        Vector3 bossScreen = mainCamera.WorldToScreenPoint(bossCtrl.transform.position);

        if (bossScreen.z < 0)
        {
            bossScreen.x = screenCenter.x * 2 - bossScreen.x;
            bossScreen.y = screenCenter.y * 2 - bossScreen.y;
        }

        Vector2 dir = ((Vector2)bossScreen - screenCenter).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arrowUI.localRotation = Quaternion.Euler(0, 0, angle);
        arrowUI.anchoredPosition = dir * distanceFromCenter;
    }
}
