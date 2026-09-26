using Characters.Enemies;
using Game;
using UnityEngine;

namespace Characters.State_Machine
{
    public class DummyChase : State<Dummy>
    {
        
        public override void Enter()
        {
            Enemy.SetTarget(Enemy.CurrentTarget, Enemy.followDistance, new Vector3(0, 4, 0));
            Enemy.SetSpeed(Enemy.Stats.SprintSpeed);
        }

        public override void Update(float deltaTime)
        {
            if (!Enemy.HasTarget) StateMachine.ChangeState<DummyIdle>();
            
        }

        public override void Exit()
        {
            
        }
    }
}
