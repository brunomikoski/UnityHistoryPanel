using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory
{
    public static class UnityMainToolbarUtility
    {
        public enum TargetContainer { Left, Right, Center }
        public enum Side { Left, Right }

        private static VisualElement MAIN_TOOLBAR_ROOT;
        private static VisualElement LEFT_ZONE_ROOT;
        private static VisualElement RIGHT_ZONE_ROOT;
        private static VisualElement rootVisualElement;
        private static Type ToolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static bool initialized;

        public static void AddCustom(TargetContainer container, Side side, VisualElement custom, int position)
        {
            if (!Initialize())
                return;
            
            VisualElement containerElement;
            switch (container)
            {
                case TargetContainer.Left:
                    containerElement = LEFT_ZONE_ROOT;
                    break;
                case TargetContainer.Right:
                    containerElement = RIGHT_ZONE_ROOT;
                    break;
                case TargetContainer.Center:
                    containerElement = rootVisualElement;
                    break;
                default:
                    throw new NotSupportedException();
            }
            int index = Mathf.Clamp(position, 0, containerElement.childCount);
            containerElement.Insert(index, custom);
        }

        public static EditorToolbarButton AddButton(TargetContainer container, Side side, string name, Texture2D icon, Action<MouseUpEvent> callback)
        {
            if (!Initialize())
                return null;
            
            EditorToolbarButton button = new EditorToolbarButton() { name = name, icon = icon };
            VisualElement containerElement;
            switch (container)
            {
                case TargetContainer.Left:
                    containerElement = LEFT_ZONE_ROOT;
                    break;
                case TargetContainer.Right:
                    containerElement = RIGHT_ZONE_ROOT;
                    break;
                case TargetContainer.Center:
                    containerElement = rootVisualElement;
                    break;
                default:
                    throw new NotSupportedException();
            }
            containerElement.Add(button);
            button.clickable.clickedWithEventInfo += x => callback((MouseUpEvent)x);
            EditorToolbarUtility.SetupChildrenAsButtonStrip(containerElement);
            return button;
        }

        private static bool Initialize()
        {
            if (initialized)
                return true;
            Object[] toolbars = Resources.FindObjectsOfTypeAll(ToolbarType);
            if (toolbars.Length == 0)
                return false;
            
            Object toolbar = toolbars[0];
            FieldInfo rootField = toolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            rootVisualElement = rootField.GetValue(toolbar) as VisualElement;
            LEFT_ZONE_ROOT = rootVisualElement.Q<VisualElement>("ToolbarZoneLeftAlign");
            RIGHT_ZONE_ROOT = rootVisualElement.Q<VisualElement>("ToolbarZoneRightAlign");
            initialized = true;
            return true;
        }
    }
}
