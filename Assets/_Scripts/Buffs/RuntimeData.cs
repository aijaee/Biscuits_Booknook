using UnityEngine;
using UnityEngine.SceneManagement;

public class RuntimeData : MonoBehaviour
{
    private static RuntimeData _instance;
    public static RuntimeData Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("RuntimeData");
                _instance = go.AddComponent<RuntimeData>();
            }
            return _instance;
        }
    }

    private float persistentHealth = -1f;
    private int additionalDamageCount = 0;
    private Sprite additionalDamageIcon;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void ResetRuntimeData()
    {
        persistentHealth = -1f;
        additionalDamageCount = 0;
        additionalDamageIcon = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // when the first scene (buildIndex 0) loads, treat as a full restart
        if (scene.buildIndex == 0)
        {
            ResetRuntimeData();
            return;
        }

        // reapply player health
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && persistentHealth >= 0f)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.currentHealth = persistentHealth;
                pc.UpdateHPBar();
            }
        }

    }

    public float PersistentHealth => persistentHealth;
    public void SetPersistentHealth(float h) => persistentHealth = h;

    public void AddAdditionalDamageBuff(Sprite icon)
    {
        additionalDamageCount++;
        additionalDamageIcon = icon;
    }
    public int PersistentAdditionalDamageCount => additionalDamageCount;
    public Sprite PersistentAdditionalDamageIcon => additionalDamageIcon;
}