using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomEventManager : MonoBehaviour
{
    public GameObject wingsBlock;
    public GameObject bombBlock;
    public GameObject timerBlock;

    private float _timeBetweenEvents;
    private float _timeOfLastEvent;
    private List<EventType> _eventTypes = new List<EventType>();
    public enum EventType
    {
        Wings,
        Bombs,
        Timer
    }

    private void Start()
    {
        _timeOfLastEvent = Time.time;
        _timeBetweenEvents = PlayerPrefs.HasKey("EventTime") ? PlayerPrefs.GetInt("EventTime") : 10000;
        
        _eventTypes.Add(EventType.Wings);
        _eventTypes.Add(EventType.Bombs);
        _eventTypes.Add(EventType.Timer);
    }

    private void Update()
    {
        CheckForRandomEvent();
    }

    private void CheckForRandomEvent()
    {
        if (Time.time - _timeOfLastEvent >= _timeBetweenEvents)
        {
            EventType eventType = ChooseRandomEvent();
            HandleRandomEvent(eventType);
        }
    }

    private EventType ChooseRandomEvent()
    {
        return _eventTypes[Random.Range(0, _eventTypes.Count)];
    }

    private void HandleRandomEvent(EventType eventType)
    {
        switch (eventType)
        {
            case EventType.Bombs:
                int numBombs = 5;
                List<BoardManager.Coordinates> bombCoords = BoardManager.Instance.GetRandomCoordinates(numBombs);
                foreach (var coordinates in bombCoords)
                {
                    BoardManager.Instance.ReplaceBlock(coordinates, bombBlock);
                }
                break;
            
            case EventType.Timer:
                int numTimers = 3;
                List<BoardManager.Coordinates> timerCoords = BoardManager.Instance.GetRandomCoordinates(numTimers);
                foreach (var coordinates in timerCoords)
                {
                    BoardManager.Instance.ReplaceBlock(coordinates, wingsBlock);
                }
                break;
            
            case EventType.Wings:
                int numWings = 3;
                List<BoardManager.Coordinates> wingCoords = BoardManager.Instance.GetRandomCoordinates(numWings);
                foreach (var coordinates in wingCoords)
                {
                    BoardManager.Instance.ReplaceBlock(coordinates, wingsBlock);
                }
                break;
        }

        _timeOfLastEvent = Time.time;
    }
}
