#if UNITY_6000_3_OR_NEWER

using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace BrunoMikoski.SelectionHistory
{
    internal static class FavoritesToolbarElement
    {
        private const string ToolbarElementPath = "SelectionHistory/Favorites";

        [MainToolbarElement(ToolbarElementPath, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement CreateFavoritesButton()
        {
            var icon = EditorGUIUtility.IconContent("Favorite").image as Texture2D;
            if (icon == null)
                icon = EditorGUIUtility.IconContent("d_Favorite Icon").image as Texture2D;
            if (icon == null)
                icon = EditorGUIUtility.IconContent("Project").image as Texture2D;

            var content = new MainToolbarContent(icon, "★ Favorites", "Click to see your favorite assets");
            return new MainToolbarButton(content, () => FavoritesPanelWindow.ShowAtPosition(null));
        }
    }
}

#endif
