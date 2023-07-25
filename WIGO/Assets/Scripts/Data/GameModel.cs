using System.Threading.Tasks;
using UnityEngine;
using WIGO;
using WIGO.Core;

[System.Serializable]
public class GameModel
{
    [SerializeField] string _ltoken;
    [SerializeField] NotificationSettings _notifications;

    ProfileData _myProfile;
    EventCard _myEvent;

    public string GetRegisterToken() => _ltoken;
    public NotificationSettings GetNotifications() => _notifications;
    public ProfileData GetMyProfile() => _myProfile;
    public bool HasMyOwnEvent() => _myEvent != null;
    public bool IsMyProfile(string id) => string.Compare(id, _myProfile.uid) == 0;

    public void SaveLongToken(string token)
    {
        _ltoken = token;
        SaveData();
    }

    public void SaveNotifications(NotificationSettings settings)
    {
        _notifications = settings;
        SaveData();
    }

    public void SetMyEvent(EventCard card)
    {
        _myEvent = card;
    }

    public async Task<bool> TryLogin()
    {
        if (string.IsNullOrEmpty(_ltoken))
        {
            // temp
            _myProfile = new ProfileData()
            {
                uid = "13",
                lang = "ru",
                nickname = "аpostal",
                email = "аpostal_88@gmail.com",
                phone = "79032222222",
                firstname = "Tommy G",
                birthday = "2000-01-01",
                about = "Обо мне",
                gender = new ContainerData() { uid = 0, name = "male" },
                tags = new ContainerData[]
                {
                    new ContainerData() {uid = 0, name = "Рестораны" },
                    new ContainerData() {uid = 1, name = "Кафе" },
                    new ContainerData() {uid = 2, name = "Вечеринки" },
                    new ContainerData() {uid = 3, name = "Свидания" },
                    new ContainerData() {uid = 4, name = "Танцы" },
                }
            };
            return true;

            //return false;
        }

        _myProfile = await NetService.TryLogin(_ltoken);
        return _myProfile != null;
    }

    void SaveData()
    {
        string saveData = JsonReader.Serialize(this);
        PlayerPrefs.SetString("SaveData", saveData);
    }
}

[System.Serializable]
public struct NotificationSettings
{
    public bool responses;
    public bool newMessages;
    public bool newEvent;
    public bool expireEvent;
    public bool areYouOK;
    public bool estimate;
}
