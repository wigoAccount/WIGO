using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class UIManager : MonoBehaviour
    {
        class StackElement
        {
            public WindowId id;
            public UIWindow window;
            public UIWindowModel windowCachedData;
        }

        [SerializeField] Canvas _canvas;
        [SerializeField] RectTransform _windowsParent;
        [SerializeField] PopupManager _popupManager;
        [SerializeField] UIWindowStorage _windowStorage;

        Dictionary<WindowId, UIWindow> _created = new Dictionary<WindowId, UIWindow>();     // all created windows storage
        Stack<StackElement> _windows = new Stack<StackElement>();                           // created windows hierarchy
        WindowId _current = NONE_WINDOW;                                                    // current active window user interacts with

        const WindowId NONE_WINDOW = 0;
        static int _uiLayerIndex;

        #region Get functions
        public PopupManager GetPopupManager() => _popupManager;
        public Canvas GetCanvas() => _canvas;
        public Vector2 GetCanvasSize()
        {
            return _canvas.GetComponent<RectTransform>().sizeDelta;
        }
        public TWindow GetWindow<TWindow>(WindowId windowId) where TWindow : UIWindow
        {
            if (_created.TryGetValue(windowId, out UIWindow window))
            {
                return window as TWindow;
            }
            return null;
        }
        #endregion

        public void ShowUI(bool show)
        {
            if (_canvas != null)
            {
                _canvas.enabled = show;
            }
        }

        #region Open functions
        /// <summary>
        /// Open window by type. May be helpfull to call 
        /// specific window afetr opening
        /// </summary>
        /// <typeparam name="TWindow"></typeparam>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <param name="overlay"></param>
        public void Open<TWindow>(WindowId id, Action<TWindow> callback = null, bool overlay = false) where TWindow : UIWindow
        {
            Open(id, (window) =>
            {
                TWindow cast = window as TWindow;
                callback?.Invoke(cast);
            }, overlay);
        }

        /// <summary>
        /// Open window by id considering overlay mode
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <param name="overlay"></param>
        public void Open(WindowId id, Action<UIWindow> callback = null, bool overlay = false)
        {
            FindOrCreate(id, (window) => OnWindowCreated(id, window, callback, overlay));
        }

        public void OpenExclusive(WindowId id, Action<UIWindow> callback = null, bool overlay = false)
        {
            if (_current != id)
            {
                CloseCurrent(true);
            }
            Open(id, callback, overlay);
        }
        #endregion

        #region Close functions
        /// <summary>
        /// Close current window and back to previous (reopen)
        /// </summary>
        /// <param name="exclusive"></param>
        public void CloseCurrent(bool exclusive = false)
        {
            if (_windows.Count > 0)
            {
                var element = _windows.Pop();

                if (_windows.Count > 0)
                {
                    WindowId next = _windows.Peek().id;
                    int curSibling = _windows.Peek().window.transform.GetSiblingIndex();
                    if (element != null)
                    {
                        int prevSibling = element.window.transform.GetSiblingIndex();
                        curSibling = (curSibling < prevSibling) ? curSibling : prevSibling;
                        CheckAndCloseBack(element, next);
                    }

                    if (!exclusive)
                    {
                        _windows.Peek().window.transform.SetSiblingIndex(curSibling);
                        Reopen(_windows.Peek());
                    }
                }
                else
                {
                    _current = NONE_WINDOW;
                    if (element != null)
                    {
                        element.window.OnBack(_current, () => element.window.gameObject.SetActive(false));
                    }
                }
            }
        }

        /// <summary>
        /// Close window by id if it's current
        /// </summary>
        /// <param name="id"></param>
        public void Close(WindowId id)
        {
            if (_current != id)
            {
                Debug.LogWarning("Failed to close window it's not current [" + _current + ", " + id + "]");
                return;
            }

            CloseCurrent();
        }

        /// <summary>
        /// Close all opened windows in stack
        /// </summary>
        public void CloseAll()
        {
            while (_windows.Count > 0)
            {
                var element = _windows.Pop();
                if (element != null)
                {
                    element.window.OnBack(NONE_WINDOW, () => element.window.gameObject.SetActive(false));
                }
            }
        }

        /// <summary>
        /// Destroy all windows and clear data
        /// </summary>
        public void CloseAllHard()
        {
            DOTween.KillAll();
            foreach (KeyValuePair<WindowId, UIWindow> element in _created)
            {
                Destroy(element.Value.gameObject);
            }
            //_popupManager.Clear();
            _created.Clear();
            _windows.Clear();
            _current = NONE_WINDOW;
        }

        /// <summary>
        /// Close active window, close all windows in stack up to required window
        /// Open required window
        /// </summary>
        /// <param name="id"></param>
        public void SwitchTo(WindowId id)
        {
            if (_windows.Any(x => x.id == id))
            {
                var currenElement = (_windows.Count > 0) ? _windows.Peek() : null;
                if (currenElement != null)
                {
                    if (currenElement.id == id)
                    {
                        return;
                    }
                    else
                    {
                        CheckAndDelete(_windows.Pop(), id);
                    }
                }

                while (_windows.Count > 0)
                {
                    var element = _windows.Peek();
                    if (element != null)
                    {
                        if (element.id != id)
                        {
                            _windows.Pop();
                            element.window.CloseUnactive();
                            if (element.window.IsCleared())
                            {
                                continue;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (_windows.Count > 0)
                {
                    Reopen(_windows.Peek());
                }

                _current = (_windows.Count > 0) ? _windows.Peek().id : NONE_WINDOW;
            }
            else
            {
                Open(id);
            }
        }


        #endregion

        void Awake()
        {
            _uiLayerIndex = LayerMask.NameToLayer("UI");
        }

        /// <summary>
        /// Find created window and open it to move forward (considering multiple windows)
        /// If there's no such, instantiate it
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        void FindOrCreate(WindowId id, Action<UIWindow> callback)
        {
            if (_created.TryGetValue(id, out UIWindow value))
            {
                if (id == _current)
                {
                    return;
                }

                callback?.Invoke(value);
            }
            else
            {
                // Create new window
                if (_windowStorage.TryGetWindowPrefabById(id, out UIWindow prefab))
                {
                    var window = Instantiate(prefab, _windowsParent);
                    _created.Add(id, window);
                    callback?.Invoke(window);
                }
                else
                {
                    Debug.LogErrorFormat("Can't get window prefab {0}. Check Local window storage", id);
                }
            }
        }

        /// <summary>
        /// Close previous window (if it's not overlay mode)
        /// and fill needed data than open new window over previous
        /// </summary>
        /// <param name="id"></param>
        /// <param name="window"></param>
        /// <param name="callback"></param>
        /// <param name="overlay"></param>
        void OnWindowCreated(WindowId id, UIWindow window, Action<UIWindow> callback, bool overlay)
        {
            StackElement last = (_windows.Count > 0) ? _windows.Peek() : null;
            if (last != null && !overlay)
            {
                var prevWindow = last.window;
                last.windowCachedData = prevWindow.GetSavedState();
                prevWindow?.OnClose(id, () => prevWindow.gameObject.SetActive(false));
            }
            _current = id;
            _windows.Push(new StackElement() { id = id, window = window });
            window.gameObject.SetActive(true);
            window.transform.SetAsLastSibling();

            WindowId lastId = (last != null) ? last.id : NONE_WINDOW;
            window.OnOpen(lastId);
            callback?.Invoke(window);
        }

        /// <summary>
        /// Check if it's multiple window. If so check for destroy and close it
        /// </summary>
        /// <param name="element"></param>
        /// <param name="nextId"></param>
        void CheckAndCloseBack(StackElement element, WindowId nextId)
        {
            element.window.OnBack(nextId, () => element.window.gameObject.SetActive(false));
        }

        /// <summary>
        /// Check if it's not original window. If so close it, destroy and mark as cleared
        /// </summary>
        /// <param name="element"></param>
        /// <param name="nextId"></param>
        void CheckAndDelete(StackElement element, WindowId nextId)
        {
            element.window.MarkAsCleared();
            element.window.OnBack(nextId, () => element.window.gameObject.SetActive(false));
        }

        /// <summary>
        /// Reopen window to move back in hierarchy
        /// </summary>
        /// <param name="windowInfo"></param>
        void Reopen(StackElement windowInfo)
        {
            windowInfo.window.gameObject.SetActive(true);
            var lastId = _current;
            _current = windowInfo.id;

            windowInfo.window.OnReopen(lastId, windowInfo.windowCachedData);
        }

        #region Helpfull functions
        public static bool IsOverUIElement()
        {
            bool isOverTaggedElement = false;

            Vector2 inputPosition = Vector2.zero;
            bool isOverGameObject = false;
            #if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            int fingerId = -1;
            for (int i = 0; i < Input.touches.Length; ++i)
            {
                if (Input.touches[i].fingerId != -1)
                {
                    inputPosition = Input.touches[i].position;
                    fingerId = Input.touches[i].fingerId;
                }
            }
            isOverGameObject = EventSystem.current.IsPointerOverGameObject(fingerId);
            #elif UNITY_STANDALONE || UNITY_EDITOR
            isOverGameObject = EventSystem.current.IsPointerOverGameObject();
            inputPosition = Input.mousePosition;
            #else
            #error "Platform is not supported"
            #endif
            if (isOverGameObject)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    pointerId = -1,
                };
                pointerData.position = inputPosition;

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                if (results.Count > 0)
                {
                    for (int i = 0; i < results.Count; ++i)
                    {
                        if (results[i].gameObject.layer == _uiLayerIndex)
                        {
                            isOverTaggedElement = true;
                            break;
                        }
                    }
                }
            }

            return isOverTaggedElement;
        }
        #endregion
    }
}