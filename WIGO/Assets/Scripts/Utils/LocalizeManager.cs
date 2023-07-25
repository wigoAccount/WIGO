using System.Collections.Generic;

namespace WIGO.Utility
{
    public static class LocalizeManager
    {
        static List<LocalizeItem> _allItems = new List<LocalizeItem>();
        static LocalizeStorage _storage;
        static Language _currentLanguage;

        public static Language GetCurrentLanguage() => _currentLanguage;

        public static void Initialize(LocalizeStorage storage, Language language)
        {
            _storage = storage;
            _currentLanguage = language;
        }

        public static void Subscribe(LocalizeItem item)
        {
            if (!_allItems.Contains(item))
            {
                _allItems.Add(item);
                string text = _storage.GetTextWithKey(item.GetKey(), _currentLanguage);
                item.ChangeLanguageText(text);
            }
        }

        public static void Unscribe(LocalizeItem item)
        {
            if (_allItems.Contains(item))
            {
                _allItems.Remove(item);
            }
        }

        public static void ChangeLanguage(Language lang)
        {
            _currentLanguage = lang;
            foreach (var item in _allItems)
            {
                string text = _storage.GetTextWithKey(item.GetKey(), _currentLanguage);
                item.ChangeLanguageText(text);
            }
        }

        public static string GetTextWithKey(string key)
        {
            return _storage.GetTextWithKey(key, _currentLanguage);
        }
    }
}
