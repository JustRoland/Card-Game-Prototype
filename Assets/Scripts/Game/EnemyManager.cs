using System;
using UnityEngine;

namespace Game
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;
        
        private EntitySpawner[] _spawners;
        [SerializeField] private float reinforcementsCooldown = 10f;
        
        private bool _reinforcementsCalled;
        private float _reinforcementsCooldownTimer;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            _spawners = FindObjectsByType<EntitySpawner>(FindObjectsSortMode.None);
        }

        private void Update()
        {
            if (!_reinforcementsCalled) return;
            _reinforcementsCooldownTimer -= Time.deltaTime;
            if (_reinforcementsCooldownTimer <= 0f) _reinforcementsCalled = false;
        }

        public void CallReinforcements(Vector3 toLocation)
        {
            if (_reinforcementsCalled) return;
            _reinforcementsCalled = true;
            print($"Reinforcements called to {toLocation}");
            _reinforcementsCooldownTimer = Mathf.Min(reinforcementsCooldown, _reinforcementsCooldownTimer);
            GetNearestSpawner(toLocation).SpawnEntity(3, 1, 1);
        }

        public void UpdateLocationForReinforcements(Vector3 newLocation)
        {
            print($"Updating location for reinforcements to {newLocation}");
            
            // TODO: Update location for reinforcements
        }
        
        private EntitySpawner GetNearestSpawner(Vector3 location)
        {
            if (_spawners.Length == 0) return null;

            float minDistance = float.MaxValue;
            EntitySpawner nearestSpawner = null;
            
            foreach (var t in _spawners)
            {
                var distance = Vector3.Distance(location, t.transform.position);
                if (!(distance < minDistance)) continue;
                minDistance = distance;
                nearestSpawner = t;
            }

            return nearestSpawner;
        }
    }
}
