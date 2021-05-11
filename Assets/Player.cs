using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public static Player Instance;
    
    public int hp;
    public int score;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI scoreText;

    void Start()
    {
        if (Instance == null) Instance = this;

        hpText.text = hp.ToString();
        score = 0;
    }

    public void TakeDamage(int damage)
    {
        if (hp - damage >= 0) 
        {
            hp -= damage;
        } else {
            hp = 0;
        }

        hpText.text = hp.ToString();
    }

    public void UpdateScore(int score)
    {
        this.score += score;
        scoreText.text = score.ToString();
    }

}
