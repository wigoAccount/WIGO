using System;

namespace WIGO.RecyclableScroll
{
    public class ContactInfo
    {
        protected bool _selected;
        Action<bool> _onSetSelected;

        public void SubscribeToChanges(Action<bool> onSetSelected)
        {
            _onSetSelected += onSetSelected;
        }

        public virtual void UnsubscribeToChanges(Action<bool> onSetSelected)
        {
            _onSetSelected -= onSetSelected;
        }

        public bool IsSelected() => _selected;

        public virtual void SetSelected(bool selected)
        {
            _selected = selected;
            _onSetSelected?.Invoke(selected);
        }
    }
}
