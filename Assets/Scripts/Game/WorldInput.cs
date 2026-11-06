using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Thatchwick.Game
{
    /// <summary>
    /// Kamera girdisi: sürükle = kaydır, tekerlek / iki parmak = yakınlaştır, kısa dokunuş = Tapped.
    /// Fare ve dokunmatik aynı kodla çalışır (Pointer soyutlaması).
    /// </summary>
    public sealed class WorldInput : MonoBehaviour
    {
        public const float MinZoom = 3f;
        public const float MaxZoom = 20f;
        private const float TapMaxMovePixels = 14f;

        private Camera _camera;
        private Vector2 _mapSize;
        private bool _pressing;
        private bool _wasPinching;
        private bool _movedTooFar;
        private Vector2 _pressStart;
        private Vector2 _lastPos;

        /// <summary>Ekran konumu (piksel, sol alt başlangıçlı) bu dikdörtgenin içindeyse girdi yok sayılır (HUD).</summary>
        public Func<Vector2, bool> IsOverUi;

        public event Action<Vector2> Tapped;

        public void Init(Camera camera, Vector2 mapSize)
        {
            _camera = camera;
            _mapSize = mapSize;
            _camera.orthographic = true;
            _camera.orthographicSize = Mathf.Clamp(Mathf.Min(mapSize.x, mapSize.y) * 0.3f, MinZoom, MaxZoom);
            _camera.transform.position = new Vector3(mapSize.x * 0.5f, mapSize.y * 0.5f, -10f);
            ClampCamera();
        }

        private void Update()
        {
            if (_camera == null) return;

            HandleScrollZoom();
            if (HandlePinch()) return;
            HandlePointer();
            ClampCamera();
        }

        private void HandleScrollZoom()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Approximately(scroll, 0f)) return;
            if (IsOverUi != null && IsOverUi(mouse.position.ReadValue())) return;
            ApplyZoom(Mathf.Exp(-Mathf.Sign(scroll) * 0.12f));
        }

        // İki parmakla yakınlaştırma. Pinch sürerken kaydırma ve dokunma devre dışı.
        private bool HandlePinch()
        {
            var ts = Touchscreen.current;
            if (ts == null) { _wasPinching = false; return false; }

            int count = 0;
            Vector2 a = default, b = default, da = default, db = default;
            foreach (var t in ts.touches)
            {
                if (!t.press.isPressed) continue;
                if (count == 0) { a = t.position.ReadValue(); da = t.delta.ReadValue(); }
                else if (count == 1) { b = t.position.ReadValue(); db = t.delta.ReadValue(); }
                count++;
            }

            if (count < 2)
            {
                _wasPinching = false;
                return false;
            }

            if (!_wasPinching)
            {
                // Pinch başlıyor: bekleyen tek parmak dokunuşunu iptal et.
                _wasPinching = true;
                _pressing = false;
            }

            float before = ((a - da) - (b - db)).magnitude;
            float now = (a - b).magnitude;
            if (before > 1f && now > 1f) ApplyZoom(before / now);
            ClampCamera();
            return true;
        }

        private void HandlePointer()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            Vector2 pos = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                if (IsOverUi != null && IsOverUi(pos)) return;
                _pressing = true;
                _movedTooFar = false;
                _pressStart = pos;
                _lastPos = pos;
                return;
            }

            if (!_pressing) return;

            if (pointer.press.isPressed)
            {
                if ((pos - _pressStart).magnitude > TapMaxMovePixels) _movedTooFar = true;
                if (_movedTooFar) Pan(pos - _lastPos);
                _lastPos = pos;
                return;
            }

            if (pointer.press.wasReleasedThisFrame || !pointer.press.isPressed)
            {
                _pressing = false;
                if (!_movedTooFar && !_wasPinching) Tapped?.Invoke(pos);
            }
        }

        private void Pan(Vector2 screenDelta)
        {
            float worldPerPixel = 2f * _camera.orthographicSize / Screen.height;
            var p = _camera.transform.position;
            p.x -= screenDelta.x * worldPerPixel;
            p.y -= screenDelta.y * worldPerPixel;
            _camera.transform.position = p;
        }

        private void ApplyZoom(float factor)
        {
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize * factor, MinZoom, MaxZoom);
        }

        // Harita görüş alanından küçükse ortalar, büyükse kenardan taşmaz.
        private void ClampCamera()
        {
            float halfH = _camera.orthographicSize;
            float halfW = halfH * _camera.aspect;
            var p = _camera.transform.position;

            p.x = _mapSize.x <= halfW * 2f
                ? _mapSize.x * 0.5f
                : Mathf.Clamp(p.x, halfW, _mapSize.x - halfW);
            p.y = _mapSize.y <= halfH * 2f
                ? _mapSize.y * 0.5f
                : Mathf.Clamp(p.y, halfH, _mapSize.y - halfH);

            _camera.transform.position = p;
        }
    }
}
