using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level: MonoBehaviour
{
    public LevelData levelData;
    public bool isUnlocked;
    public bool isComplete;

    private Vector3 _originalScale;

    public void LoadLevel()
    {
        GameManager.Instance.levelData = levelData;
        SceneManager.LoadScene("PlayScene");
    }

    public void ChooseLevel()
    {
        
    }

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void Grow()
    {
        var newScale = new Vector3(_originalScale.x * 1.2f, _originalScale.y *1.2f, _originalScale.z);
        transform.DOKill();
        transform.DOScale(newScale, .2f).SetEase(Ease.OutBack);
    }

    public void Shrink()
    {
        transform.DOScale(_originalScale, .2f);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
