using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class ChatInputBottomPanel : MonoBehaviour
    {
        [SerializeField] TMP_InputField _messageInput;
        [SerializeField] Image _sendButton;

        RectTransform _panel;
        RectTransform _fieldRect;
        Action<bool> _onKeyboardActivated;
        Action<float> _onCorrectHeight;
        Tween _squeezeAnimation;
        float _defaultPos;
        float _maxInputHeight;
        int _inputMessageLinesCount = 1;
        bool _messageIsEmpty = true;

        const float LINE_DELTA = 22f;
        const float DEFAULT_INPUT_HEIGHT = 40f;
        const int MAX_LINES_COUNT = 5;

        public string GetMessage() => _messageInput.text;

        public void Initialize(Action<bool> onKeyboardActive, Action<float> onCorrectHeight)
        {
            _onKeyboardActivated = onKeyboardActive;
            _onCorrectHeight = onCorrectHeight;
            _panel = transform as RectTransform;
            _defaultPos = _panel.anchoredPosition.y;
            _maxInputHeight = DEFAULT_INPUT_HEIGHT + (MAX_LINES_COUNT - 1) * LINE_DELTA;
            _fieldRect = _messageInput.transform as RectTransform;

            _messageInput.onSelect.AddListener((text) => _onKeyboardActivated?.Invoke(true));
            _messageInput.onDeselect.AddListener((text) => _onKeyboardActivated?.Invoke(false));
            //_messageInput.onTouchScreenKeyboardStatusChanged.AddListener(OnKeyboardStatusChange);
        }

        public void SetViewDefault(bool animate = true)
        {
            CancelSqueeze();

            float panelPos = _defaultPos;
            if (animate)
            {
                _squeezeAnimation = _panel.DOAnchorPosY(panelPos, 0.1f)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() => _squeezeAnimation = null);
                return;
            }

            _panel.anchoredPosition = Vector2.up * panelPos;
        }

        public void SetHeight(float height)
        {
            CancelSqueeze();
            _panel.anchoredPosition = Vector2.up * (40f + height);
        }

        public void OnEditMessage(string text)
        {
            if (_messageIsEmpty && !string.IsNullOrEmpty(text))
            {
                _messageIsEmpty = false;
                _sendButton.color = UIGameColors.Blue;
            }
            else if (!_messageIsEmpty && string.IsNullOrEmpty(text))
            {
                _messageIsEmpty = true;
                _sendButton.color = UIGameColors.transparent10;

                if (_inputMessageLinesCount > 1)
                {
                    _inputMessageLinesCount = 1;
                    _fieldRect.sizeDelta = new Vector2(_fieldRect.sizeDelta.x, DEFAULT_INPUT_HEIGHT);
                    _panel.sizeDelta = new Vector2(_panel.sizeDelta.x, DEFAULT_INPUT_HEIGHT + 28f);
                    _onCorrectHeight?.Invoke(0f);
                }
            }

            int lines = _messageInput.textComponent.GetTextInfo(text).lineCount;
            if (_inputMessageLinesCount != lines)
            {
                _inputMessageLinesCount = Mathf.Max(lines, 1);
                float height = Mathf.Clamp(DEFAULT_INPUT_HEIGHT + (_inputMessageLinesCount - 1) * LINE_DELTA, DEFAULT_INPUT_HEIGHT, _maxInputHeight);
                _fieldRect.sizeDelta = new Vector2(_fieldRect.sizeDelta.x, height);
                _panel.sizeDelta = new Vector2(_panel.sizeDelta.x, height + 28f);
                _onCorrectHeight?.Invoke(height - DEFAULT_INPUT_HEIGHT);
            }
        }

        public void ClearMessage()
        {
            _messageInput.SetTextWithoutNotify(string.Empty);
            _messageIsEmpty = true;
            _sendButton.color = UIGameColors.transparent10;
            if (_inputMessageLinesCount > 1)
            {
                _fieldRect.sizeDelta = new Vector2(_fieldRect.sizeDelta.x, DEFAULT_INPUT_HEIGHT);
                _panel.sizeDelta = new Vector2(_panel.sizeDelta.x, DEFAULT_INPUT_HEIGHT + 28f);
            }
            _inputMessageLinesCount = 1;
        }

        public void ResetPanel()
        {
            ClearMessage();
            SetViewDefault(false);
        }

        void OnKeyboardStatusChange(TouchScreenKeyboard.Status status)
        {
            Debug.LogFormat("Keyboard: {0}", status.ToString());
            switch (status)
            {
                case TouchScreenKeyboard.Status.Visible:
                    _onKeyboardActivated?.Invoke(true);
                    break;
                case TouchScreenKeyboard.Status.Done:
                case TouchScreenKeyboard.Status.Canceled:
                case TouchScreenKeyboard.Status.LostFocus:
                    _onKeyboardActivated?.Invoke(false);
                    break;
                default:
                    break;
            }
        }

        void CancelSqueeze()
        {
            if (_squeezeAnimation != null)
            {
                _squeezeAnimation.Kill();
                _squeezeAnimation = null;
            }
        }
    }
}
