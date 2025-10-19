using UnityEngine;
using System.Collections;

public class BossDeathSequenceController : MonoBehaviour
{
    private static BossDeathSequenceController instance;  

    [Header("References")]
    public BossStatsMovement boss;                   
    public BossDefeatedPortalSpawner portalSpawner;    
    public PlayerController playerController;          
    public Rigidbody2D playerRb;                     
    public Camera mainCamera;                   

    [Header("Sequence Settings")]
    public float waitForPortalTimeout = 5f;  
    public float cameraPanDuration   = 1f;    
    public float pauseDuration       = 0.5f;  

    [Header("Options")]
    [SerializeField] private bool triggerOnEnable = false;  

    [Header("Pan Settings")]    
    public float panSmoothSpeed = 5f;  

    private bool sequenceStarted = false;
    private bool sequenceCompleted = false;  
    private RigidbodyConstraints2D originalPlayerConstraints;  
    private CameraFollow cameraFollow;  

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;


        if (boss == null)
            boss = FindObjectOfType<BossStatsMovement>();

        if (portalSpawner == null)
            portalSpawner = FindObjectOfType<BossDefeatedPortalSpawner>();
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            if (playerController == null)
                playerController = player.GetComponent<PlayerController>();
            if (playerRb == null)
                playerRb = player.GetComponent<Rigidbody2D>();
        }

        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera != null)
            cameraFollow = mainCamera.GetComponent<CameraFollow>();  

        if (playerRb != null)
            originalPlayerConstraints = playerRb.constraints;   
    }

    private void OnEnable()
    {
   
        if (this == instance && triggerOnEnable)
            TriggerSequence();
    }

    void Update()
    {
        if (gameObject.CompareTag("Portal")) return;
        if (sequenceStarted) return;
        if (triggerOnEnable) return;                     

        if (boss == null || boss.currentHealth <= 0f)
        {
            sequenceStarted = true;
            StartCoroutine(HandleDeathSequence());
        }
    }

    public void TriggerSequence()
    {
        if (this != instance || sequenceStarted || sequenceCompleted) return; 
        sequenceStarted = true;
        StartCoroutine(HandleDeathSequence());
    }

    private IEnumerator HandleDeathSequence()
    {
        if (sequenceCompleted)   
            yield break;

        if (playerController != null)
            playerController.enabled = false;
        if (playerRb != null)
            playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
        if (cameraFollow != null)                       
            cameraFollow.enabled = false;


        GameObject portal = null;
        float timer = 0f;
        while (portal == null && timer < waitForPortalTimeout)
        {
            portal = GameObject.FindWithTag("Portal");
            timer += Time.deltaTime;
            yield return null;
        }
        if (portal == null)
        {
            RestorePlayer();
            yield break;
        }

        var origPos   = mainCamera.transform.position;
        var portalPos = new Vector3(portal.transform.position.x, portal.transform.position.y, origPos.z);

        // pan to portal and back
        yield return PanCamera(origPos, portalPos);
        yield return new WaitForSeconds(pauseDuration);
        yield return PanCamera(portalPos, origPos);

        RestorePlayer();
        sequenceCompleted = true;       
        enabled = false;
    }

    private IEnumerator PanCamera(Vector3 start, Vector3 end)
    {
        while (Vector3.Distance(mainCamera.transform.position, end) > 0.01f)
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                end,
                panSmoothSpeed * Time.deltaTime
            );
            yield return null;
        }
        mainCamera.transform.position = end;
    }

    private void RestorePlayer()
    {
        if (playerController != null)
            playerController.enabled = true;
        if (playerRb != null)
            playerRb.constraints = originalPlayerConstraints;
        if (cameraFollow != null)                                 
            cameraFollow.enabled = true;
    }
}
