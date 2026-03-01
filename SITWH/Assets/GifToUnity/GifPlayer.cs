using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GifImporter
{
    [ExecuteAlways]
    public class GifPlayer : MonoBehaviour
    {
        public List<Gif> Gifs = new List<Gif>();
        public float changeGifEverySeconds = 5f;

        private int _gifIndex;
        private int _frameIndex;
        private float _flip;
        private float _nextGifChange;
        private Gif _currentGif;

        private void OnEnable()
        {
            PickRandomGif(true);
            ApplyCurrentFrame();
        }

        private void Update()
        {
            if (_currentGif == null) return;

            var frames = _currentGif.Frames;
            if (frames == null || frames.Count == 0) return;

            if (Application.isPlaying && Time.time >= _nextGifChange)
            {
                PickRandomGif(false);
                ApplyCurrentFrame();
                return;
            }

            if (Application.isPlaying && Time.time >= _flip)
            {
                _frameIndex++;
                if (_frameIndex >= frames.Count) _frameIndex = 0;
                ApplyCurrentFrame();
            }
        }

        private void PickRandomGif(bool forceResetTimer)
        {
            if (Gifs == null || Gifs.Count == 0) return;

            int newIndex = Random.Range(0, Gifs.Count);

            if (Gifs.Count > 1)
                while (newIndex == _gifIndex)
                    newIndex = Random.Range(0, Gifs.Count);

            _gifIndex = newIndex;
            _currentGif = Gifs[_gifIndex];
            _frameIndex = 0;

            if (Application.isPlaying || forceResetTimer)
                _nextGifChange = Time.time + changeGifEverySeconds;
        }

        private void ApplyCurrentFrame()
        {
            if (_currentGif == null) return;

            var frames = _currentGif.Frames;
            if (frames == null || frames.Count == 0) return;

            if (_frameIndex >= frames.Count) _frameIndex = 0;

            var frame = frames[_frameIndex];

            Image image = null;
            if (TryGetComponent<SpriteRenderer>(out var spriteRenderer) || TryGetComponent(out image))
            {
                _flip = Time.time + frame.DelayInMs * 0.001f;

                if (spriteRenderer != null) spriteRenderer.sprite = frame.Sprite;
                else if (image != null) image.sprite = frame.Sprite;
            }
        }
    }
}