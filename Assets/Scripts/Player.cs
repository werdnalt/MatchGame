using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public static Player Instance;
    
    public int maxHP;
    public int score;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI scoreText;

    private int currHP;

    void Start()
    {
        EventManager.Instance.onNextLevel += Reset;

        currHP = maxHP;
        if (Instance == null) Instance = this;

        hpText.text = currHP.ToString();
        score = 0;
    }

    public void TakeDamage(int damage)
    {
        if (currHP - damage >= 0) 
        {
            currHP -= damage;
        } else {
            currHP = 0;
        }

        hpText.text = currHP.ToString();
    }

    public void UpdateScore(int score)
    {
        this.score += score;
        scoreText.text = score.ToString();
    }

    public void Reset(int level)
    {
        currHP = maxHP;
    }

}
