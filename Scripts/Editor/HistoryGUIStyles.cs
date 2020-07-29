using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.EditorHistoryNavigation
{
    public static class HistoryGUIStyles
    {

        private static GUIStyle cachedHistoryButton;
        public static GUIStyle HistoryButton
        {
            get
            {
                if (cachedHistoryButton != null)
                    return cachedHistoryButton;
                cachedHistoryButton = EditorStyles.miniButton;
                cachedHistoryButton.alignment = TextAnchor.MiddleLeft;
                cachedHistoryButton.fixedHeight = 28;
                RectOffset border = cachedHistoryButton.border;
                border.bottom = border.top = 10;
                cachedHistoryButton.border = border;
                return cachedHistoryButton;
            }
        }
    }
}