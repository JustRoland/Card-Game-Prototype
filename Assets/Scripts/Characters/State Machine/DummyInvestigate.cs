using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class DummyInvestigate : State<Dummy>
    {
        public override void Enter()
        {
            Enemy.SetSpeed(Enemy.Stats.SprintSpeed);
        }

        public override void Update(float deltaTime)
        {
            if (Enemy.HasTarget) StateMachine.ChangeState<DummyChase>();
            if (Enemy.HasArrived) StateMachine.ChangeState<DummyIdle>();
        }

        public override void Exit()
        {
            
        }
    }
}
