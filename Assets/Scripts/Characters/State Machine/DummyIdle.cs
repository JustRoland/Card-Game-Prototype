using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class DummyIdle : State<Dummy>
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
            
            if (Enemy.HasTarget) StateMachine.ChangeState<DummyChase>();
            
            if (_idleTimer > 3f)
            {
                StateMachine.ChangeState<DummyPatrol>();
            }
        }

        public override void Exit()
        {
            
        }
    }
}
