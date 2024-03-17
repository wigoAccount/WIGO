using System.Threading.Tasks;

namespace WIGO.Utility
{
    public interface INotificationAgent
    {
        Task InitAgent();
        void StopAgent();
        Task<string> GetToken();
        event System.Action<string> OnAuthorization;
        event System.Action<PushNotificationStruct> OnPushReceived;
    }
}