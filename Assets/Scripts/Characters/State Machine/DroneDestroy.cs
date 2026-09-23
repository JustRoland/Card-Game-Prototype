using Characters.Enemies;

namespace Characters.State_Machine
{
    public class DroneDestroy : State<Drone>
    {

        public override void Enter()
        {
            if (Enemy.Spawner) Enemy.Spawner.UnloadEntity(Enemy.gameObject);
            else UnityEngine.Object.Destroy(Enemy.gameObject);
            
            CardManager.Instance.GetCardDrop(Enemy.transform.position);
        }

        public override void Update(float deltaTime)
        {
        
        }

        public override void Exit()
        {
        
        }
    }
}
