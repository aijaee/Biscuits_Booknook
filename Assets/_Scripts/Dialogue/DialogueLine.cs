using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public Sprite portraitSprite;
    public RuntimeAnimatorController animatorController;
    public Sprite nameImage;
    [TextArea(2, 6)]
    public string text;
    public bool walkIn;
    public bool walkOut;
}