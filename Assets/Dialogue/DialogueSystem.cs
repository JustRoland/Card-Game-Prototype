using System;
using Characters.Player;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Dialogue
{
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance;

        [Header("References")] 
        [SerializeField] private GameObject dialogueOverlay;
        [SerializeField] private TextMeshProUGUI textBox;
        [SerializeField] private TextMeshProUGUI nameBox;
        [SerializeField] private Image leftImage;
        [SerializeField] private Image rightImage;
        [SerializeField] private UnityEvent onAllDialogueFinished;
        private UnityEvent _onThisDialogueFinished;
        private PlayerCharacter _playerReference;


        private CharacterScrObj _leftChar;
        private CharacterScrObj _rightChar;
        private DialogueData[] _data;
        private DialogueData _currentDialogue;
        private int _currentIndex;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }


        public void SetDialogue(CharacterScrObj leftCharacter, CharacterScrObj rightCharacter, DialogueData[] dialogueData, UnityEvent finishedEvent)
        {
            _leftChar = leftCharacter;
            _rightChar = rightCharacter;
            _data = dialogueData;

            leftImage.sprite = _leftChar.sprite;
            rightImage.sprite = _rightChar.sprite;

            _onThisDialogueFinished = finishedEvent;
            
            _currentIndex = 0;
        }

        public void InitiateDialogue(PlayerCharacter player)
        {
            //open overlay
            dialogueOverlay.SetActive(true);
            Time.timeScale = 0f;
            _playerReference = player;
            player.SetInDialogue(true);
            ProgressDialogue();
        }

        public void ProgressDialogue()
        {
            if (_currentIndex >= _data.Length)
            {
                CloseDialogue();
                return;
            }
            
            //take next dialogue step and display it
            textBox.text = _data[_currentIndex].dialogueText;
            nameBox.text = _data[_currentIndex].side switch
            {
                Side.None => "Narrator",
                Side.Left => _leftChar.characterName,
                Side.Right => _rightChar.characterName,
                _ => throw new ArgumentOutOfRangeException()
            };
            _currentIndex++;
        }

        public void CloseDialogue()
        {
            dialogueOverlay.SetActive(false);
            Time.timeScale = 1f;
            onAllDialogueFinished.Invoke();
            _onThisDialogueFinished.Invoke();
            _playerReference.SetInDialogue(false);
        }

    }
}
