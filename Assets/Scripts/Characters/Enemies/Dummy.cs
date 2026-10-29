using Characters.State_Machine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Enemies
{
    public class Dummy : EnemyCharacter
    {
        private StateMachine<Dummy> stateMachine;

        [ShowInInspector, PropertySpace]
        private string CurrentState => stateMachine != null ? stateMachine.CurrentState.Name : "No state";

        // TODO: Protect variable later
        public float attackCooldown = 4f;
        

        protected override void OnEnable()
        {
            base.OnEnable();
            // if (!enableThisEnemy) return;
            stateMachine = new StateMachine<Dummy>(this);
            stateMachine.OnStateChanged += StateChangeDebugMessage;
            stateMachine.Start<DummyIdle>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // if (!enableThisEnemy) return;
            stateMachine.OnStateChanged -= StateChangeDebugMessage;
        }

        protected void Update()
        {
            if (!enableThisEnemy) return;
            stateMachine.Update(Time.deltaTime);
        }

        private void StateChangeDebugMessage(State<Dummy> from, State<Dummy> to)
        {
            if (debugMode) print($"[Enemy] {from?.Name ?? "null"} -> {to.Name}");
        }


        public override void SetDestination(Vector3 destination, bool investigate)
        {
            if (!enableThisEnemy) return;
            base.SetDestination(destination, investigate);
            controller.SetDestination(destination);
            if (!investigate) return;
            if (stateMachine.CurrentState is DummyIdle or DummyPatrol) stateMachine.ChangeState<DummyInvestigate>();
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
                stateMachine?.ChangeState<DummyDestroy>();
            }
        }
    }
}