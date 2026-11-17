using UnityEngine;

namespace Utility.UI_Message
{
    public class MessageManager : MonoBehaviour
    {
        public static MessageManager Instance; 
    
        [SerializeField] private GameObject _messageContainer;
        [SerializeField] private UIMessage _UIMessagePrefab;
        [SerializeField] private GameObject _imageContainer;
        [SerializeField] private UIImage _UIImagePrefab;

        [Header("Settings")]
        [SerializeField] private float _duration;
        [SerializeField] private float _fadeOutTime;
        
        // TODO: Add factory

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void NewMessage(string message, float duration = 0, float fadeOutTime = 0, Color color = default)
        {
            Debug.Assert(_UIMessagePrefab, "UI Message prefab not set");
            UIMessage m = Instantiate(_UIMessagePrefab, _messageContainer.transform);
            m.Initialize(message, duration > 0 ? duration : _duration, fadeOutTime > 0 ? fadeOutTime : _fadeOutTime, color);
        }

        public void NewImage(Sprite sprite, float duration = 0, float fadeOutTime = 0)
        {
            Debug.Assert(_UIImagePrefab, "UI Image prefab not set");
            UIImage m = Instantiate(_UIImagePrefab, _imageContainer.transform);
            m.Instantiate(sprite, duration > 0 ? duration : _duration, fadeOutTime > 0 ? fadeOutTime : _fadeOutTime);
        }
    }
}
