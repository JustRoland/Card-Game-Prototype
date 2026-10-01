using System;
using System.Linq;
using Characters.Enemies;
using UnityEngine;

namespace Game
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;
        
        private EnemySpawner[] _spawners;
        [SerializeField] private float reinforcementsCooldown = 10f;
        
        private bool _reinforcementsCalled;
        private float _reinforcementsCooldownTimer;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            _spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
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
            _reinforcementsCooldownTimer = Mathf.Min(reinforcementsCooldown, _reinforcementsCooldownTimer);
            GetNearestSpawner(toLocation).Spawn(3);
            
            //Grab all spawners and bind all active entities to the caller. Caller can then update their target location.
            _spawners.ToList().ForEach(sp => sp.GetActiveEntities().ForEach(e => e.BindToCaller(caller)));
        }
        
        private EnemySpawner GetNearestSpawner(Vector3 location)
        {
            if (_spawners.Length == 0) return null;

            float minDistance = float.MaxValue;
            EnemySpawner nearestSpawner = null;
            
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
