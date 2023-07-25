using UnityEngine;

namespace WIGO.Userinterface
{
    public class SaveEventWindow : EventInfoWindow
    {
        bool _available;

        public override void Setup(string path)
        {
            base.Setup(path);
            _available = false;
        }

        public override void OnEditDescText(string text)
        {
            base.OnEditDescText(text);

            if (_available && string.IsNullOrEmpty(text))
            {
                _available = false;
                CheckIfAvailable();
            }
            else if (!_available && !string.IsNullOrEmpty(text))
            {
                _available = true;
                CheckIfAvailable();
            }
        }

        protected override bool IsAvailable()
        {
            return _available;
        }
    }
}
