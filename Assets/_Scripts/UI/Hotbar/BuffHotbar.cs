using UnityEngine;
using UnityEngine.UI;
using TMPro;                      // add this
using System.Collections;
using System.Collections.Generic;

public enum BuffType { Speed, AdditionalDamage }

public class BuffHotbar : MonoBehaviour
{
    public static BuffHotbar Instance;
    [Header("Hotbar Settings")]
    public int maxSlots = 5;
    public GameObject buffIconPrefab;      
    public Transform slotContainer;        

    public float flickerThreshold = 3f;
    public float flickerInterval = 0.2f;

    private List<Buff> activeBuffs = new List<Buff>();
    private List<GameObject> buffIcons = new List<GameObject>();

    private int additionalDamageCount = 0;
    private GameObject additionalDamageIcon;
    private TMP_Text additionalDamageText;   // use TextMeshPro

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddBuff(BuffType type, Sprite icon, float duration)
    {
        if (type == BuffType.AdditionalDamage)
        {
            additionalDamageCount++;
            if (additionalDamageIcon == null)
            {
                GameObject go = Instantiate(buffIconPrefab, slotContainer);
                var img = go.GetComponent<Image>();
                img.sprite = icon;
                additionalDamageIcon = go;

                TMP_Text txt = null;        
                foreach (var tf in go.GetComponentsInChildren<Transform>(true))
                {
                    if (tf.name == "CountText")
                    {
                        tf.gameObject.SetActive(true);
                        txt = tf.GetComponent<TMP_Text>();  
                        break;
                    }
                }
                additionalDamageText = txt;
            }
            if (additionalDamageText != null)
                additionalDamageText.text = additionalDamageCount.ToString();
            return;
        }
        
        var existing = activeBuffs.Find(b => b.type == type);
        if (existing != null)
        {
            existing.Reset(duration);
            return;
        }

        if (activeBuffs.Count >= maxSlots)
            RemoveBuff(activeBuffs[0]);

        GameObject go2 = Instantiate(buffIconPrefab, slotContainer);
        Image img2 = go2.GetComponent<Image>();
        img2.sprite = icon;

        foreach (var tf in go2.GetComponentsInChildren<Transform>(true))
        {
            if (tf.name == "CountText")
            {
                tf.gameObject.SetActive(false);
                break;
            }
        }

        buffIcons.Add(go2);

        var newBuff = new Buff(type, icon, duration, this, img2);
        activeBuffs.Add(newBuff);

        if (duration > 0f)
            StartCoroutine(newBuff.Timer());
    }

    public void RemoveBuff(Buff buff)
    {
        int idx = activeBuffs.IndexOf(buff);
        if (idx >= 0)
        {
            activeBuffs.RemoveAt(idx);
            Destroy(buffIcons[idx]);
            buffIcons.RemoveAt(idx);
        }
    }

    public class Buff
    {
        public BuffType type;
        public Sprite icon;
        public float duration;
        private BuffHotbar hotbar;
        private Image iconImage;

        public Buff(BuffType t, Sprite i, float d, BuffHotbar h, Image img)
        {
            type = t; icon = i; duration = d; hotbar = h;
            iconImage = img;
        }

        public IEnumerator Timer()
        {
            float elapsed = 0f;
            float flickerTimer = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                if (duration - elapsed <= hotbar.flickerThreshold && iconImage != null)
                {
                    flickerTimer += Time.deltaTime;
                    if (flickerTimer >= hotbar.flickerInterval)
                    {
                        flickerTimer = 0f;
                        Color col = iconImage.color;
                        col.a = (col.a > 0f ? 0f : 1f);
                        iconImage.color = col;
                    }
                }

                yield return null;
            }

            if (iconImage != null)
            {
                Color col = iconImage.color;
                col.a = 1f;
                iconImage.color = col;
            }

            hotbar.RemoveBuff(this);
        }

        public void Reset(float newDuration) { duration = newDuration; }
    }
}
