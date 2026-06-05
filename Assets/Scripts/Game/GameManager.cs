using System;
using System.Timers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private SimpleTimer _timer;

    public TimerTime CurrentTime => _timer.ReadTime();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        _timer = new SimpleTimer(TimerType.Stopwatch);
        _timer.StartTimer();
    }
}