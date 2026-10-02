using System;
using UnityEngine;
using Utility;

namespace Game
{
    public enum Speed
    {
        Slow,
        Normal,
        Fast
    }
    
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Time Speeds")]
        [SerializeField] private float slowTime = 0.2f;
        [SerializeField] private float normalTime = 1f;
        [SerializeField] private float fastTime = 1.5f;

        private SimpleTimer _timer;

        public TimerTime CurrentTime => _timer.ReadTime();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start()
        {
            _timer = new SimpleTimer(TimerType.Stopwatch);
            _timer.StartTimer();
        }

        public void SlowTime(Speed speed)
        {
            Time.timeScale = speed switch
            {
                Speed.Slow => slowTime,
                Speed.Normal => normalTime,
                Speed.Fast => fastTime,
                _ => throw new ArgumentOutOfRangeException(nameof(speed), speed, null)
            };
        }
    }
}