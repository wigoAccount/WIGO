using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace WIGO.Userinterface
{
    public class PopupWindowElement : MonoBehaviour
    {
        [SerializeField] TMP_Text _title;
        [SerializeField] PopupOptionElement _optionPrefab;
        [SerializeField] RectTransform _content;
        [SerializeField] RectTransform _panel;

        CanvasGroup _windowGroup;

        public void Setup(string titleKey, IEnumerable<PopupOption> options)
        {
            _windowGroup = GetComponent<CanvasGroup>();

            _title.SetText(titleKey);
            foreach (var option in options)
            {
                var optionElement = Instantiate(_optionPrefab, _content);
                optionElement.Setup(option.descKey, option.onOptionSelect, option.color);
            }

            _panel.localScale = Vector3.one * 0.8f;
            _windowGroup.alpha = 0f;
            StartCoroutine(UpdateView());
        }

        public void OnClose()
        {
            DOTween.Sequence().Append(_windowGroup.DOFade(0f, 0.24f))
                .Join(_panel.DOScale(0.8f, 0.24f).SetEase(Ease.InBack))
                .OnComplete(() => Destroy(gameObject));
        }

        IEnumerator UpdateView()
        {
            yield return new WaitForEndOfFrame();
            _panel.sizeDelta = new Vector2(_panel.sizeDelta.x, _content.sizeDelta.y + 32f + _title.preferredHeight);

            DOTween.Sequence().Append(_windowGroup.DOFade(1f, 0.24f))
                .Join(_panel.DOScale(1f, 0.24f).SetEase(Ease.OutBack));
        }
    }
}
