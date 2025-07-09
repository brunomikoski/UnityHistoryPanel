using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory
{
    [InitializeOnLoad]
    internal static class FavoritesToolbar
    {
        static FavoritesToolbar()
        {
            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
            VisualElement parent = new VisualElement
            {
                style =
                {
                    flexGrow = 0,
                    flexDirection = FlexDirection.Row,
                    flexShrink = 1,
                    minWidth = 0
                }
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

                EditorApplication.delayCall += () =>
                {
                    Rect rect = favoritesDropdown.worldBound;
                    AdvancedDropdownState state = new AdvancedDropdownState();
                    FavoritesAdvancedDropdown dropdown = new FavoritesAdvancedDropdown(state);
                    dropdown.SetFavorites(FavoritesManager.GetManualFavorites(),
                        FavoritesManager.GetLearnedFavorites());
                    dropdown.Show(rect);
                };
            });

            parent.Add(favoritesDropdown);
            UnityMainToolbarUtility.AddCustom(UnityMainToolbarUtility.TargetContainer.Left, UnityMainToolbarUtility.Side.Right, parent, 4);
        }

        private static void ShowAssetsMenu(ToolbarMenu menu)
        {
            Rect rect = menu.worldBound;
            AdvancedDropdownState state = new AdvancedDropdownState();
            FavoritesAdvancedDropdown dropdown = new FavoritesAdvancedDropdown(state);
            dropdown.SetFavorites(FavoritesManager.GetManualFavorites(), FavoritesManager.GetLearnedFavorites());
            dropdown.Show(rect);
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
