using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class DronePatrol : State<Drone>
    {
        public override void Enter()
        {
            Enemy.SetSpeed(Enemy.Stats.WalkSpeed);
            Enemy.SetDestination(PickRandomLocation(15f), false);
        }

        public override void Update(float deltaTime)
        {
            if (Enemy.HasTarget) StateMachine.ChangeState<DroneChase>();
            if (Enemy.HasArrived) StateMachine.ChangeState<DroneIdle>();
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
