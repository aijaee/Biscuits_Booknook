using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterData
{
    public string characterName;
    public Sprite idleSprite;
    public RuntimeAnimatorController animatorController;
}

public class PortraitController : MonoBehaviour
{
    public Image portraitImage;
    public Animator portraitAnimator;
    public RectTransform portraitRect;
    public Vector2 defaultPosition;

    public void SetCharacter(Sprite portrait, RuntimeAnimatorController animator)
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }

        if (portraitAnimator != null)
            portraitAnimator.runtimeAnimatorController = animator;
    }

    public void SetTalking(bool talking)
    {
        if (portraitAnimator != null && portraitAnimator.runtimeAnimatorController != null)
            portraitAnimator.SetBool("isTalking", talking);
    }
}