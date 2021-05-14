using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    public EnemyHydrator enemyHydrator;
    public TextMeshProUGUI healthText;

    private EnemyHydrator.Type enemyType;
    private int hp;
    private float attackTime;

    void Start()
    {
        if (enemyHydrator)
        {
            this.enemyType = enemyHydrator.type;
            this.hp = enemyHydrator.hp;
            this.attackTime = enemyHydrator.attackTime;
        }

        healthText = GameManager.Instance.enemyHealthText;
        healthText.text = hp.ToString();
    }

    public void TakeDamage(int damage)
    {
        if (hp - damage >= 0) 
        {
            hp -= damage;
        } else {
            hp = 0;
        }

        healthText.text = hp.ToString();
        if (hp <= 0) Death();
    }

    private void Death()
    {
        Debug.Log("damn u died");
        GameManager.Instance.LevelComplete();
    }
}
