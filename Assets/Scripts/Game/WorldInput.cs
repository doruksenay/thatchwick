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
        private Rect _bounds;
        private bool _pressing;
        private bool _wasPinching;
        private bool _movedTooFar;
        private Vector2 _pressStart;
        private Vector2 _lastPos;

        /// <summary>Ekran konumu (piksel, sol alt başlangıçlı) bu dikdörtgenin içindeyse girdi yok sayılır (HUD).</summary>
        public Func<Vector2, bool> IsOverUi;

        public event Action<Vector2> Tapped;

        // Kamera odağı (yapı modunda eşyaya yaklaşma). Kullanıcı kaydırır/yakınlaştırırsa iptal olur.
        private bool _focusing;
        private Vector3 _focusPos;
        private float _focusSize;
        private bool _hasSavedView;
        private Vector3 _savedPos;
        private float _savedSize;

        /// <summary>Kamerayı yumuşakça verilen dünya noktasına ve yakınlaştırmaya götürür.</summary>
        public void FocusOn(Vector2 world, float orthographicSize)
        {
            if (_camera == null) return;
            if (!_hasSavedView)
            {
                _hasSavedView = true;
                _savedPos = _camera.transform.position;
                _savedSize = _camera.orthographicSize;
            }
            _focusing = true;
            _focusPos = new Vector3(world.x, world.y, -10f);
            _focusSize = Mathf.Clamp(orthographicSize, MinZoom, MaxZoom);
        }

        /// <summary>FocusOn öncesi görünüme yumuşakça döner.</summary>
        public void RestoreView()
        {
            if (_camera == null || !_hasSavedView) return;
            _hasSavedView = false;
            _focusing = true;
            _focusPos = _savedPos;
            _focusSize = _savedSize;
        }

        private void StepFocus()
        {
            if (!_focusing) return;
            float t = 1f - Mathf.Exp(-9f * Time.unscaledDeltaTime);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, _focusPos, t);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _focusSize, t);
            if (((Vector2)(_camera.transform.position - _focusPos)).sqrMagnitude < 0.0004f &&
                Mathf.Abs(_camera.orthographicSize - _focusSize) < 0.01f)
                _focusing = false;
        }

        /// <summary>bounds: haritanın dünya (izometrik) sınırları; kamera dışına çıkamaz.</summary>
        public void Init(Camera camera, Rect bounds)
        {
            _camera = camera;
            _bounds = bounds;
            _camera.orthographic = true;
            _camera.orthographicSize = Mathf.Clamp(Mathf.Min(bounds.width, bounds.height) * 0.3f, MinZoom, MaxZoom);
            _camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
            ClampCamera();
        }

        private void Update()
        {
            if (_camera == null) return;

            HandleScrollZoom();
            if (HandlePinch()) return;
            HandlePointer();
            StepFocus();
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
            _focusing = false;
            float worldPerPixel = 2f * _camera.orthographicSize / Screen.height;
            var p = _camera.transform.position;
            p.x -= screenDelta.x * worldPerPixel;
            p.y -= screenDelta.y * worldPerPixel;
            _camera.transform.position = p;
        }

        private void ApplyZoom(float factor)
        {
            _focusing = false;
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize * factor, MinZoom, MaxZoom);
        }

        // Harita görüş alanından küçükse ortalar, büyükse kenardan taşmaz.
        private void ClampCamera()
        {
            float halfH = _camera.orthographicSize;
            float halfW = halfH * _camera.aspect;
            var p = _camera.transform.position;

            p.x = _bounds.width <= halfW * 2f
                ? _bounds.center.x
                : Mathf.Clamp(p.x, _bounds.xMin + halfW, _bounds.xMax - halfW);
            p.y = _bounds.height <= halfH * 2f
                ? _bounds.center.y
                : Mathf.Clamp(p.y, _bounds.yMin + halfH, _bounds.yMax - halfH);

            _camera.transform.position = p;
        }
    }
}
