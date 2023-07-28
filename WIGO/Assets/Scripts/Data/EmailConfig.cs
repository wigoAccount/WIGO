using UnityEngine;

[CreateAssetMenu(menuName = "Content/Configs/Email config")]
public class EmailConfig : ScriptableObject
{
    [System.Serializable]
    struct Letter
    {
        public string address;
        public string subject;
        [Multiline]
        public string body;
    }

    [SerializeField]
    Letter _letter = new Letter()
    {
        address = "help@support.com",
        subject = "Письмо в поддержку",
        body = "Привет. Я сломал вашу игру. И вот как я это сделал."
    };

    public string GetAddress()
    {
        return _letter.address;
    }

    public string GetSubject()
    {
        return _letter.subject;
    }

    public string GetBody()
    {
        return _letter.body;
    }
}
