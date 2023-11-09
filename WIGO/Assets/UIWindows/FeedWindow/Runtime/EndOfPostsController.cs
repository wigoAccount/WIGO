using UnityEngine;
using TMPro;

namespace WIGO.Userinterface
{
    public enum EndOfPostsType
    {
        HaveNoMyEvent,
        NotificationsOff,
        FiltersSearch,
        EmptyFeed,
        LocationOff
    }

    public class EndOfPostsController : MonoBehaviour
    {
        [SerializeField] TMP_Text _title;
        [SerializeField] TMP_Text _desc;
        [SerializeField] GameObject[] _buttons;
        [Space]
        [SerializeField] EmptyFeedDescription[] _modes;

        public void Activate(EndOfPostsType type)
        {
            gameObject.SetActive(true);

            int index = (int)type;
            _title.text = _modes[index].title;
            _desc.text = _modes[index].desc;
            _buttons[index].SetActive(true);
        }

        public void Deactivate()
        {
            foreach (var btn in _buttons)
            {
                btn.SetActive(false);
            }

            gameObject.SetActive(false);
        }
    }

    [System.Serializable]
    public struct EmptyFeedDescription
    {
        public string title;
        public string desc;
    }
}
