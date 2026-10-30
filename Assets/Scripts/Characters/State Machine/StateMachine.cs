using System;
using System.Collections.Generic;
using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class StateMachine<TEnemy> where TEnemy : EnemyCharacter
    {
        public State<TEnemy> CurrentState { get; private set; }
        private TEnemy Enemy { get; }
        
        private readonly Dictionary<Type, State<TEnemy>> statesCache = new ();
        
        public event Action<State<TEnemy>, State<TEnemy>> OnStateChanged;
        
        
        public StateMachine(TEnemy enemy)
        {
            Enemy = enemy;
        }
        
        
        public void Start<TState>(bool terminalState = false) where TState : State<TEnemy>, new()
        {
            CurrentState = GetOrCreate<TState>(terminalState);
            CurrentState.Enter();
        }
 
        public void Update(float deltaTime)
        {
            CurrentState.Update(deltaTime);
        }

        public void ChangeState<T>(bool terminalState = false) where T : State<TEnemy>, new()
        {
            if (CurrentState.TerminalState && !terminalState) return; //Terminal states are states that can't be changed out of. This line handles exactly that, unless the next state is also an end state.
            var next = GetOrCreate<T>(terminalState);
            if (next == CurrentState) return;
            
            Debug.Assert(next != null, "No state assigned.");
 
            var prev = CurrentState;
            prev?.Exit();
            CurrentState = next;
            CurrentState.Enter();
            OnStateChanged?.Invoke(prev, CurrentState);
        }
 
        private State<TEnemy> GetOrCreate<T>(bool terminalState) where T : State<TEnemy>, new()
        {
            var type = typeof(T);
            if (!statesCache.TryGetValue(type, out var state))
            {
                state = new T();
                state.Initialize(Enemy, this, terminalState);
                statesCache[type] = state;
            }
            return state;
        }
    }
}
