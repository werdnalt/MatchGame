using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Responsible for holding references to UI elements in the level that the enemy + player
// will plug into.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public List<Level> levels = new List<Level>();

    public Image backgroundImage;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI scoreText;
    public Enemy currentEnemy;
    public Vector2 playerSpawnPoint;

    private Vector2 enemySpawnPoint;
    private Dictionary<int, Level> levelDict = new Dictionary<int, Level>();
    private Level currLevel;

    void Start()
    {
        enemyHealthText = GameObject.FindGameObjectWithTag("Enemy_Health").GetComponent<TextMeshProUGUI>();
        playerHealthText = GameObject.FindGameObjectWithTag("Player_Health").GetComponent<TextMeshProUGUI>();
        comboText = GameObject.FindGameObjectWithTag("Combo_Text").GetComponent<TextMeshProUGUI>();
        comboText.gameObject.SetActive(false);
        scoreText = GameObject.FindGameObjectWithTag("Score_Text").GetComponent<TextMeshProUGUI>();
        playerSpawnPoint = GameObject.FindGameObjectWithTag("Player_Spawn").transform.position;
        enemySpawnPoint = GameObject.FindGameObjectWithTag("Enemy_Spawn").transform.position;
        backgroundImage = GameObject.FindGameObjectWithTag("Background").GetComponent<Image>();

        if (Instance == null) Instance = this;

        foreach(Level level in levels)
        {
            int index = level.levelNumber;
            levelDict.Add(level.levelNumber, level);
        }
        Setup();
    }

    // Setup the background image and enemy
    public void Setup()
    {
        if (levelDict.ContainsKey(GameManager.Instance.currentLevel))
        {
            currLevel = levelDict[GameManager.Instance.currentLevel];
        } else 
        {
            Debug.Log("No level loaded in for level " + GameManager.Instance.currentLevel);
        }
        backgroundImage.sprite = currLevel.backgroundImage;
        currentEnemy = Instantiate(currLevel.enemy, enemySpawnPoint, Quaternion.identity);
    }
}
