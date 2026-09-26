using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class DroneInvestigate : State<Drone>
    {
        public override void Enter()
        {
            Enemy.SetSpeed(Enemy.Stats.WalkSpeed);
            
        }

        public override void Update(float deltaTime)
        {
            if (Enemy.HasTarget) StateMachine.ChangeState<DroneChase>();
            if (Enemy.HasArrived) StateMachine.ChangeState<DroneIdle>();
        }

        public override void Exit()
        {
            
        }
    }
}
