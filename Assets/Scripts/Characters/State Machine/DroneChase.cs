using Characters.Enemies;
using Game;
using UnityEngine;

namespace Characters.State_Machine
{
    public class DroneChase : State<Drone>
    {

        private float _locationUpdateTimer;
        private float _locationUpdateInterval = 2f;
        
        public override void Enter()
        {
            Enemy.SetTarget(Enemy.CurrentTarget, Enemy.followDistance, new Vector3(0, 4, 0));
            _locationUpdateTimer = _locationUpdateInterval;
            EnemyManager.Instance.CallReinforcements(Enemy.transform.position);

        }

        public override void Update(float deltaTime)
        {
            if (!Enemy.HasTarget) StateMachine.ChangeState<DroneIdle>();

            _locationUpdateTimer -= deltaTime;

            if (_locationUpdateTimer <= 0)
            {
                _locationUpdateTimer = _locationUpdateInterval;
                EnemyManager.Instance.UpdateLocationForReinforcements(Enemy.transform.position);
            }
        }

        public override void Exit()
        {
            
        }
    }
}
