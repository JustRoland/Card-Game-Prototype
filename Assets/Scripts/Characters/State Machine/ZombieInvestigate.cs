using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class ZombieInvestigate : State<Zombie>
    {
        public override void Enter()
        {
            Enemy.SetSpeed(Enemy.Stats.SprintSpeed);
        }

        public override void Update(float deltaTime)
        {
            if (Enemy.HasTarget) StateMachine.ChangeState<ZombieChase>();
            if (Enemy.HasArrived) StateMachine.ChangeState<ZombieIdle>();
        }

        public override void Exit()
        {
            
        }
    }
}
