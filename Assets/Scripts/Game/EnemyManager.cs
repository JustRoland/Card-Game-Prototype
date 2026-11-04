using System.Linq;
using Characters.Enemies;
using Characters.Player;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public enum RoundState { Start, InProgress, End }
    
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;

        private EnemySpawner[] _spawners;
        [SerializeField] private Transform player;
        [SerializeField] private bool debug;
        [SerializeField] private bool playerLocationKnownToEnemies;
        [SerializeField] private float reinforcementsCooldown = 10f;
        [SerializeField, MinValue(1)] private int round = 1;
        [SerializeField] private float roundSpawnMultiplier = 1.2f;
        [SerializeField] private float roundStartDelay = 5f;
        [SerializeField] private float baseSpawnDelay = 6f;
        [SerializeField, MinValue(1)] private int baseSpawnAmount = 1;
        [SerializeField] private int baseReinforcementAmount;

        [ShowInInspector, DisplayAsString] private RoundState RoundState => _roundState;
        [ShowInInspector] private int TotalEnemies => _spawners?.Sum(s => s.activeEnemies) ?? 0;
        
        public int Round => round;
        

        private bool _reinforcementsCalled;
        private float _reinforcementsCooldownTimer;
        private int _spawnCounter;
        private RoundState _roundState;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _spawners = FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            _spawners = _spawners.Where(spawner => !spawner.ExcludeFromRounds).ToArray();
            
            StartRound(round);
        }

        private void FixedUpdate()
        {
            RoundStateTracker();
            if (_roundState is RoundState.End) StartRound(round);
            
            if (!_reinforcementsCalled) return;
            _reinforcementsCooldownTimer -= Time.deltaTime;
            if (_reinforcementsCooldownTimer <= 0f) _reinforcementsCalled = false;
        }

        private void RoundStateTracker()
        {
            if (TotalEnemies == 0 && _roundState is RoundState.InProgress)
            {
                _roundState = RoundState.End;
                round++;
            }
        }

        private void StartRound(int rnd)
        {
            _roundState = RoundState.Start;
            _spawnCounter = (int)(baseSpawnAmount*Mathf.Pow(roundSpawnMultiplier, rnd-1));
            RoundSpawner(rnd, _spawnCounter).Forget();
        }

        public void CallReinforcements(Vector3 toLocation, EnemyCharacter caller)
        {
            if (_reinforcementsCalled) return;
            _reinforcementsCalled = true;
            print($"Reinforcements called to {toLocation}");
            _reinforcementsCooldownTimer = reinforcementsCooldown;
            GetFarthestAvailableSpawner(true, toLocation)?.Spawn();

            //Grab all spawners and bind all active entities to the caller. Caller can then update their target location.
            _spawners.ToList().Where(spawner => spawner.AllowReinforcements).ForEach(spawner =>
                spawner.GetActiveEntities().ForEach(enemy => enemy.BindToCaller(caller)));
        }

        private async UniTask RoundSpawner(int rnd, int amountToSpawn)
        {
            if (debug) Debug.Log($"Spawning round {rnd} with {amountToSpawn} enemies.", this);
            
            await UniTask.WaitForSeconds(roundStartDelay);
            _roundState = RoundState.InProgress;
            
            var spawners = _spawners.Where(spawner => spawner.MinRoundToSpawn <= rnd).ToList();
            var counter = 0;

            while (counter < amountToSpawn)
            {
                foreach (var spawner in spawners.TakeWhile(spawner => counter < amountToSpawn))
                {
                    counter += spawner.Spawn(rnd, amountToSpawn - counter, playerLocationKnownToEnemies ? player : null).Count;
                }

                await UniTask.WaitForSeconds(baseSpawnDelay);
            }

            await UniTask.Yield();
        }

        private EnemySpawner GetFarthestAvailableSpawner(bool forReinforcement, Vector3 location)
        {
            if (_spawners.Length == 0) return null;

            float maxDistance = 0;
            EnemySpawner farthestSp = null;

            foreach (var t in _spawners)
            {
                if (!forReinforcement && !t.AllowReinforcements && t.MinRoundToSpawn <= round) continue;
                var distance = Vector3.Distance(location, t.transform.position);
                if (!(distance > maxDistance)) continue;
                maxDistance = distance;
                farthestSp = t;
            }

            return farthestSp;
        }
    }
}