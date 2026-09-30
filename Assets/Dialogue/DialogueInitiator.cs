using Characters.Player;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace Dialogue
{
    public class DialogueInitiator : MonoBehaviour, IInteractable<PlayerCharacter>
    {
        [SerializeField] private CharacterScrObj leftCharacter;
        [SerializeField] private CharacterScrObj rightCharacter;
        [SerializeField] private DialogueScrObj dialogue;
        [SerializeField] private UnityEvent onDialogueFinished;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerCharacter>(out var player))
            {
                Interact(player);
            }
        }

        private void SetDialogue() =>
            DialogueSystem.Instance.SetDialogue(leftCharacter, rightCharacter, dialogue.dialogueData, onDialogueFinished);

        private void StartDialogue(PlayerCharacter player) => DialogueSystem.Instance.InitiateDialogue(player);


        public void Interact(PlayerCharacter caller)
        {
            SetDialogue();
            StartDialogue(caller);
        }
    }
}