using System;
using System.Collections.Generic;
using Characters.Enemies;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
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
        [SerializeField] private EnemyCharacter prefab;
        [SerializeField] private int spawnAmount;
        [SerializeField] private int maxAmount;
        [SerializeField] private float spawnRadius;
        [SerializeField] private bool spawnOnStart;
        [SerializeField] private bool allowReinforcements;
        [Tooltip("Binds pre-existing enemies to this spawner. Note: These enemies will be recycled by the factory!")]
        [SerializeField] private EnemyCharacter[] bindOnStart;

        [ShowInInspector] private List<EnemyCharacter> FactoryItems => _factory?.LoadedItems;


        private GenericFactory<EnemyCharacter> _factory;
        private int _counter;
        public bool IsFull => _counter >= maxAmount;
        public bool AllowReinforcements => allowReinforcements;

        private void Start()
        {
            _factory = new GenericFactory<EnemyCharacter>(prefab, enemy => !enemy.gameObject.activeSelf, 0, maxAmount + 1);

            bindOnStart.ForEach(b => _factory.BindItem(b));
            
            if (spawnOnStart) Spawn(spawnAmount, spawnRadius);
        }

        public void PresetSpawn()
        {
            Spawn(spawnAmount, spawnRadius);
        }

        public EnemyCharacter[] Spawn(int amount, float radius = 0)
        {
            var rad = radius > 0 ? radius : spawnRadius;
            var fixedAmount = Math.Min(amount, maxAmount - _counter);
            
            if (fixedAmount <= 0) return null;
            
            EnemyCharacter[] spawnedEnemies = new EnemyCharacter[fixedAmount];
            for (int i = 0; i < fixedAmount; i++)
            {
                var newEnemy = _factory.GetItem();
                newEnemy.transform.position = new Vector3(transform.position.x + Random.Range(-rad, rad),
                    transform.position.y,
                    transform.position.z + Random.Range(-rad, rad));
                newEnemy.GetComponent<IEntity>()?.SetSpawner(this);
                newEnemy.gameObject.SetActive(true);
                spawnedEnemies[i] = newEnemy;
                _counter++;
            }
            
            return spawnedEnemies;
        }

        public void UnloadEntity(EnemyCharacter entity)
        {
            entity.gameObject.SetActive(false);
            _counter--;
        }

        public List<EnemyCharacter> GetActiveEntities() => _factory.ActiveItems;
    }
}