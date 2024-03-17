using System;
using System.Collections.Generic;
using WIGO.Userinterface;

namespace WIGO.Utility
{
    public partial class PushNotificationsController
    {
        private static class Actions
        {
            //public static Action OpenWindowWithOption(WindowId windowId, IDictionary<string, string> data)
            //{
            //    var pushID = data["push_id"];
            //    switch (windowId)
            //    {
            //        case WindowId.PROFILE_WINDOW:
            //            return OpenProfileWindow();
            //        case WindowId.PROFILE_SETTINGS_WINDOW:
            //            return OpenProfileSettingsWindow();
            //        case WindowId.BASE_CUSTOMIZE_WINDOW:
            //            {
            //                var option = new CustomizeWindowOption() { PushID = pushID };
            //                if (data.TryGetValue("param1", out string category))
            //                {
            //                    option.Category = category;
            //                }

            //                return OpenCustomizeCharacter(option);
            //            }
            //        case WindowId.PHOTOBOOTH_WINDOW:
            //            {
            //                var option = new PhotoboothWindowOption() { PushID = pushID };
            //                if (data.TryGetValue("param1", out string p1))
            //                {
            //                    if (Enum.TryParse(p1, out CustomizeCategoryType panel))
            //                    {
            //                        option.Panel = panel;
            //                        if (data.TryGetValue("param2", out string element))
            //                        {
            //                            option.Element = element;
            //                        }
            //                    }
            //                }

            //                return OpenPhotoboothWindow(option);
            //            }
            //        case WindowId.FEED_WINDOW:
            //            {
            //                var option = new FeedFullOption { PushID = pushID };
            //                if (data.TryGetValue("post_id", out string postID))
            //                {
            //                    option.PostID = postID;
            //                    return OpenFeedFullViewWindow(option);
            //                }
            //                return null;
            //            }
            //        default:
            //            return new Action(() => ServiceLocator.Get<UIManager>().Open(windowId,
            //                (w) => w.SetParam(new UIWindowOption() { PushID = pushID })));
            //    }
            //}

            //private static Action OpenProfileWindow()
            //{
            //    return new Action(() =>
            //        ServiceLocator.Get<UIManager>().Open<ProfileWindowController>(WindowId.PROFILE_WINDOW, callback: (w) => {
            //            w.OpenFromPush(WindowId.PROFILE_WINDOW);
            //            w.OpenPlayer().ProcessErrors();
            //        })
            //    );
            //}

            //private static Action OpenPhotoboothWindow(PhotoboothWindowOption option)
            //{
            //    return new Action(() =>
            //    {
            //        ServiceLocator.Get<UIManager>().Open<PhotoboothWindow>(WindowId.PHOTOBOOTH_WINDOW, callback: (w) =>
            //        {
            //            w.Setup();
            //            w.SetParam(option);
            //        });
            //    });
            //}

            //private static Action OpenCustomizeCharacter(CustomizeWindowOption option)
            //{
            //    return new Action(async () =>
            //    {
            //        WardrobeOpenSettings settings = null;
            //        if (option != null && !string.IsNullOrEmpty(option.Category))
            //        {
            //            settings = new WardrobeOpenSettings();
            //            await settings.SetupByCategoryWithoutItem(option.Category);
            //        }
            //        else
            //            UnityEngine.Debug.LogError("Customize window option or category is undefined");
                    
            //        _ = GameStateController.Get().ChangeState<WardrobeState>(options: new WardrobeOption()
            //        {
            //            AvatarLogic = ServiceLocator.Get<CharacterManager>().PlayerAvatar,
            //            CreateCharacter = false,
            //            Settings = settings
            //        });
            //    });
            //}

            //private static Action OpenProfileSettingsWindow()
            //{
            //    return new Action(() => ServiceLocator.Get<UIManager>().Open(WindowId.PROFILE_SETTINGS_WINDOW));
            //}

            //private static Action OpenFeedFullViewWindow(FeedFullOption option)
            //{
            //    return new Action(() => ServiceLocator.Get<UIManager>().Open<FeedFullViewWindow>(WindowId.FEED_WINDOW, (w) =>
            //    {
            //        w.SetParam(option);
            //    }));
            //}
        }
    }
}
