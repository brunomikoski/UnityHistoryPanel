using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.EditorHistoryNavigation
{
    public class HotkeyDataGUI
    {
        private HotKeyData editingHotkey;
        private bool isListening;
        private string title;

        public HotkeyDataGUI(string title)
        {
            this.title = title;
        }
        
        public bool DrawAndGetNewHotkey(HotKeyData targetHotkeyData, out HotKeyData newHotkeyData)
        {
            using (new EditorGUILayout.HorizontalScope("Box"))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

                Color color = GUI.color;
                if (isListening)
                {
                    GUI.color = Color.red;

                    if (GUILayout.Button("Cancel"))
                    {
                        isListening = false;
                    }
                }
                else
                {
                    GUI.color = color;

                    if (targetHotkeyData != null)
                    {
                        if (GUILayout.Button(targetHotkeyData.ToDisplay()))
                        {
                            isListening = true;
                            
                        }
                    }
                    else
                    {
                        GUI.color = Color.green;

                        if (GUILayout.Button("Assing Hotkey"))
                        {
                            isListening = true;
                        }
                    }
                }

                GUI.color = color;

                if (isListening)
                {
                    if (targetHotkeyData != null)
                        editingHotkey = targetHotkeyData;
                    else
                        editingHotkey = new HotKeyData(KeyCode.None, EventModifiers.None);
                }
            }

            if (isListening)
            {
                if (Event.current != null)
                {
                    if (Event.current.isKey)
                    {
                        editingHotkey = new HotKeyData(Event.current.keyCode, Event.current.modifiers);
                        if (Event.current.type == EventType.KeyUp)
                        {
                            isListening = false;
                            Event.current.Use();
                            newHotkeyData = editingHotkey;
                            return true;
                        }
                    }
                }
            }

            newHotkeyData = null;
            return false;
        }
    }
}
