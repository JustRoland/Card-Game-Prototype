using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public abstract class State<T> where T : EnemyCharacter
    {
        protected StateMachine<T> StateMachine;
        protected T Enemy;

        public void Initialize(T enemy, StateMachine<T> stateMachine)
        {
            StateMachine = stateMachine;
            Enemy = enemy;
        }
        
        public string Name => GetType().Name;

        public abstract void Enter();

        public abstract void Update(float deltaTime);

        public abstract void Exit();

    }
}
