using UnityEngine;
using WIGO.Core;
using WIGO.Userinterface;
using WIGO.Utility;

namespace WIGO
{
    public class MainGameObject : MonoBehaviour
    {
        [SerializeField] UIManager _uiManager;
        [SerializeField] S3DataConfig _s3DataConfig;
        [SerializeField] bool _clearSaveData;

        GameModel _model;

        private void Awake()
        {
            Application.targetFrameRate = 120;
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
            ServiceLocator.Set(_model);
            S3ContentClient s3Client = new S3ContentClient(_s3DataConfig);
            ServiceLocator.Set(s3Client);

            if (string.IsNullOrEmpty(saveData))
            {
                _uiManager.Open<StartScreenWindow>(WindowId.START_SCREEN, window => window.Setup(true));
            }
            else
            {
                Login();
            }

            KeyboardManager keyboardManager = new KeyboardManager();
            ServiceLocator.Set(keyboardManager);
        }

        private void Update()
        {
            _model.Tick();
        }

        async void Login()
        {
            LogoWindow logoWindow = null;
            _uiManager.Open<LogoWindow>(WindowId.LOGO_SCREEN, window => 
            {
                logoWindow = window;
            });

            bool res = await _model.TryLogin();
            if (res)
            {
                logoWindow?.Setup(true);
                return;
            }

            _uiManager.GetPopupManager().AddErrorNotification(100);
        }

        void LoadLanguageLocal()
        {
            Language lang = Language.RUS;// _model.GetLanguage();

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
