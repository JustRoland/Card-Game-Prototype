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

        private void Update()
        {
            _timer.UpdateTimer(Time.deltaTime);
        }

        public void Initialize(string message, float duration, float fadeOutTime)
        {
            _textComponent = GetComponent<TextMeshProUGUI>();

            _textComponent.text = message;
            _duration = duration;
            _fadeOutTime = fadeOutTime;

            _timer = new SimpleTimer(TimerType.Countdown);
            _timer.SetTimer(0, 0, _duration);
            _timer.StartTimer();
            _timer.CountdownFinishedEvent.AddListener(OnCountdownFinished);

        }

        void OnCountdownFinished()
        {
            FadeOut().Forget();
        }

        async UniTask FadeOut()
        {
            float elapsedTime = 0;

            float currentOpacity = 1;

            while (elapsedTime < _fadeOutTime)
            {
                currentOpacity = 1 - elapsedTime / _fadeOutTime;
                _textComponent.alpha = currentOpacity;

                elapsedTime += Time.deltaTime;

                await UniTask.NextFrame();
            }

            Destroy(gameObject);
        }
    }
}
