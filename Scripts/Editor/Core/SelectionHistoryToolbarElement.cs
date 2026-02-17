#if UNITY_6000_3_OR_NEWER

using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace BrunoMikoski.SelectionHistory
{
    internal static class SelectionHistoryToolbarElement
    {
        private const string BackButtonPath = "SelectionHistory/Back";
        private const string ForwardButtonPath = "SelectionHistory/Forward";

        [MainToolbarElement(BackButtonPath, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement CreateBackButton()
        {
            var icon = EditorGUIUtility.IconContent("d_tab_prev@2x").image as Texture2D;
            if (icon == null)
                icon = EditorGUIUtility.IconContent("tab_prev").image as Texture2D;

            var content = new MainToolbarContent("", icon, "Go Back in Selection History");

            bool canGoBack = SelectionHistoryToolbar.CanGoBack;
            var button = new MainToolbarButton(content, SelectionHistoryToolbar.GoBack)
            {
                enabled = canGoBack
            };
            return button;
        }

        [MainToolbarElement(ForwardButtonPath, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement CreateForwardButton()
        {
            var icon = EditorGUIUtility.IconContent("d_tab_next@2x").image as Texture2D;
            if (icon == null)
                icon = EditorGUIUtility.IconContent("tab_next").image as Texture2D;

            var content = new MainToolbarContent("", icon, "Go Forward in Selection History");

            bool canGoForward = SelectionHistoryToolbar.CanGoForward;
            var button = new MainToolbarButton(content, SelectionHistoryToolbar.GoForward)
            {
                enabled = canGoForward
            };
            return button;
        }

        public static void RefreshToolbar()
        {
            MainToolbar.Refresh(BackButtonPath);
            MainToolbar.Refresh(ForwardButtonPath);
        }
    }
}

#endif
