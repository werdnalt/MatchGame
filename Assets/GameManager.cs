using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<Enemy> enemies = new List<Enemy>();
    public Enemy currentEnemy;
    public TextMeshProUGUI enemyHealthText;
    public GameObject enemySpawn;
    public GameObject LevelIntermissionUI;

    private int currentLevel;
    
    void Start()
    {
        if (Instance == null) Instance = this;
        if (enemies.Count >= 1) currentEnemy = Instantiate(enemies[0], transform.position, Quaternion.identity, enemySpawn.transform);

        currentLevel = 1;

        DontDestroyOnLoad(this.gameObject);
    }

    public void LevelComplete()
    {
        LevelIntermissionUI.gameObject.SetActive(true);
    }

}
