using Characters.Enemies;
using Game;

namespace Characters.State_Machine
{
    public class DroneDestroy : State<Drone>
    {

        public override void Enter()
        {
            if (Enemy.Spawner) Enemy.Spawner.UnloadEntity(Enemy);
            else UnityEngine.Object.Destroy(Enemy.gameObject);
            
            DropManager.Instance.GetDrop().transform.position = Enemy.transform.position;
        }

        public override void Update(float deltaTime)
        {
        
        }

        public override void Exit()
        {
        
        }
    }
}
