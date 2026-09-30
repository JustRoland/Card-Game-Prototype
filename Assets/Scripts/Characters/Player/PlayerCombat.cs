namespace Characters.Player
{
    public class PlayerCombat : CombatBase
    {

        public void UpdateInput(CharacterInput input)
        {
            if (input.Attack) Attack(input.AttackButtonDown);
            if (input.Reload) Reload();
        }


    }
}