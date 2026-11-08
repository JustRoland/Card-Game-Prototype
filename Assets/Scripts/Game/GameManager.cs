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
        [SerializeField] private TextMeshProUGUI nextRoundText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private GameObject gameOverUI;

        [Header("Time")]
        [SerializeField] private float slowTime = 0.2f;
        [SerializeField] private float normalTime = 1f;
        [SerializeField] private float fastTime = 1.5f;
        [SerializeField] private float nextRoundTime = 30f;

        [Header("Round")] [SerializeField] private bool startRoundOnPlay;
        [SerializeField, MinValue(1)] private int round = 1;
        [ShowInInspector, DisplayAsString] private RoundState RoundState => _roundState;

        private RoundState _roundState = RoundState.Idle;
        private SimpleTimer _timer;
        private SimpleTimer _nextRoundTimer;
        private SceneChanger _sceneChanger;
        
        public int Round => round;
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

        private void OnDisable()
        {
            _nextRoundTimer?.CountdownFinishedEvent.RemoveListener(StartNextRound);
        }

        private void FixedUpdate()
        {
            RoundStateTracker();
            _timer.UpdateTimer(Time.unscaledDeltaTime);
            _nextRoundTimer.UpdateTimer(Time.unscaledDeltaTime);
            if (roundText) roundText.text = Round.ToString();
            if (nextRoundText) nextRoundText.text = "Next round in: " + _nextRoundTimer.ReadTimeString(TimerDisplaySetting.MinSec);
        }

        private void Start()
        {
            _timer = new SimpleTimer(TimerType.Stopwatch);
            _timer.SetTimer(0,0,0);
            _timer.StartTimer();
            
            _nextRoundTimer = new SimpleTimer(TimerType.Countdown);
            _nextRoundTimer.SetTimer(0, 0, nextRoundTime);
            _nextRoundTimer.CountdownFinishedEvent.AddListener(StartNextRound);
            nextRoundText.gameObject.SetActive(false);
            
            if (startRoundOnPlay) StartRound(round);
        }

        bool ButtonToggle => _roundState is not RoundState.Idle;
        [Button, HideIf("ButtonToggle")]
        private void StartRound(int rnd)
        {
            round = rnd;
            _roundState = RoundState.InProgress;
            EnemyManager.Instance.SpawnWave(round);
            
        }

        private void RoundStateTracker()
        {
            if (EnemyManager.Instance.TotalEnemies != 0 || _roundState is not RoundState.InProgress) return;
            
            _roundState = RoundState.Start;
            _nextRoundTimer.ResetTimer();
            nextRoundText.gameObject.SetActive(true);
        }

        public void StartNextRound()
        {
            if (_roundState is not RoundState.Start) return;
            round++;
            _nextRoundTimer.StopTimer();
            _roundState = RoundState.InProgress;
            EnemyManager.Instance.SpawnWave(round);
            nextRoundText.gameObject.SetActive(false);
            
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
                             $"Rounds survived: {Round}\n" +
                             $"Enemies killed: {KillCount}\n" +
                             $"Damage dealt: {DamageDealt}\n" +
                             $"Damage taken: {DamageTaken}\n" +
                             $"Revives: {Revives}";

        }

        [Button, PropertySpace]
        public void RestartGame() => _sceneChanger.Restart();
    }
}