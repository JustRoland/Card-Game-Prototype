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
        [SerializeField] private int maxAmount;
        [SerializeField] private float spawnRadius;
        [SerializeField] private bool spawnOnStart;
        [SerializeField] private bool spawnOnDeath;
        [SerializeField] private bool allowReinforcements;

        [Tooltip("Binds pre-existing enemies to this spawner. Note: These enemies will be recycled by the factory!")]
        [SerializeField]
        private EnemyCharacter[] bindOnStart;

        [ShowInInspector] private List<EnemyCharacter> FactoryItems => _factories?.SelectMany(f => f.ActiveItems).ToList();


        private GenericFactory<EnemyCharacter>[] _factories;
        private int _counter;
        public bool IsFull => _counter >= maxAmount;
        public bool AllowReinforcements => allowReinforcements;

        private void OnEnable()
        {
            _factories = new GenericFactory<EnemyCharacter>[spawns.Length];
            spawnAmount = spawns.Sum(s => s.amount);

            for (var i = 0; i < spawns.Length; i++)
            {
                _factories[i] =
                    new GenericFactory<EnemyCharacter>(spawns[i].enemyPrefab, enemy => !enemy.gameObject.activeSelf, 0, maxAmount + 1);
            }


            bindOnStart.ForEach(b => _factories[0].BindItem(b));

            if (spawnOnStart) Spawn(spawnAmount);

            if (spawnOnDeath)
                _factories[0].LoadedItems.ForEach(character => character.onDeathEvent.AddListener(OnDeathSpawn));
        }

        private void OnDisable()
        {
            if (spawnOnDeath)
                _factories[0].LoadedItems.ForEach(character => character.onDeathEvent.RemoveListener(OnDeathSpawn));
        }

        public void Spawn(int amount = 0)
        {
            int fixedAmount = Math.Min(amount > 0 ? amount : spawnAmount, maxAmount - _counter);
            int localCounter = 0;

            if (debug) Debug.Log($"Requested to spawn {amount} enemies -- Actually spawning {amount} enemies");

            if (fixedAmount <= 0) return;

            for (var spawnIndex = 0; spawnIndex < spawns.Length; spawnIndex++)
            {
                for (int amountIndex = 0; amountIndex < spawns[spawnIndex].amount && localCounter < fixedAmount; amountIndex++, localCounter++, _counter++)
                {
                    var newEnemy = _factories[spawnIndex].GetItem();
                    newEnemy.transform.position = new Vector3(
                        transform.position.x + Random.Range(-spawnRadius, spawnRadius),
                        transform.position.y,
                        transform.position.z + Random.Range(-spawnRadius, spawnRadius));
                    newEnemy.GetComponent<IEntity>()?.SetSpawner(this);
                    newEnemy.gameObject.SetActive(true);
                    if (spawnOnDeath) newEnemy.onDeathEvent.AddListener(OnDeathSpawn);
                }
            }
            
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