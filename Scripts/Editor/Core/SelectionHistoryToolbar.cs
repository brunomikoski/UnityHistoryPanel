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
        private static ToolbarMenu HISTORY_SELECTION_MENU;

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

        private static VisualElement backButton;
        private static VisualElement forwardButton;


        static SelectionHistoryToolbar()
        {
            if (Application.isBatchMode)
                return;

            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
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
                UnityMainToolbarUtility.Side.Right, parent);

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
            backButton.SetEnabled(History.SelectionData.Count > 1 && History.PointInTime > 0);
            forwardButton.SetEnabled(History.SelectionData.Count > 1 && History.PointInTime < History.SelectionData.Count - 1);
        }

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

        private static void ClearHistory()
        {
            EditorPrefs.DeleteKey(HISTORY_STORAGE_KEY);
            CACHED_HISTORY = new SelectionHistoryData();
            UpdateButtonsVisibility();
        }

        private static void SetPointInTime(int itemIndex)
        {
            History.SetPointInTime(itemIndex);
        }

        private static void ShowBackwardsHistory()
        {
            HISTORY_SELECTION_MENU.menu.MenuItems().Clear();

            for (int i = History.PointInTime-1; i >= 0; i--)
            {
                SelectionData selectionData = History.SelectionData[i];

                if (!selectionData.IsValid)
                    continue;

                int targetIndex = i;
                HISTORY_SELECTION_MENU.menu.AppendAction(selectionData.DisplayName, a =>
                {
                    SetPointInTime(targetIndex);
                });
            }


            HISTORY_SELECTION_MENU.menu.AppendSeparator();
            HISTORY_SELECTION_MENU.menu.AppendAction("Clear History", a =>
            {
                ClearHistory();
            }, a => DropdownMenuAction.Status.Normal);

            HISTORY_SELECTION_MENU.ShowMenu();
        }
        
        private static void ShowForwardHistory()
        {
            HISTORY_SELECTION_MENU.menu.MenuItems().Clear();

            for (int i = History.PointInTime+1; i < History.SelectionData.Count; i++)
            {
                SelectionData selectionData = History.SelectionData[i];

                if (!selectionData.IsValid)
                    continue;


                int targetIndex = i;
                HISTORY_SELECTION_MENU.menu.AppendAction(selectionData.DisplayName, a =>
                {
                    SetPointInTime(targetIndex);
                });
            }

            HISTORY_SELECTION_MENU.menu.AppendSeparator();
            HISTORY_SELECTION_MENU.menu.AppendAction("Clear History", a =>
            {
                ClearHistory();
            }, a => DropdownMenuAction.Status.Normal);

            HISTORY_SELECTION_MENU.ShowMenu();
        }


        #region UI Elements visuals

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

        #endregion
    }
}