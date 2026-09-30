using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Dialogue
{
    [CreateAssetMenu(menuName = "Dialogue/DialogueData")]
    public class DialogueScrObj : ScriptableObject
    {
        [FormerlySerializedAs("DialogueData")] public DialogueData[] dialogueData;
    }

    [Serializable]
    public struct DialogueData
    {
        public Side side;
        public string dialogueText;
    }

    public enum Side
    {
        None,
        Left,
        Right
    }
}
