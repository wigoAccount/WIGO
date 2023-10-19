using System;
using UnityEngine;

namespace WIGO.Userinterface
{
    public enum WindowId
    {
        LOADING_SCREEN,
        FEED_SCREEN,
        SETTINGS_SCREEN,
        PROFILE_SCREEN,
        REGISTRATION_SCREEN,
        REPORT_SCREEN,
        RECORD_EVENT_SCREEN,
        SAVE_EVENT_SCREEN,
        VIDEO_PREVIEW_SCREEN,
        RESPONSE_INFO_SCREEN,
        CREATE_EVENT_SCREEN,
        LOCATION_SELECT_SCREEN,
        CHATS_LIST_SCREEN,
        CHAT_SCREEN,
        EVENTS_REQUESTS_SCREEN,
        EVENT_VIEW_SCREEN,
        START_SCREEN,
        POPUP_SCREEN,
        COMPLAIN_SCREEN
    }

    public class UIWindow : MonoBehaviour
    {
        protected UIWindowModel _model;                         // cached window data
        protected bool _cleared;                                // was current window marked as cleared from stack

        /// <summary>
        /// Call from UIManager during skip in Switch function
        /// </summary>
        /// <returns></returns>
        public bool IsCleared() => _cleared;

        /// <summary>
        /// Call from UIManager to clear each window once
        /// </summary>
        public void MarkAsCleared() => _cleared = true;

        /// <summary>
        /// Cache window data and store it in UIManager
        /// </summary>
        /// <returns></returns>
        public virtual UIWindowModel GetSavedState()
        {
            return _model;
        }

        #region Window switch functions
        /// <summary>
        /// Open window if move forward in hierarchy
        /// </summary>
        public virtual void OnOpen(WindowId previous) { }

        /// <summary>
        /// Reopen window if previous window has closed
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="cachedModel"></param>
        public virtual void OnReopen(WindowId previous, UIWindowModel cachedModel)
        {
            OnOpen(previous);
        }

        /// <summary>
        /// Close window to come back to previous window
        /// </summary>
        /// <param name="previous"></param>
        public virtual void OnBack(WindowId previous, Action callback = null) 
        {
            OnClose(previous, callback);
        }

        /// <summary>
        /// Close window and move forward to next window
        /// </summary>
        public virtual void OnClose(WindowId next, Action callback = null)
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Close or reboot window if it's not active
        /// </summary>
        public virtual void CloseUnactive()
        {
            if (!gameObject.activeInHierarchy)
            {
                MarkAsCleared();
                gameObject.SetActive(false);
            }
        }
        #endregion

        #region Unity functions
        protected virtual void Awake() { }
        #endregion
    }
}
