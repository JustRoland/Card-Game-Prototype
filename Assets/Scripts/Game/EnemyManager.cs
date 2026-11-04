using System.Linq;
using Characters.Enemies;
using Characters.Player;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Game
{
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

        [ShowInInspector, ReadOnly] private int TotalEnemies => _spawners?.Sum(s => s.activeEnemies) ?? 0;
        

        private bool _reinforcementsCalled;
        private float _reinforcementsCooldownTimer;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
            
            RoundSpawner((int)(baseSpawnAmount*Mathf.Pow(roundSpawnMultiplier, round-1))).Forget();
        }

        private void Update()
        {
            if (!_reinforcementsCalled) return;
            _reinforcementsCooldownTimer -= Time.deltaTime;
            if (_reinforcementsCooldownTimer <= 0f) _reinforcementsCalled = false;
        }

        public void CallReinforcements(Vector3 toLocation, EnemyCharacter caller)
        {
            if (_reinforcementsCalled) return;
            _reinforcementsCalled = true;
            print($"Reinforcements called to {toLocation}");
            _reinforcementsCooldownTimer = reinforcementsCooldown;
            GetNearestAvailableSpawner(true, toLocation)?.Spawn();

            //Grab all spawners and bind all active entities to the caller. Caller can then update their target location.
            _spawners.ToList().Where(spawner => spawner.AllowReinforcements).ForEach(spawner =>
                spawner.GetActiveEntities().ForEach(enemy => enemy.BindToCaller(caller)));
        }

        private async UniTask RoundSpawner(int amountToSpawn)
        {
            if (debug) Debug.Log($"Spawning round {round} with {amountToSpawn} enemies.", this);
            
            await UniTask.WaitForSeconds(roundStartDelay);
            // grab all spawners that fit the level requirement
            var spawners = _spawners.Where(spawner => spawner.AllowRoundSpawn && spawner.MinRoundToSpawn <= round).ToList();
            var counter = 0;

            while (counter < amountToSpawn)
            {
                // loop over spawners and call their spawn method, tracking the number of spawned units
                counter = spawners.Aggregate(counter,
                    (current, enemySpawner) => current + enemySpawner.Spawn(round, amountToSpawn - current,
                        playerLocationKnownToEnemies ? player : null).Count);

                // after a delay, loop again until amountToSpawn is reached
                await UniTask.WaitForSeconds(baseSpawnDelay);
            }

            await UniTask.Yield();
        }

        private EnemySpawner GetNearestAvailableSpawner(bool forReinforcement, Vector3 location)
        {
            if (_spawners.Length == 0) return null;

            float minDistance = float.MaxValue;
            EnemySpawner nearestSpawner = null;

            foreach (var t in _spawners)
            {
                if (!forReinforcement && !t.AllowReinforcements && t.MinRoundToSpawn <= round) continue;
                var distance = Vector3.Distance(location, t.transform.position);
                if (!(distance < minDistance)) continue;
                minDistance = distance;
                nearestSpawner = t;
            }

            return nearestSpawner;
        }
    }
}