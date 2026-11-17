using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.UI_Message
{
    public class UIImage : MonoBehaviour
    {
        private Image image;
        
        private SimpleTimer _timer;
        private float _duration;
        private float _fadeOutTime;
        
        
        public void Instantiate(Sprite sprite, float duration, float fadeOutTime)
        {
            image = GetComponent<Image>();

            image.sprite = sprite;
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
                image.color = new Color(1,1,1, currentOpacity);

                elapsedTime += Time.deltaTime;

                await UniTask.NextFrame();
            }

            Destroy(gameObject);
        }
    }
}
