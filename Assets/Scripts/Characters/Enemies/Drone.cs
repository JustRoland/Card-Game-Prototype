using Characters.State_Machine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Enemies
{
    public class Drone : EnemyCharacter
    {        
        [Header("State")]
        private StateMachine<Drone> stateMachine;
        [ShowInInspector] private string CurrentState => stateMachine != null ? stateMachine.CurrentState.Name:"No state";
        
        

        protected override void OnEnable()
        {
            base.OnEnable();
                        
            stateMachine = new StateMachine<Drone>(this);
            stateMachine.OnStateChanged += StateChangeDebugMessage;
            stateMachine.Start<DroneIdle>();
            
            GetComponent<DroneController>()?.Initialize(this);
        }

        protected void OnDisable()
        {
            stateMachine.OnStateChanged -= StateChangeDebugMessage;
        }
        
        protected void Update()
        {
            stateMachine.Update(Time.deltaTime);
        }
        
        private void StateChangeDebugMessage(State<Drone> from, State<Drone> to) => print($"[Enemy] {from?.Name ?? "null"} -> {to.Name}");
        
        
        public override void SetDestination(Vector3 destination, bool investigate)
        {
            base.SetDestination(destination, investigate);
            controller.SetDestination(destination);
            if (!investigate) return;
            if (stateMachine.CurrentState is DroneIdle or DronePatrol) stateMachine.ChangeState<DroneInvestigate>();
        }



        public override void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin)
        {
            base.Damage(bodyPart, damage, knockBack, origin);
            
            if (Stats.Health > 0)
            {
                SetDestination(origin, true);
            }
            else
            {
                stateMachine.ChangeState<DroneDestroy>();
            }
        }
        
    }
}