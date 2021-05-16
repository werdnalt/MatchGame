using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Level", order = 2)]
public class Level : ScriptableObject
{
    public Sprite backgroundImage;
    public Enemy enemy;
    public int levelNumber;
}
