using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BrunoMikoski.SelectionHistory
{
    [InitializeOnLoad]
    internal static class FavoritesToolbar
    {
        static FavoritesToolbar()
        {
#if !UNITY_6000_3_OR_NEWER
            EditorApplication.delayCall += Initialize;
#endif
        }

#if !UNITY_6000_3_OR_NEWER
        private static void Initialize()
        {
            VisualElement parent = new VisualElement
            {
                style = { flexGrow = 0, flexDirection = FlexDirection.Row, flexShrink = 1, minWidth = 0 }
            };

            var favoritesButton = new ToolbarButton(OpenFavoritesPanel)
            {
                tooltip = "Click to see your favorite assets",
                text = "★ Favorites"
            };
            ApplyToolbarStyle(favoritesButton);

            parent.Add(favoritesButton);

            UnityMainToolbarUtility.AddCustom(UnityMainToolbarUtility.TargetContainer.Left,
                UnityMainToolbarUtility.Side.Right,
                parent, 4);
        }

        private static void OpenFavoritesPanel()
        {
            FavoritesPanelWindow.ShowAtPosition(null);
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
#endif
    }
}
