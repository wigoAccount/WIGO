using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIGO.Core;
using WIGO.Userinterface;
using WIGO.Utility;

namespace WIGO
{
    public class MainGameObject : MonoBehaviour
    {
        [SerializeField] UIManager _uiManager;
        [SerializeField] bool _clearSaveData;

        GameModel _model;

        private void Awake()
        {
            Application.targetFrameRate = 60;
#if UNITY_ANDROID && !UNITY_EDITOR
            TouchScreenKeyboard.Android.consumesOutsideTouches = false;
#elif UNITY_IOS && !UNITY_EDITOR
            MessageIOSHandler.Initialize();
#endif
            if (_clearSaveData)
            {
                PlayerPrefs.DeleteKey("SaveData");
            }
            ServiceLocator.Set(_uiManager);
        }

        private void Start()
        {
            string saveData = PlayerPrefs.GetString("SaveData");
            _model = string.IsNullOrEmpty(saveData) ? new GameModel() : JsonReader.Deserialize<GameModel>(saveData);
            LoadLanguageLocal();

            if (string.IsNullOrEmpty(saveData))
            {
                //_uiManager.Open<RegistrationWindow>(WindowId.REGISTRATION_SCREEN);
                //Login();
                _uiManager.Open<StartScreenWindow>(WindowId.START_SCREEN, window => window.Setup(Login));
            }
            else
            {
                //_uiManager.Open<FeedWindow>(WindowId.FEED_SCREEN);
                Login();
            }

            ServiceLocator.Set(_model);
            KeyboardManager keyboardManager = new KeyboardManager();
            ServiceLocator.Set(keyboardManager);
        }

        async void Login()
        {
            bool res = await _model.TryLogin();
            if (res)
            {
                _uiManager.Open<FeedWindow>(WindowId.FEED_SCREEN);
            }
        }

        void LoadLanguageLocal()
        {
            Language lang = Language.ENG;// _model.GetLanguage();

            TextAsset mytxtData = (TextAsset)Resources.Load("Localize");
            string data = mytxtData.text;
            LocalizeStorage storage = JsonReader.Deserialize<LocalizeStorage>(data);
            if (storage != null)
            {
                LocalizeManager.Initialize(storage, lang);
            }
        }
    }
}
