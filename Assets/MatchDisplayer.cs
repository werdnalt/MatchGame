using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchDisplayer : MonoBehaviour
{
    public Image BlockIcon;
    public TextMeshProUGUI effectText;

    public void Setup(Sprite icon, string text)
    {
        BlockIcon.sprite = icon;
        effectText.text = text;
    }
}
