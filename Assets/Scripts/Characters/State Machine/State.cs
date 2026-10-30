using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public abstract class State<T> where T : EnemyCharacter
    {
        protected StateMachine<T> StateMachine;
        protected T Enemy;
        public bool TerminalState { get; private set; }

        public void Initialize(T enemy, StateMachine<T> stateMachine, bool terminalState)
        {
            StateMachine = stateMachine;
            Enemy = enemy;
            TerminalState = terminalState;
        }
        
        public string Name => GetType().Name;

        public abstract void Enter();

        public abstract void Update(float deltaTime);

        public abstract void Exit();

    }
}
