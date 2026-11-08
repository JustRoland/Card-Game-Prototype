using System;
using System.Linq;
using Characters.Enemies;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Game
{
    public enum RoundState { Idle, Start, InProgress, End }
    
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;

        private EnemySpawner[] _spawners;
        [SerializeField] private Transform player;
        [SerializeField] private bool debug;
        [SerializeField] private bool startRoundOnStart;
        [SerializeField] private bool playerLocationKnownToEnemies;
        [SerializeField] private float reinforcementsCooldown = 10f;
        [SerializeField] private float roundSpawnMultiplier = 1.2f;
        [SerializeField] private float baseSpawnDelay = 6f;
        [SerializeField, MinValue(1)] private int baseSpawnAmount = 1;
        [SerializeField] private int baseReinforcementAmount;


        public int TotalEnemies => _spawners?.Sum(s => s.activeEnemies) ?? 0;
        

        private bool _reinforcementsCalled;
        private float _reinforcementsCooldownTimer;
        private int _spawnCounter;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _spawners = FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            _spawners = _spawners.Where(spawner => !spawner.ExcludeFromRounds).ToArray();

        }

        private void FixedUpdate()
        {
            if (!_reinforcementsCalled) return;
            _reinforcementsCooldownTimer -= Time.deltaTime;
            if (_reinforcementsCooldownTimer <= 0f) _reinforcementsCalled = false;
        }
        
        
        public void SpawnWave(int round)
        {
            _spawnCounter = (int)(baseSpawnAmount*Mathf.Pow(roundSpawnMultiplier, round-1));
            RoundSpawner(round, _spawnCounter).Forget();
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

        private async UniTask RoundSpawner(int round, int amountToSpawn)
        {
            if (debug) Debug.Log($"Spawning round {round} with {amountToSpawn} enemies.", this);
            
            var counter = 0;

            while (counter < amountToSpawn)
            {
                foreach (var spawner in _spawners.TakeWhile(spawner => counter < amountToSpawn))
                {
                    counter += spawner.Spawn(round, amountToSpawn - counter, playerLocationKnownToEnemies ? player : null).Count;
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
                if (!forReinforcement && !t.AllowReinforcements) continue;
                var distance = Vector3.Distance(location, t.transform.position);
                if (!(distance > maxDistance)) continue;
                maxDistance = distance;
                farthestSp = t;
            }

            return farthestSp;
        }
    }
}