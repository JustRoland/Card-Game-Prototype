using System;
using Sirenix.OdinInspector;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private GameObject gameOverUI;

        [Header("Time Speeds")]
        [SerializeField] private float slowTime = 0.2f;
        [SerializeField] private float normalTime = 1f;
        [SerializeField] private float fastTime = 1.5f;

        private SimpleTimer _timer;
        private SceneChanger _sceneChanger;
        
        public int Round { get; private set; }
        public TimerTime CurrentTime => _timer.ReadTime();
        public int KillCount { get; private set; }
        public int DamageDealt { get; private set; }
        public int DamageTaken { get; private set; }
        public int Revives { get; private set; }
        public string Killer { get; private set; }
        

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            _sceneChanger = GetComponent<SceneChanger>();
        
            gameOverUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void FixedUpdate()
        {
            if (roundText && EnemyManager.Instance) roundText.text = EnemyManager.Instance.Round.ToString();
            _timer.UpdateTimer(Time.deltaTime);
        }

        private void Start()
        {
            _timer = new SimpleTimer(TimerType.Stopwatch);
            _timer.SetTimer(0,0,0);
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
                
        public void CountKill() => KillCount++;
        public void ResetKillCount() => KillCount = 0;
        public void ResetTimer() => _timer.ResetTimer();
        public void TrackDamageDealt(int damage) => DamageDealt += damage;
        public void TrackDamageTaken(int damage) => DamageTaken += damage;
        public void TrackRevive() => Revives++;
        public void SetKiller(string killer) => Killer = killer;

        public void EndGame()
        {
            gameOverUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
            _timer.StopTimer();
            statsText.text = $"<b>Killed by {Killer}</b>\n \n" +
                             $"Time survived: {_timer.ReadTimeString(TimerDisplaySetting.MinSec)}\n" +
                             $"Rounds survived: {EnemyManager.Instance.Round}\n" +
                             $"Enemies killed: {KillCount}\n" +
                             $"Damage dealt: {DamageDealt}\n" +
                             $"Damage taken: {DamageTaken}\n" +
                             $"Revives: {Revives}";

        }

        [Button, PropertySpace]
        public void RestartGame() => _sceneChanger.Restart();
    }
}