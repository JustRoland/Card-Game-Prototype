using System;
using System.Collections.Generic;
using Characters.Enemies;
using UnityEngine;

namespace Characters.State_Machine
{
    public class StateMachine<TEnemy> where TEnemy : Enemy
    {
        public State<TEnemy> CurrentState { get; private set; }
        public TEnemy Enemy { get; }
        
        private readonly Dictionary<Type, State<TEnemy>> statesCache = new ();
        
        public event Action<State<TEnemy>, State<TEnemy>> OnStateChanged;
        
        
        public StateMachine(TEnemy enemy)
        {
            Enemy = enemy;
        }
        
        
        public void Start<TState>() where TState : State<TEnemy>, new()
        {
            CurrentState = GetOrCreate<TState>();
            CurrentState.Enter();
        }
 
        public void Update(float deltaTime)
        {
            CurrentState?.Update(deltaTime);
        }

        public void ChangeState<T>() where T : State<TEnemy>, new()
        {
            var next = GetOrCreate<T>();
            if (next == CurrentState) return;
 
            var prev = CurrentState;
            prev?.Exit();
            CurrentState = next;
            CurrentState.Enter();
            OnStateChanged?.Invoke(prev, CurrentState);
        }
 
        private State<TEnemy> GetOrCreate<T>() where T : State<TEnemy>, new()
        {
            var type = typeof(T);
            if (!statesCache.TryGetValue(type, out var state))
            {
                state = new T();
                state.Initialize(Enemy, this);
                statesCache[type] = state;
            }
            return state;
        }
    }
}
