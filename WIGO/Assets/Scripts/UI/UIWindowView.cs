using UnityEngine;

namespace WIGO.Userinterface
{
    public class UIWindowView<TModel> : MonoBehaviour where TModel : UIWindowModel
    {
        protected TModel _windowModel;

        public virtual void Init(TModel model)
        {
            _windowModel = model;
            model.SetupCallback(OnPropertyChanged);
        }

        protected virtual void OnPropertyChanged() { }
    }
}
