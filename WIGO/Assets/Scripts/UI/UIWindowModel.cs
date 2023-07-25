using System;

namespace WIGO.Userinterface
{
    public class UIWindowModel
    {
        protected Action _onModelUpdated;

        public virtual void SetupCallback(Action callback)
        {
            _onModelUpdated = callback;
        }
    }

    public class Property<TValue>
    {
        Action<TValue> _callback;
        TValue _value;

        public Property(TValue startValue, Action<TValue> callback = null)
        {
            _value = startValue;
            _callback = callback;
        }

        public TValue Value
        {
            get => _value;
            set
            {
                _value = value;
                _callback?.Invoke(_value);
            }
        }

        public void SetupCallback(Action<TValue> callback) => _callback = callback;
        public void SetupValueWithoutNotify(TValue newValue) => _value = newValue;
    }
}
