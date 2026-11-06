using Characters.Enemies;
using Game;
using UnityEngine;

namespace Characters.State_Machine
{
    public class ZombieDestroy : State<Zombie>
    {
        public override void Enter()
        {
            DropManager.Instance.GetDrop(Enemy.Value, Enemy.transform.position);
            
            if (Enemy.Spawner) Enemy.Spawner.UnloadEntity(Enemy);
            else Object.Destroy(Enemy.gameObject);

        }

        public override void Update(float deltaTime)
        {
        
        }

        public override void Exit()
        {
        
        }
    }
}
