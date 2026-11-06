using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class ZombieChase : State<Zombie>
    {
        private float _attackSpeed;
        private float _attackCooldown;
        private bool _inRange;
        private bool _canAttack;

        public override void Enter()
        {
            Enemy.SetSpeed(Enemy.Stats.SprintSpeed);
            Enemy.SetTarget(Enemy.CurrentTarget);
        }

        public override void Update(float deltaTime)
        {
            if (!Enemy.HasTarget)
            {
                StateMachine.ChangeState<ZombieIdle>();
                return;
            }

            if (!Enemy.HasWeapon) return;

            _attackCooldown -= Time.deltaTime;
            var distance = Vector3.Distance(Enemy.transform.position, Enemy.CurrentTarget.transform.position);

            _inRange = distance <= Enemy.MaxRange;
            if (_inRange) _attackSpeed -= Time.deltaTime;
            else _attackSpeed = Enemy.attackSpeed;
            
            _canAttack = _inRange && _attackSpeed <= 0 && _attackCooldown <= 0;
            
            if (_canAttack)
            {
                Enemy.Attack();
                _attackCooldown = Enemy.attackCooldown;
            }
            
        }

        public override void Exit()
        {
        }
    }
}