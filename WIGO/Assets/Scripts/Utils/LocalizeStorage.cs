using System.Collections.Generic;
using UnityEngine;

namespace WIGO.Utility
{
    public enum Language
    {
        RUS,
        ENG
    }

    [System.Serializable]
    public class LocalizeStorage
    {
        [SerializeField] List<LocalizeText> _texts;

        public string GetTextWithKey(string key, Language currentLanguage)
        {
            var text = _texts.Find(x => x.key == key);
            return text?.GetTextByLang(currentLanguage);
        }
    }

    [System.Serializable]
    public class LocalizeText
    {
        public string key;
        public string rusText;
        public string engText;

        public string GetTextByLang(Language language)
        {
            switch (language)
            {
                case Language.RUS:
                    return rusText;
                case Language.ENG:
                    return engText;
                default:
                    return null;
            }
        }
    }
}
