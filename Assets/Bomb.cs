using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bomb : Block
{
    public SpriteRenderer countdown;
    public SpriteRenderer background;
    
    public List<Sprite> countdownSprites;
    public int bombLifetime;
    public BombType bombType;

    private float endTime;
    private float timeLeft;
    private int _belongsToPlayerIndex;
    private BoardManager.Coordinates _coordinates;
    public enum BombType
    {
        Landmine,
        Round,
        Atomic
    }

    public void Explode()
    {
        switch (bombType)
        {
            case BombType.Round:
                List<BoardManager.Coordinates> neighboringCoords =
                    BoardManager.Instance.GetNeighboringCoordinates(_coordinates, BoardManager.BlockLayout.Surrounding);
                foreach (var coords in neighboringCoords)
                {
                    if (!coords.Equals(_coordinates))
                    {
                        // play animation
                        BoardManager.Instance.SpawnSmoke(coords, _belongsToPlayerIndex);
                        
                        // deal damage if player is there
                        Player p = BoardManager.Instance.GetPlayerFromPosition(coords);
                        if (p)
                        {
                            p.TakeDamage(1);
                        }
                    }
                }
                break;
            
            case BombType.Atomic:
                List<BoardManager.Coordinates> crossingCoords =
                    BoardManager.Instance.GetNeighboringCoordinates(_coordinates, BoardManager.BlockLayout.Crossing);
                foreach (var coords in crossingCoords)
                {
                    if (!coords.Equals(_coordinates))
                    {
                        // play animation
                        BoardManager.Instance.SpawnSmoke(coords, _belongsToPlayerIndex);
                        
                        // deal damage if player is there
                        Player p = BoardManager.Instance.GetPlayerFromPosition(coords);
                        if (p)
                        {
                            p.TakeDamage(2);
                        }
                    }
                }
                break;
        }
        
        BoardManager.Instance.ReplaceWithRandomBlock(_coordinates);
        Destroy(gameObject);
    }

    private void Update()
    {
        endTime = timeSpawned + bombLifetime;
        timeLeft = endTime - Time.time;

        if (timeLeft <= 5 && timeLeft > 0)
        {
            countdown.sprite = countdownSprites[(int) timeLeft];
        }

        if (timeLeft <= 0)
        {
            timeLeft = 1000;
            Explode();
        }
    }

    public void Setup(BombType type, int playerIndex, BoardManager.Coordinates coordinates, int lifetime)
    {
        _belongsToPlayerIndex = playerIndex;
        _coordinates = coordinates;
        background.sprite = Resources.Load<Sprite>(playerIndex + "bomb_background");
        bombType = type;
        _blockIcon.sprite = Resources.Load<Sprite>(type.ToString());
        bombLifetime = lifetime;
    }
}
