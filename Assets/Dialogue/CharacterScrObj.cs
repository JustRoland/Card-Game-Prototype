using UnityEngine;
using UnityEngine.Serialization;

namespace Dialogue
{
    [CreateAssetMenu(menuName = "Dialogue/Character")]
    public class CharacterScrObj : ScriptableObject
    {
        public string characterName;
        public Sprite sprite;
    }
}
