using System.Collections.Generic;
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
            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
            VisualElement parent = new VisualElement
            {
                style =
                {
                    flexGrow = 0,
                    flexDirection = FlexDirection.Row
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
                if (e.button == 0)
                    ShowAssetsMenu(favoritesDropdown);
            });

            parent.Add(favoritesDropdown);

            UnityMainToolbarUtility.AddCustom(UnityMainToolbarUtility.TargetContainer.Left, UnityMainToolbarUtility.Side.Right, parent);
        }

        private static void ShowAssetsMenu(ToolbarMenu menu)
        {
            Rect rect = menu.worldBound;
            GenericMenu genericMenu = new GenericMenu();
            List<Object> manual = FavoritesManager.GetManualFavorites();
            foreach (Object asset in manual)
            {
                Object currentAsset = asset;
                GUIContent content = new GUIContent(currentAsset.name, EditorGUIUtility.ObjectContent(currentAsset, currentAsset.GetType()).image);
                genericMenu.AddItem(content, false, () =>
                {
                    Selection.activeObject = currentAsset;
                    EditorGUIUtility.PingObject(currentAsset);
                });
            }
            if (manual.Count > 0)
                genericMenu.AddSeparator("");
            List<Object> learned = FavoritesManager.GetLearnedFavorites();
            foreach (Object asset in learned)
            {
                if (FavoritesManager.IsManualFavorite(asset))
                    continue;
                Object currentAsset = asset;
                GUIContent content = new GUIContent(currentAsset.name, EditorGUIUtility.ObjectContent(currentAsset, currentAsset.GetType()).image);
                genericMenu.AddItem(content, false, () =>
                {
                    Selection.activeObject = currentAsset;
                    EditorGUIUtility.PingObject(currentAsset);
                });
            }
            genericMenu.DropDown(rect);
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