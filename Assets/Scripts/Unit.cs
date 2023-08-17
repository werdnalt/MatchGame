using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "ScriptableObjects/Unit", order = 1)]
public class Unit : ScriptableObject
{
    private void Start()
    {
        
    }
    
    private void Update()
    {


    }
    
    public enum Tribe
    {
        Red,
        Blue,
        Green,
        Yellow
    }

    public enum Classification
    {
        Rock,
        Paper,
        Scissors
    }

    private void AnimateSprite()
    {
        
    }

    public Tribe tribe;
    public Classification classification;
    public int hp;
    public int attack;
    public Sprite unitSprite;


}
