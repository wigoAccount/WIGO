using Crystal;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class ChatsListView : UIWindowView<UIWindowModel>
    {
        [SerializeField] RectTransform _header;
        [SerializeField] SafeArea _safeArea;
        [SerializeField] GameObject _loadingTemplate;
        [SerializeField] GameObject _emptyChatsTip;

        public void Init()
        {
            _header.sizeDelta += Vector2.up * _safeArea.GetSafeAreaUpperPadding();
        }

        public void SetLoadingVisible(bool visible)
        {
            _loadingTemplate.SetActive(visible);
        }

        public void SetEmptyTipVisible(bool visible)
        {
            _emptyChatsTip.SetActive(visible);
        }
    }
}
