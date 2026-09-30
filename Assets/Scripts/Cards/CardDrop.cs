using Characters.Player;
using UnityEngine;
using Utility;

namespace Cards
{
    public class CardDrop : MonoBehaviour, IInteractable<PlayerCharacter>
    {

        public void Interact(PlayerCharacter caller)
        {
            CardManager.Instance.AddCard();
            gameObject.SetActive(false);
        }
    
    }
}
