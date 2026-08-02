using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BrunoMikoski.SelectionHistory
{
    [InitializeOnLoad]
    internal class SelectionHistoryToolbar
    {
#if !UNITY_6000_3_OR_NEWER
        private static ToolbarMenu HISTORY_SELECTION_MENU;
#endif

        private static readonly string HISTORY_STORAGE_KEY = Application.productName + "EditorHistoryKey";
        private const string MAX_HISTORY_ITEMS_KEY = "MaxHistoryItemsKey";

        private static SelectionHistoryData CACHED_HISTORY;
        private static SelectionHistoryData History
        {
            get
            {
                if (CACHED_HISTORY != null)
                    return CACHED_HISTORY;

                CACHED_HISTORY = new SelectionHistoryData();
                string historyJson = SessionState.GetString(HISTORY_STORAGE_KEY, string.Empty);
                if (!string.IsNullOrEmpty(historyJson))
                    EditorJsonUtility.FromJsonOverwrite(historyJson, CACHED_HISTORY);
                return CACHED_HISTORY;
            }
        }


        private static int? CACHED_MAXIMUM_HISTORY_ITEMS;

        public static int MaximumHistoryItems
        {
            get
            {
                if (CACHED_MAXIMUM_HISTORY_ITEMS.HasValue)
                    return CACHED_MAXIMUM_HISTORY_ITEMS.Value;
                CACHED_MAXIMUM_HISTORY_ITEMS = EditorPrefs.GetInt(MAX_HISTORY_ITEMS_KEY, 30);
                return CACHED_MAXIMUM_HISTORY_ITEMS.Value;
            }
        }

#if !UNITY_6000_3_OR_NEWER
        private static VisualElement backButton;
        private static VisualElement forwardButton;
#endif


        static SelectionHistoryToolbar()
        {
            if (Application.isBatchMode)
                return;

            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
#if !UNITY_6000_3_OR_NEWER
            VisualElement parent = new VisualElement()
            {
                style =
                {
                    flexGrow = 0,
                    flexDirection = FlexDirection.Row,
                },
            };

            parent.Add(new VisualElement()
            {
                style =
                {
                    flexGrow = 1,
                },
            });


            HISTORY_SELECTION_MENU = new ToolbarMenu
            {
                visible = false,
            };

            HISTORY_SELECTION_MENU.menu.AppendAction("Default is never shown", a => { },
                a => DropdownMenuAction.Status.None);

            parent.Add(HISTORY_SELECTION_MENU);
            backButton = AddButton("d_tab_prev@2x", "Go Back in Selection History", GoBack, ShowBackwardsHistory);
            parent.Add(backButton);
            forwardButton = AddButton("d_tab_next@2x", "Go Forward in Selection History", GoForward, ShowForwardHistory);
            parent.Add(forwardButton);


            UnityMainToolbarUtility.AddCustom(UnityMainToolbarUtility.TargetContainer.Left,
                UnityMainToolbarUtility.Side.Right, parent, 3);
#endif

            EditorApplication.playModeStateChanged += EditorApplicationOnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += SaveHistory;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj != PlayModeStateChange.ExitingPlayMode && obj != PlayModeStateChange.ExitingEditMode)
                return;

            SaveHistory();
        }

        private static void SaveHistory()
        {
            if (CACHED_HISTORY == null)
                return;

            string json = EditorJsonUtility.ToJson(CACHED_HISTORY);
            SessionState.SetString(HISTORY_STORAGE_KEY, json);
        }

        private static void OnSelectionChanged()
        {
            History.AddToHistory(Selection.objects);
            UpdateButtonsVisibility();
        }

        private static void UpdateButtonsVisibility()
        {
#if UNITY_6000_3_OR_NEWER
            SelectionHistoryToolbarElement.RefreshToolbar();
#else
            backButton.SetEnabled(CanGoBack);
            forwardButton.SetEnabled(CanGoForward);
#endif
        }

        internal static bool CanGoBack =>
            History.SelectionData.Count > 1 && History.PointInTime > 0;

        internal static bool CanGoForward =>
            History.SelectionData.Count > 1 && History.PointInTime < History.SelectionData.Count - 1;

        [MenuItem("Tools/Selection History/Go Back")]
        public static void GoBack()
        {
            History.Back();
        }

        [MenuItem("Tools/Selection History/Go Forward")]
        public static void GoForward()
        {
            History.Forward();
        }

        [MenuItem("Tools/Selection History/Clear History")]
        public static void ClearHistory()
        {
            SessionState.EraseString(HISTORY_STORAGE_KEY);
            CACHED_HISTORY = new SelectionHistoryData();
            UpdateButtonsVisibility();
        }

        private static void SetPointInTime(int itemIndex)
        {
            History.SetPointInTime(itemIndex);
        }

        internal static void PopulateBackwardsHistory(DropdownMenu menu)
        {
            for (int i = History.PointInTime-1; i >= 0; i--)
            {
                SelectionData selectionData = History.SelectionData[i];

                if (!selectionData.IsValid)
                    continue;

                int targetIndex = i;
                menu.AppendAction(selectionData.DisplayName, a =>
                {
                    SetPointInTime(targetIndex);
                });
            }

            AppendClearHistory(menu);
        }

        internal static void PopulateForwardHistory(DropdownMenu menu)
        {
            for (int i = History.PointInTime+1; i < History.SelectionData.Count; i++)
            {
                SelectionData selectionData = History.SelectionData[i];

                if (!selectionData.IsValid)
                    continue;

                int targetIndex = i;
                menu.AppendAction(selectionData.DisplayName, a =>
                {
                    SetPointInTime(targetIndex);
                });
            }

            AppendClearHistory(menu);
        }

        private static void AppendClearHistory(DropdownMenu menu)
        {
            menu.AppendSeparator();
            menu.AppendAction("Clear History", a =>
            {
                ClearHistory();
            }, a => DropdownMenuAction.Status.Normal);
        }

#if !UNITY_6000_3_OR_NEWER
        private static void ShowBackwardsHistory()
        {
            HISTORY_SELECTION_MENU.menu.MenuItems().Clear();
            PopulateBackwardsHistory(HISTORY_SELECTION_MENU.menu);
            HISTORY_SELECTION_MENU.ShowMenu();
        }

        private static void ShowForwardHistory()
        {
            HISTORY_SELECTION_MENU.menu.MenuItems().Clear();
            PopulateForwardHistory(HISTORY_SELECTION_MENU.menu);
            HISTORY_SELECTION_MENU.ShowMenu();
        }
#endif


        #region UI Elements visuals

#if !UNITY_6000_3_OR_NEWER
        private static VisualElement AddButton(string iconName, string tooltip, Action leftMouseClickCallback,
            Action rightMouseClickCallback = null)
        {
            Button button = new Button()
            {
                tooltip = tooltip
            };
            button.clickable.activators.Clear();
            button.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 1 && rightMouseClickCallback != null)
                    rightMouseClickCallback();
                else
                    leftMouseClickCallback();
            });

            FitChildrenStyle(button);

            VisualElement icon = new VisualElement();
            icon.AddToClassList("unity-editor-toolbar-element__icon");
            icon.style.backgroundImage =
                Background.FromTexture2D((Texture2D) EditorGUIUtility.IconContent(iconName).image);
            icon.style.height = 12;
            icon.style.width = 12;
            icon.style.alignSelf = Align.Center;
            button.Add(icon);

            return button;
        }


        private static void FitChildrenStyle(VisualElement element)
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

        #endregion
    }
}