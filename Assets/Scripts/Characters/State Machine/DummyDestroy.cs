using Cards;
using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class DummyDestroy : State<Dummy>
    {
        public override void Enter()
        {
            if (Enemy.Spawner) Enemy.Spawner.UnloadEntity(Enemy.gameObject);
            else Object.Destroy(Enemy.gameObject);
            
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
