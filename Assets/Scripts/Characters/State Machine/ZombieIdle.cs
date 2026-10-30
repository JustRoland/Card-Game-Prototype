using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class ZombieIdle : State<Zombie>
    {
        private float _idleTimer;

        public override void Enter()
        {
            _idleTimer = 0;
            Enemy.ClearTarget();
        }

        public override void Update(float deltaTime)
        {
            _idleTimer += deltaTime;
            
            if (Enemy.HasTarget) StateMachine.ChangeState<ZombieChase>();
            
            if (_idleTimer > 3f)
            {
                StateMachine.ChangeState<ZombiePatrol>();
            }
        }

        public override void Exit()
        {
            
        }
    }
}
