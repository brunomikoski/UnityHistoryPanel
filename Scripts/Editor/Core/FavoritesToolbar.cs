using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BrunoMikoski.SelectionHistory
{
    [InitializeOnLoad]
    internal static class FavoritesToolbar
    {
        private static bool pendingOpen;
        private static Rect pendingRect;
        private static readonly AdvancedDropdownState CACHED_STATE = new AdvancedDropdownState();

        static FavoritesToolbar()
        {
            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
            VisualElement parent = new VisualElement
            {
                style = { flexGrow = 0, flexDirection = FlexDirection.Row, flexShrink = 1, minWidth = 0 }
            };

            ToolbarMenu favoritesDropdown = new ToolbarMenu
            {
                tooltip = "Click to see your favorite assets",
                text = "Favorites "
            };
            ApplyToolbarStyle(favoritesDropdown);

            favoritesDropdown.RegisterCallback<MouseUpEvent>(e =>
            {
                if (e.button != 0)
                    return;

                pendingRect = favoritesDropdown.worldBound;
                pendingOpen = true;
            });

            parent.Add(favoritesDropdown);

            IMGUIContainer opener = new IMGUIContainer();
            opener.style.width = 0;
            opener.style.height = 0;
            opener.onGUIHandler = () =>
            {
                if (!pendingOpen)
                    return;
                pendingOpen = false;

                Vector2 guiPos = GUIUtility.ScreenToGUIPoint(pendingRect.position);
                Rect guiRect = new Rect(guiPos.x, guiPos.y + pendingRect.height, pendingRect.width, 0f);
                FavoritesAdvancedDropdown dropdown = new FavoritesAdvancedDropdown(CACHED_STATE);
                dropdown.SetFavorites(FavoritesManager.GetManualFavoriteEntries(), FavoritesManager.GetLearnedFavorites());
                dropdown.Show(guiRect);
            };
            parent.Add(opener);

            UnityMainToolbarUtility.AddCustom(UnityMainToolbarUtility.TargetContainer.Left,
                UnityMainToolbarUtility.Side.Right,
                parent, 4);
        }

        private static void ApplyToolbarStyle(VisualElement element)
        {
            element.AddToClassList("unity-toolbar-button");
            element.AddToClassList("unity-editor-toolbar-element");
            element.RemoveFromClassList("unity-button");
            element.style.paddingRight = 8;
            element.style.paddingLeft = 8;
            element.style.justifyContent = Justify.Center;
            element.style.display = DisplayStyle.Flex;
            element.style.borderTopLeftRadius = 2;
            element.style.borderTopRightRadius = 2;
            element.style.borderBottomLeftRadius = 2;
            element.style.borderBottomRightRadius = 2;
            element.style.height = 19;
            element.style.marginRight = 1;
            element.style.marginLeft = 1;
        }
    }
}
