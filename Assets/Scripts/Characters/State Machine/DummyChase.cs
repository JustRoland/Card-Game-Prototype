using Characters.Enemies;
using Game;
using UnityEngine;

namespace Characters.State_Machine
{
    public class DummyChase : State<Dummy>
    {
        private float _attackTimer;

        public override void Enter()
        {
            Enemy.SetSpeed(Enemy.Stats.SprintSpeed);
            Enemy.SetTarget(Enemy.CurrentTarget);
            _attackTimer = Enemy.attackCooldown;
        }

        public override void Update(float deltaTime)
        {
            if (!Enemy.HasTarget)
            {
                StateMachine.ChangeState<DummyIdle>();
                return;
            }
            
            if (!Enemy.HasWeapon) return;

            var distance = Vector3.Distance(Enemy.transform.position, Enemy.CurrentTarget.transform.position);
            _attackTimer -= deltaTime;

            if (_attackTimer <= 0 && distance <= Enemy.MaxRange)
            {
                _attackTimer = Enemy.attackCooldown;
                Enemy.Attack();
                Debug.Log($"{Enemy.name} is attacking {Enemy.CurrentTarget.name}.");
            }
        }

        public override void Exit()
        {
        }
    }
}