using UnityEngine;
using Utility;

namespace Cards
{
    public class CardDrop : MonoBehaviour, IInteractable
    {

        public void Interact()
        {
            CardManager.Instance.AddCard();
            gameObject.SetActive(false);
        }
    
    }
}
