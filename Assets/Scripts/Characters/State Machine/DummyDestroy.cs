using Characters.Enemies;
using Game;
using UnityEngine;

namespace Characters.State_Machine
{
    public class DummyDestroy : State<Dummy>
    {
        public override void Enter()
        {
            if (Enemy.Spawner) Enemy.Spawner.UnloadEntity(Enemy);
            else Object.Destroy(Enemy.gameObject);
            
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
