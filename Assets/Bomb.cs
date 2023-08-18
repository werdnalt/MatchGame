using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bomb : UnitBehaviour
{
    public SpriteRenderer countdown;
    public SpriteRenderer background;
    
    public List<Sprite> countdownSprites;
    public int bombLifetime;
    public BombType bombType;

    private float endTime;
    private float timeLeft;
    public int belongsToPlayerIndex;
    private float _pulseTime = .5f;
    private float _timeOfPulseSwitch;
    private BoardManager.Coordinates _coordinates;
    public enum BombType
    {
        Landmine,
        Round,
        Atomic
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
        }
    }

    // public void Setup(BombType type, int playerIndex, BoardManager.Coordinates coordinates, int lifetime)
    // {
    //     belongsToPlayerIndex = playerIndex;
    //     _coordinates = coordinates;
    //     background.sprite = Resources.Load<Sprite>(playerIndex + "bomb_background");
    //     bombType = type;
    //     _blockIcon.sprite = Resources.Load<Sprite>(type.ToString());
    //     bombLifetime = lifetime;
    //     Pulse();
    // }

    // private void Pulse()
    // {
    //     StartCoroutine(IPulseIn());
    // }

    // private IEnumerator IPulseIn()
    // {
    //     _timeOfPulseSwitch = Time.time;
    //     float elapsedTime = 0;
    //     Transform blockTransform = _blockIcon.transform;
    //     
    //     Vector3 originalScale = _blockIcon.transform.localScale;
    //     Vector3 targetScale = new Vector3(originalScale.x / 3, originalScale.y / 3, originalScale.z / 3);
    //     
    //     while (elapsedTime < _pulseTime)
    //     {
    //         blockTransform.localScale = Vector3.Lerp(originalScale, targetScale, (elapsedTime / _pulseTime));
    //         elapsedTime += Time.deltaTime;
    //
    //         // Yield here
    //         yield return null;
    //     }  
    //     // Make sure we got there
    //     blockTransform.localScale = targetScale;
    //     StartCoroutine(IPulseOut());
    //     yield return null;
    // }
    
    // private IEnumerator IPulseOut()
    // {
    //     _timeOfPulseSwitch = Time.time;
    //     float elapsedTime = 0;
    //     // Transform blockTransform = _blockIcon.transform;
    //     //
    //     // Vector3 originalScale = _blockIcon.transform.localScale;
    //     Vector3 targetScale = new Vector3(originalScale.x * 3, originalScale.y * 3, originalScale.z * 3);
    //     
    //     while (elapsedTime < _pulseTime)
    //     {
    //         blockTransform.localScale = Vector3.Lerp(originalScale, targetScale, (elapsedTime / _pulseTime));
    //         elapsedTime += Time.deltaTime;
    //
    //         // Yield here
    //         yield return null;
    //     }  
    //     // Make sure we got there
    //     blockTransform.localScale = targetScale;
    //     //StartCoroutine(IPulseIn());
    //     yield return null;
    // }
}
