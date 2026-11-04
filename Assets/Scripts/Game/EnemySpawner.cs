using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Enemies;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using Random = UnityEngine.Random;

namespace Game
{
    public interface IEntity
    {
        public void SetSpawner(EnemySpawner spawner);
    }

    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private bool debug;
        [SerializeField] private EnemySpawn[] spawns;
        private int spawnAmount;
        [SerializeField] private float spawnRadius;
        [SerializeField] private bool spawnOnStart;
        [SerializeField] private bool spawnOnDeath;
        [SerializeField] private bool allowReinforcements;
        [SerializeField] private bool allowRoundSpawn;
        [SerializeField, MinValue(1)] private int minRoundToSpawn;

        [Tooltip("Binds pre-existing enemies to this spawner. Note: These enemies will be recycled by the factory!")]
        [SerializeField]
        private EnemyCharacter[] bindOnStart;

        [ShowInInspector] private List<EnemyCharacter> FactoryItems => _factories?.SelectMany(f => f.ActiveItems).ToList();
        public int activeEnemies => FactoryItems.Count;


        private GenericFactory<EnemyCharacter>[] _factories;
        private int _counter;
        public bool AllowReinforcements => allowReinforcements;
        public bool AllowRoundSpawn => allowRoundSpawn;
        public int MinRoundToSpawn => minRoundToSpawn;

        private void OnEnable()
        {
            _factories = new GenericFactory<EnemyCharacter>[spawns.Length];
            spawnAmount = spawns.Sum(s => s.amount);

            for (var i = 0; i < spawns.Length; i++)
            {
                _factories[i] =
                    new GenericFactory<EnemyCharacter>(spawns[i].enemyPrefab, enemy => !enemy.gameObject.activeSelf, maxItems: 100);
            }


            bindOnStart.ForEach(b => _factories[0].BindItem(b));

            if (spawnOnStart) Spawn();

            if (spawnOnDeath)
                _factories[0].LoadedItems.ForEach(character => character.onDeathEvent.AddListener(OnDeathSpawn));
        }

        private void OnDisable()
        {
            if (spawnOnDeath)
                _factories[0].LoadedItems.ForEach(character => character.onDeathEvent.RemoveListener(OnDeathSpawn));
        }

        public List<EnemyCharacter> Spawn(int roundScaling = 1, int maxAmount = 0, Transform target = null)
        {
            var results = new List<EnemyCharacter>();
            int amount = maxAmount > 0 ? Mathf.Min(spawnAmount, maxAmount): spawnAmount;
            int localCounter = 0;
            
            if (amount <= 0) return null;

            for (var spawnIndex = 0; spawnIndex < spawns.Length; spawnIndex++)
            {
                for (int amountIndex = 0; amountIndex < spawns[spawnIndex].amount && localCounter < amount; amountIndex++, localCounter++, _counter++)
                {
                    var newEnemy = _factories[spawnIndex].GetItem();
                    if (debug) Debug.Log($"Spawning {newEnemy} from {_factories[spawnIndex].Name}.", this);
                    newEnemy.transform.position = new Vector3(
                        transform.position.x + Random.Range(-spawnRadius, spawnRadius),
                        transform.position.y,
                        transform.position.z + Random.Range(-spawnRadius, spawnRadius));
                    newEnemy.GetComponent<IEntity>()?.SetSpawner(this);
                    newEnemy.Initialize(roundScaling);
                    newEnemy.gameObject.SetActive(true);
                    if (target) newEnemy.SetTarget(target); // Not working atm!
                    if (spawnOnDeath) newEnemy.onDeathEvent.AddListener(OnDeathSpawn);
                    results.Add(newEnemy);
                }
            }
            
            return results;
            
        }

        private void OnDeathSpawn() => Spawn(1);


        public void UnloadEntity(EnemyCharacter entity)
        {
            entity.gameObject.SetActive(false);
            _counter--;
        }

        public List<EnemyCharacter> GetActiveEntities() => _factories?.SelectMany(f=> f.ActiveItems).ToList();
    }

    [Serializable]
    public struct EnemySpawn
    {
        public EnemyCharacter enemyPrefab;
        public int amount;
    }
}