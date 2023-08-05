using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTimeManager : MonoBehaviour
{
    public static AttackTimeManager instance;
    private float timeOfLastAttack;
    public float secondsBetweenAttacks = 3;

    // A number on a scale of 0 to 1 that represents the percentage of time that has elapsed since the last attack
    public float attackPercentage => Mathf.Min((Time.time - timeOfLastAttack) / secondsBetweenAttacks, 1);


    public delegate void AttackTrigger();
    public event AttackTrigger attackTriggerListeners;

    void Awake()
    {
        if (instance == null) instance = this;
        
        StartTimer();
    }

    public void StartTimer()
    {
        timeOfLastAttack = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (attackPercentage == 1)
        {
            attackTriggerListeners.Invoke();
            timeOfLastAttack = Time.time;
        }
    }
}
