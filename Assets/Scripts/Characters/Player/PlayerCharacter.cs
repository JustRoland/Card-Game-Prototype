using System;
using UnityEngine;

namespace Characters.Player
{
    public class PlayerCharacter : CharacterBase
    {
        private Color _effectColor;
        
        private PlayerCombat _combat;
        
        private bool _inDialogue;

        private void Awake()
        {
            _combat = GetComponent<PlayerCombat>();
        }

        public void SetInDialogue(bool locked) => _inDialogue = locked;

    }
}
