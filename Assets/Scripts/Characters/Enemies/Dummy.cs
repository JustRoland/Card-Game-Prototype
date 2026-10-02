using Characters.State_Machine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Enemies
{
    public class Dummy : EnemyCharacter
    {
        [Header("State")] private StateMachine<Dummy> stateMachine;
        [SerializeField] private bool debugMode = false;

        [ShowInInspector]
        private string CurrentState => stateMachine != null ? stateMachine.CurrentState.Name : "No state";

        // TODO: Protect variable later
        public float attackCooldown = 4f;

        protected override void OnEnable()
        {
            base.OnEnable();

            stateMachine = new StateMachine<Dummy>(this);
            stateMachine.OnStateChanged += StateChangeDebugMessage;
            stateMachine.Start<DummyIdle>();

            GetComponent<DummyController>()?.Initialize(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            stateMachine.OnStateChanged -= StateChangeDebugMessage;
        }

        protected void Update()
        {
            stateMachine.Update(Time.deltaTime);
        }

        private void StateChangeDebugMessage(State<Dummy> from, State<Dummy> to)
        {
            if (debugMode) print($"[Enemy] {from?.Name ?? "null"} -> {to.Name}");
        }


        public override void SetDestination(Vector3 destination, bool investigate)
        {
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
                stateMachine.ChangeState<DummyDestroy>();
            }
        }
    }
}