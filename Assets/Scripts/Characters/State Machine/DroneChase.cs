using Characters.Enemies;
using Game;
using UnityEngine;

namespace Characters.State_Machine
{
    public class DroneChase : State<Drone>
    {

        private float _locationUpdateTimer;
        private readonly float _locationUpdateInterval = 2f;
        
        public override void Enter()
        {
            Enemy.SetTarget(Enemy.CurrentTarget);
            Enemy.SetSpeed(Enemy.Stats.WalkSpeed);
            _locationUpdateTimer = _locationUpdateInterval;
            EnemyManager.Instance.CallReinforcements(Enemy.transform.position, Enemy);
            Enemy.targetLocationUpdate.Invoke(Enemy.CurrentTarget.transform.position);

        }

        public override void Update(float deltaTime)
        {
            if (!Enemy.HasTarget) StateMachine.ChangeState<DroneIdle>();

            _locationUpdateTimer -= deltaTime;

            if (_locationUpdateTimer <= 0)
            {
                _locationUpdateTimer = _locationUpdateInterval;
                Enemy.targetLocationUpdate.Invoke(Enemy.CurrentTarget.transform.position);
            }
        }

        public override void Exit()
        {
            
        }
    }
}
