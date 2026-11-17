using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Utility.UI_Message
{
    public class UIMessage : MonoBehaviour
    {

        private TextMeshProUGUI _textComponent;

        private SimpleTimer _timer;
        private float _duration;
        private float _fadeOutTime;

        
        public void Initialize(string message, float duration, float fadeOutTime, Color color = default)
        {
            _textComponent = GetComponent<TextMeshProUGUI>();

            _textComponent.text = message;
            if (color != default) _textComponent.color = color;
            _duration = duration;
            _fadeOutTime = fadeOutTime;

            _timer = new SimpleTimer(TimerType.Countdown);
            _timer.SetTimer(0, 0, _duration);
            _timer.StartTimer();
            _timer.CountdownFinishedEvent.AddListener(OnCountdownFinished);

        }
        
        private void Update()
        {
            _timer.UpdateTimer(Time.deltaTime);
        }

        private void OnCountdownFinished()
        {
            FadeOut().Forget();
        }

        private async UniTask FadeOut()
        {
            float elapsedTime = 0;

            while (elapsedTime < _fadeOutTime)
            {
                var currentOpacity = 1 - elapsedTime / _fadeOutTime;
                _textComponent.alpha = currentOpacity;

                elapsedTime += Time.deltaTime;

                await UniTask.NextFrame();
            }

            Destroy(gameObject);
        }
    }
}
