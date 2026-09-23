using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public interface IEntity
    {
        public void SetSpawner(EntitySpawner spawner);
    }

    public class EntitySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int spawnAmount;
        [SerializeField] private int maxAmount;
        [SerializeField] private float spawnRadius;
        [SerializeField] private float spawnDelay;
        [SerializeField] private float totalDuration;
        [SerializeField] bool spawnOnStart;

        private readonly CancellationTokenSource _source = new();
        private CancellationToken _token;

        private GenericFactory<GameObject> _factory;
        private int _counter;

        private void Start()
        {
            _factory = new GenericFactory<GameObject>(prefab, enemy => !enemy.activeSelf, 0, maxAmount + 1);
            _token = _source.Token;
            if (spawnOnStart) Spawn(spawnAmount, spawnRadius, spawnDelay, totalDuration,  _token).Forget();
        }

        private void OnDisable()
        {
            _source.Cancel();
        }

        public void SpawnEntity(int amount, float frequency, float duration) =>
            Spawn(amount, spawnRadius, frequency, duration, _token).Forget();

        private async UniTask Spawn(int amount, float radius, float delay, float duration, CancellationToken token)
        {
            var startTime = Time.time;
            while (!token.IsCancellationRequested && Time.time < startTime + duration)
            {
                await UniTask.WaitUntil(() => _counter < maxAmount, cancellationToken: token);
                var fixedAmount = Math.Min(amount, maxAmount - _counter);
                for (int i = 0; i < fixedAmount; i++)
                {
                    var newEnt = _factory.GetItem();
                    newEnt.transform.position = new Vector3(transform.position.x + Random.Range(-radius, radius),
                        transform.position.y,
                        transform.position.z + Random.Range(-radius, radius));
                    newEnt.GetComponent<IEntity>()?.SetSpawner(this);
                    newEnt.SetActive(true);
                    _counter++;
                }

                await UniTask.WaitForSeconds(delay, cancellationToken: token);
            }

            await UniTask.Yield();
        }

        public void UnloadEntity(GameObject entity)
        {
            entity.SetActive(false);
            _counter--;
        }
    }
}