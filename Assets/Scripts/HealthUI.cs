using System;
using System.Collections;
using System.Collections.Generic;
using AllIn1SpringsToolkit;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private GameObject heartObject;
    [SerializeField] private Image heartImage;
    [SerializeField] private TextMeshProUGUI healthAmountText;
    [SerializeField] private TransformSpringComponent transformSpringComponent;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = heartObject.transform.localScale;
    }

    public virtual void ShowAndUpdateHealth(int currentHp, int maxHp)
    {
        heartObject.SetActive(true);
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        healthAmountText.text = currentHp.ToString();
        heartImage.fillAmount = ((float)currentHp / maxHp);
        
        var punchScale = new Vector3(_originalScale.x * 1.25f, _originalScale.y * 1.25f, _originalScale.z * 1.25f);
        transformSpringComponent.SetCurrentValueScale(punchScale);
    }

    public void HideHealth()
    {
        heartObject.SetActive(false);
    }
}
