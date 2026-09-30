using UnityEngine;

namespace Utility.UI_Message
{
    public class MessageManager : MonoBehaviour
    {
        public static MessageManager Instance; 
    
        [SerializeField] private GameObject _messageContainer;
        [SerializeField] private GameObject _UIMessagePrefab;

        [Header("Settings")]
        [SerializeField] private float _duration;
        [SerializeField] private float _fadeOutTime;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void NewMessage(string message)
        {
            UIMessage m = Instantiate(_UIMessagePrefab, _messageContainer.transform).GetComponent<UIMessage>();
            m.Initialize(message, _duration, _fadeOutTime);
        
        }
    }
}
