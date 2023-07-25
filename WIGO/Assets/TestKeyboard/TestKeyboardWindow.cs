using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using WIGO.Utility;

namespace MyTests.Keyboard
{
    public class TestKeyboardWindow : MonoBehaviour
    {
        [SerializeField] RectTransform _canvas;
        [SerializeField] RectTransform _bottomPanel;
        [Space]
        [SerializeField] TMP_InputField _messageField;
        [SerializeField] TMP_Text _keyboardStatusText;
        [SerializeField] Color[] _statusColors;

        KeyboardManager _keyboardManager;
        Coroutine _keyboardCoroutine;

        public void OnOpenKeyboardClick()
        {
            TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Default, true, true, false, false, string.Empty, 0);
            OnActivateKeyboard(true);
        }

        public void OnSendButtonClick()
        {
            _messageField.text = string.Empty;
        }

        private void Awake()
        {
#if UNITY_ANDROID
            TouchScreenKeyboard.Android.consumesOutsideTouches = false;
#endif
            TouchScreenKeyboard.hideInput = true;
            _keyboardManager = new KeyboardManager(_canvas.rect.height);

            _messageField.onSelect.AddListener((text) => OnOpenKeyboardClick());//OnActivateKeyboard(true));
            //_messageField.onDeselect.AddListener((text) => OnActivateKeyboard(false));
            _messageField.onSubmit.AddListener((text) => Debug.Log(text));
            _messageField.onTouchScreenKeyboardStatusChanged.AddListener(OnKeyboardStatusChange);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _messageField.DeactivateInputField();
                OnActivateKeyboard(false);
            }
        }

        void OnActivateKeyboard(bool activate)
        {
            if (_keyboardCoroutine != null)
            {
                StopCoroutine(_keyboardCoroutine);
                _keyboardCoroutine = null;
            }

            if (activate)
            {
                _keyboardCoroutine = StartCoroutine(CalculateKeyboardHeight());
                return;
            }

            _bottomPanel.DOAnchorPosY(0f, 0.1f).SetEase(Ease.OutSine);
        }

        void OnKeyboardStatusChange(TouchScreenKeyboard.Status status)
        {
            Debug.LogFormat("Keyboard: {0}", status.ToString());
            Color statusColor = _statusColors[(int)status];
            _keyboardStatusText.text += string.Format("<color=#{0}>{1}</color>\r\n", ColorUtility.ToHtmlStringRGB(statusColor), status.ToString());
            if (status == TouchScreenKeyboard.Status.Canceled)
            {
                //_messageField.DeactivateInputField();
                OnActivateKeyboard(false);
            }
        }

        IEnumerator CalculateKeyboardHeight()
        {
            for (float timer = 0f; timer < 1f; timer += Time.deltaTime / 1f)
            {
                float height = _keyboardManager.GetKeyboardHeight() + 56f;
                _bottomPanel.anchoredPosition = Vector2.up * height;
                yield return null;
            }

            _keyboardCoroutine = null;
        }
    }
}
