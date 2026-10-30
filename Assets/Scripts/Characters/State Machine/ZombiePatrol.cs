using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class ZombiePatrol : State<Zombie>
    {
        public override void Enter()
        {
            Enemy.SetSpeed(Enemy.Stats.WalkSpeed);
            Enemy.SetDestination(PickRandomLocation(15f), false);
        }

        public override void Update(float deltaTime)
        {
            if (Enemy.HasTarget) StateMachine.ChangeState<ZombieChase>();
            if (Enemy.HasArrived) StateMachine.ChangeState<ZombieIdle>();
        }

        public override void Exit()
        {
            
        }


        private Vector3 PickRandomLocation(float range)
        {
            return new Vector3(Enemy.transform.position.x + Random.Range(-range, range), Enemy.transform.position.y, Enemy.transform.position.z + Random.Range(-range, range));
        }
    }
    
}
