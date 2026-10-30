using Characters.State_Machine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Enemies
{
    public class Zombie : EnemyCharacter
    {
        private StateMachine<Zombie> stateMachine;

        [ShowInInspector, PropertySpace]
        private string CurrentState => stateMachine != null ? stateMachine.CurrentState.Name : "No state";

        // TODO: Protect variable later
        public float attackCooldown = 4f;
        

        protected override void OnEnable()
        {
            base.OnEnable();
            
            stateMachine = new StateMachine<Zombie>(this);
            stateMachine.OnStateChanged += StateChangeDebugMessage;
            stateMachine.Start<ZombieIdle>(!responsive);
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

        private void StateChangeDebugMessage(State<Zombie> from, State<Zombie> to)
        {
            if (debug) print($"[Enemy] {from?.Name ?? "null"} -> {to.Name}");
        }


        public override void SetDestination(Vector3 destination, bool investigate)
        {
            base.SetDestination(destination, investigate);
            // controller.SetDestination(destination);
            if (!investigate) return;
            if (stateMachine.CurrentState is ZombieIdle or ZombiePatrol) stateMachine.ChangeState<ZombieInvestigate>();
        }


        public override void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin)
        {
            base.Damage(bodyPart, damage, knockBack, origin);

            if (Stats.Health > 0)
            {
                if (responsive) SetDestination(origin, true);
            }
            else
            {
                stateMachine.ChangeState<ZombieDestroy>(true);
            }
        }
    }
}