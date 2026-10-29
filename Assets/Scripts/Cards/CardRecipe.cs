using UnityEngine;

namespace Cards
{

    [CreateAssetMenu(fileName = "Card Recipe", menuName = "Cards/Card Recipe")]
    public class CardRecipe : ScriptableObject
    {
        [SerializeField] private string description;
        [SerializeField] private CardData[] input;
        [SerializeField] private CardData output;
        [SerializeField] private Effect[] effects;
        
        public string Description => description;
        public CardData[] Input => input;
        public CardData Output => output;
        public Effect[] Effects => effects;
        
        

    
        public Card[] InputCards => GenerateCardInputs();
        private Card[] _inputCards;


        private Card[] GenerateCardInputs()
        {
            if (_inputCards != null) return _inputCards;
            if (Input == null) return null;
        
            _inputCards = new Card[Input.Length];
            for (int i = 0; i < _inputCards.Length; i++)
            { 
                _inputCards[i] = Input[i].GenerateCard();
            }
            return _inputCards;
        }
    
    }
}