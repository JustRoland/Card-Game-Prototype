using Characters.Enemies;

namespace Characters.State_Machine
{
    public class DroneIdle : State<Drone>
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
            
            if (Enemy.HasTarget) StateMachine.ChangeState<DroneChase>();
            
            if (_idleTimer > 3f)
            {
                StateMachine.ChangeState<DronePatrol>();
            }
        }

        public override void Exit()
        {
            
        }
    }
}
