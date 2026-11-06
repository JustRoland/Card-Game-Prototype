using UnityEngine;
using UnityEngine.UI;

namespace Progress_Bar.Resources
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private Image _mask;
        [Tooltip("Fill speed. The lower, the faster.")]
        [SerializeField] private float _speed = 10f;
        [SerializeField] private BarType _type;
        private int _maximum;
        private float _newValue;
        private float _velocity;
        private enum BarType
        {
            Smooth,
            Direct
        }

        void Update()
        {

            SetFill();

        }

        private void SetFill()
        {
            if (_type == BarType.Smooth)
            {
                _mask.fillAmount = Mathf.SmoothDamp(_mask.fillAmount, _newValue / _maximum, ref _velocity, Time.deltaTime * _speed);
            }
        }

        public void AddToProgressBar(float amount, float speed = 10f)
        {
            _speed = speed;
            _newValue += amount;
        }

        public void SetProgressBar(float amount)
        {
            _mask.fillAmount = amount / _maximum;
        }

        public void ResetProgressBar(int upperLimit) { _maximum = upperLimit; }
    }
}

