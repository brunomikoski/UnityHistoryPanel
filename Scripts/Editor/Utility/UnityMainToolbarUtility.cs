using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using VisualElement = UnityEngine.UIElements.VisualElement;

namespace BrunoMikoski.SelectionHistory
{
    public static class UnityMainToolbarUtility
    {
        public enum TargetContainer
        {
            Left,
            Right,
            Center
        }

        public enum Side
        {
            Left,
            Right
        }

        private static VisualElement MAIN_TOOLBAR_ROOT;
        private static VisualElement PLAYMODE_BUTTONS_ROOT;
        private static VisualElement LEFT_ZONE_ROOT;
        private static VisualElement RIGHT_ZONE_ROOT;

        
        private static Type ToolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");


        private static Func<object> toolbarGetter;
        private static Func<object, VisualElement> rootGetter;
        private static bool initialized;
        
        private static VisualElement leftZoneRootVisualElement;
        private static VisualElement rightZoneRootVisualElement;
        private static VisualElement rootVisualElement;

        public static void AddCustom(TargetContainer container, Side side, VisualElement custom)
        {
            Initialize();
            VisualElement containerElement;

            switch (container)
            {
                case TargetContainer.Left:
                    containerElement = leftZoneRootVisualElement;
                    break;
                case TargetContainer.Right:
                    containerElement = rightZoneRootVisualElement;
                    break;
                case TargetContainer.Center:
                    containerElement = rootVisualElement;
                    break;
                default:
                    throw new NotSupportedException();
            }

            switch (side)
            {
                case Side.Left:
                    containerElement.Insert(0, custom);
                    break;
                case Side.Right:
                    containerElement.Add(custom);
                    break;
            }
        }

        public static EditorToolbarButton AddButton(TargetContainer container, Side side, string name, Texture2D icon,
            Action<MouseUpEvent> callback)
        {
            Initialize();
            
            EditorToolbarButton button = new EditorToolbarButton()
            {
                name = name,
                icon = icon
            };

            VisualElement containerElement;

            switch (container)
            {
                case TargetContainer.Left:
                    containerElement = leftZoneRootVisualElement;
                    break;
                case TargetContainer.Right:
                    containerElement = rightZoneRootVisualElement;
                    break;
                case TargetContainer.Center:
                    containerElement = rootVisualElement;
                    break;
                default:
                    throw new NotSupportedException();
            }

            switch (side)
            {
                case Side.Left:
                    containerElement.Insert(0, button);
                    break;
                case Side.Right:
                    containerElement.Add(button);
                    break;
            }

            button.clickable.clickedWithEventInfo += x => callback((MouseUpEvent) x);
            EditorToolbarUtility.SetupChildrenAsButtonStrip(containerElement);
            return button;
        }

        private static void Initialize()
        {
            if (initialized)
                return;
            
            Object[] toolbars = Resources.FindObjectsOfTypeAll(ToolbarType);

            if (toolbars.Length == 0)
                return;
            
            Object toolbar = toolbars[0];
            
            FieldInfo rootField = toolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            rootVisualElement = rootField.GetValue(toolbar) as VisualElement;
            leftZoneRootVisualElement = rootVisualElement.Q<VisualElement>("ToolbarZoneLeftAlign");
            rightZoneRootVisualElement = rootVisualElement.Q<VisualElement>("ToolbarZoneLeftAlign");
            
            initialized = true;
        }
    }
}
