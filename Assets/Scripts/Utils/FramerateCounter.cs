using TMPro;
using UnityEngine;

namespace KKL.Utils
{
    public class FramerateCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        
        private float _pollingTime;
        private float _time;
        private int _frameCount;
        private bool _active;
        
        private void Awake()
        {
            _pollingTime = 1;
            _active = false;
        }
        
        private void Update()
        {
            _time += Time.deltaTime;
            _frameCount++;

            if (!(_time > _pollingTime)) return;
            
            text.text = Mathf.RoundToInt(_frameCount / _time) + " FPS";
            _time -= _pollingTime;
            _frameCount = 0;
        }
        
        public void Toggle()
        {
            _active = !_active;
            text.enabled = _active;
        }
    }
}