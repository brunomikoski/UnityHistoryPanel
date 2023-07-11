using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;
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

        private static VisualElement MainToolbarRoot => FetchMainToolbarRoot();
        private static VisualElement CenterRoot => FetchPlaymodeButtonsRoot();
        private static VisualElement LeftRoot => FetchZoneLeftRoot();
        private static VisualElement RightRoot => FetchZoneRightRoot();
        
        public static int GetCurrentToolbarVersion() => MainToolbarRoot.GetHashCode();

        private static Func<object> toolbarGetter;
        private static Func<object, VisualElement> rootGetter;

        public static void RemoveStockButton(string name)
        {
            MainToolbarRoot.Q<VisualElement>(name).RemoveFromHierarchy();
        }

        public static void RemovePlasticSCMButton()
        {
            LeftRoot.Q(className: "unity-imgui-container").RemoveFromHierarchy();
        }

        public static void AddCustom(TargetContainer container, Side side, VisualElement custom)
        {
            VisualElement containerElement;

            switch (container)
            {
                case TargetContainer.Left:
                    containerElement = LeftRoot;
                    break;
                case TargetContainer.Right:
                    containerElement = RightRoot;
                    break;
                case TargetContainer.Center:
                    containerElement = CenterRoot;
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
            EditorToolbarButton button = new EditorToolbarButton()
            {
                name = name,
                icon = icon
            };

            VisualElement containerElement;

            switch (container)
            {
                case TargetContainer.Left:
                    containerElement = LeftRoot;
                    break;
                case TargetContainer.Right:
                    containerElement = RightRoot;
                    break;
                case TargetContainer.Center:
                    containerElement = CenterRoot;
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


        private static VisualElement FetchZoneLeftRoot()
        {
            return MainToolbarRoot.Q<VisualElement>("ToolbarZoneLeftAlign");
        }

        private static VisualElement FetchZoneRightRoot()
        {
            return MainToolbarRoot.Q<VisualElement>("ToolbarZoneRightAlign");
        }


        private static VisualElement FetchMainToolbarRoot()
        {
            if (toolbarGetter == null || rootGetter == null)
            {
                Type type = TypeCache.GetTypesDerivedFrom<object>().First(x => x.FullName == "UnityEditor.Toolbar");
                type.GetField("get", BindingFlags.Static | BindingFlags.Public)
                    .CreateFieldAccessDelegate(out toolbarGetter);
                FieldInfo rootGetterField = type.GetField("m_Root", BindingFlags.Instance | BindingFlags.NonPublic);
                rootGetterField.CreateFieldAccessDelegate(out rootGetter);
            }

            object toolbar = toolbarGetter();
            VisualElement root = rootGetter(toolbar);
            return root;
        }

        private static VisualElement FetchPlaymodeButtonsRoot()
        {
            return MainToolbarRoot.Q<EditorToolbarToggle>("Play").parent;
        }
    }
}
