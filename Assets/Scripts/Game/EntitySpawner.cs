using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

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
    [SerializeField] private float spawnFrequency;
    [SerializeField] private float totalDuration;

    private readonly CancellationTokenSource _source = new();
    private CancellationToken _token;

    private GenericFactory<GameObject> _factory;
    private int _counter;

    private void Start()
    {
        _factory = new GenericFactory<GameObject>(prefab, enemy => !enemy.activeSelf, 0, maxAmount + 1);
        _token = _source.Token;
        Spawn(spawnRadius, spawnFrequency, totalDuration, spawnAmount, _token).Forget();
    }

    private void OnDisable()
    {
        _source.Cancel();
    }

    private async UniTask Spawn(float radius, float frequency, float duration, int amount, CancellationToken token)
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
                newEnt.GetComponent<IEntity>().SetSpawner(this);
                newEnt.SetActive(true);
                _counter++;
            }

            await UniTask.WaitForSeconds(frequency, cancellationToken: token);
        }

        await UniTask.Yield();
    }

    public void UnloadEntity(GameObject entity)
    {
        entity.SetActive(false);
        _counter--;
    }
}