using System.Collections;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.EditorHistoryNavigation
{
    public sealed class HistoryPanelEditorWindow : EditorWindow
    {
        private const string BACK_BUTTON_HOTKEY_KEY = "BACK_BUTTON_HOTKEY_KEY";
        private const string FORWARD_BUTTON_HOTKEY_KEY = "FORWARD_BUTTON_HOTKEY_KEY";
        
        
        private HotKeyData cachedBackButton;
        private HotKeyData BackButtonHotkey
        {
            get
            {
                if (cachedBackButton != null)
                    return cachedBackButton;
                string json = EditorPrefs.GetString(BACK_BUTTON_HOTKEY_KEY, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    if (cachedBackButton == null)
                        cachedBackButton = new HotKeyData();
                    EditorJsonUtility.FromJsonOverwrite(json, cachedBackButton);
                }

                return cachedBackButton;

            }
            set
            {
                cachedBackButton = value;
                if (cachedBackButton != null)
                    EditorPrefs.SetString(BACK_BUTTON_HOTKEY_KEY, EditorJsonUtility.ToJson(cachedBackButton));
                else
                    EditorPrefs.DeleteKey(BACK_BUTTON_HOTKEY_KEY);
            }
        }

        private HotKeyData cachedForwardButton;
        private HotKeyData ForwardButtonHotkey
        {
            get
            {
                if (cachedForwardButton != null)
                    return cachedForwardButton;
                string json = EditorPrefs.GetString(FORWARD_BUTTON_HOTKEY_KEY, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    if (cachedForwardButton == null)
                        cachedForwardButton = new HotKeyData();
                    
                    EditorJsonUtility.FromJsonOverwrite(json, cachedForwardButton);
                }

                return cachedBackButton;

            }
            set
            {
                cachedForwardButton = value;
                if (cachedForwardButton != null)
                    EditorPrefs.SetString(FORWARD_BUTTON_HOTKEY_KEY, EditorJsonUtility.ToJson(cachedForwardButton));
                else
                    EditorPrefs.DeleteKey(FORWARD_BUTTON_HOTKEY_KEY);
            }
        }


        private HotkeyDataGUI backButtonHotkeyDataGui;
        private HotkeyDataGUI forwardButtonHotkeyDataGui;
        private EditorApplication.CallbackFunction eventHandlerCallbackFunction;
        private bool showSettings;
        private bool showHistory = true;
        private Vector2 scrollView;

        private void OnSelectionChange()
        {
            HistoryPanelCore.OnSelectionChanged();
            Repaint();
        }

        private void OnEnable()
        {
            backButtonHotkeyDataGui = new HotkeyDataGUI("Back");
            forwardButtonHotkeyDataGui = new HotkeyDataGUI("Forward");
            
            System.Reflection.FieldInfo info = typeof (EditorApplication).GetField ("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
 
            eventHandlerCallbackFunction = (EditorApplication.CallbackFunction)info.GetValue (null);
 
            eventHandlerCallbackFunction += CheckForShortcuts;
 
            info.SetValue (null, eventHandlerCallbackFunction);
        }

        private void OnDisable()
        {
            if (eventHandlerCallbackFunction != null)
                eventHandlerCallbackFunction -= CheckForShortcuts;
        }
        
        private void CheckForShortcuts()
        {
            if (Event.current == null)
                return;
            
            if (Event.current.isKey)
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    if (BackButtonHotkey != null && BackButtonHotkey.Match(Event.current))
                    {
                        GoBack();
                    }
                    else if (ForwardButtonHotkey != null && ForwardButtonHotkey.Match(Event.current))
                    {
                        GoForward();
                    }
                }
            }

            if (Event.current.type == EventType.MouseDown)
            {
                Debug.Log(Event.current.button);
            }
        }

        private void GoBack()
        {
            HistoryPanelCore.GoBack();
        }

        private void GoForward()
        {
            HistoryPanelCore.GoForward();

        }

        private void OnGUI()
        {
            DrawSettingsGUI();
            
            DrawHistoryGUI();
        }

        private void DrawHistoryGUI()
        {
            scrollView = EditorGUILayout.BeginScrollView(scrollView, false, false);

            using (new EditorGUILayout.VerticalScope("Box"))
            {
                showHistory = EditorGUILayout.BeginFoldoutHeaderGroup(showHistory, "History");
                {
                    if (showHistory)
                    {
                        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

                        EditorGUI.indentLevel++;
                        for (int i = 0; i < HistoryPanelCore.history.SelectionData.Count; i++)
                        {
                            HistoryPanelCore.SelectionData selectionData =
                                HistoryPanelCore.history.SelectionData[i];

                            DrawSelectionData(selectionData, i, HistoryPanelCore.history.PointInTime);
                        }
                        EditorGUI.indentLevel--;

                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSelectionData(HistoryPanelCore.SelectionData selectionData, int itemIndex, int pointInTime)
        {
            Color guiColor = GUI.color;
            Color backgroundColor = GUI.backgroundColor;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10);

                if (itemIndex == pointInTime)
                    GUI.backgroundColor = new Color(0.43f, 0.59f, 0.84f);

                bool isEnabled = itemIndex <= pointInTime;

                Color color = GUI.color;
                color.a = isEnabled ? 1 : 0.3f;
                GUI.color = color;
                
                if (GUILayout.Button(selectionData.GUIContent, HistoryGUIStyles.HistoryButton))
                    HistoryPanelCore.SetPointInTime(itemIndex);
            }

            GUI.color = guiColor;
            GUI.backgroundColor = backgroundColor;
        }

        private void DrawSettingsGUI()
        {
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                showSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showSettings, "Settings");
                {
                    if (showSettings)
                    {
                        EditorGUI.indentLevel++;
                        if (backButtonHotkeyDataGui.DrawAndGetNewHotkey(BackButtonHotkey, out HotKeyData newBackButtonHotkey))
                        {
                            BackButtonHotkey = newBackButtonHotkey;
                        }

                        if (forwardButtonHotkeyDataGui.DrawAndGetNewHotkey(ForwardButtonHotkey, out HotKeyData newForwardButton))
                        {
                            ForwardButtonHotkey = newForwardButton;
                        }

                        if (GUILayout.Button("Clear History"))
                        {
                            HistoryPanelCore.ClearHistory();
                        }
                        
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }
        }

        [MenuItem("Tools/Open Navigation Panel")]
        private static void Init()
        {
            HistoryPanelEditorWindow window = (HistoryPanelEditorWindow) GetWindow(typeof(HistoryPanelEditorWindow));
            window.Show();
            window.titleContent = new GUIContent("Navigation Panel");
        }
        
    }
}
