using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.EditorHistoryNavigation
{
    public sealed class HistoryPanelEditorWindow : EditorWindow
    {
        private bool showHistory = true;
        private bool showSettings;
        private Vector2 scrollView;

        private void OnSelectionChange()
        {
            HistoryPanelCore.OnHistoryChangedEvent += Repaint;
        }

        private void OnGUI()
        {
            DrawHistoryGUI();
        }

        private void DrawHistoryGUI()
        {
            scrollView = EditorGUILayout.BeginScrollView(scrollView, false, false);

            showSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showSettings, "Settings");
            if (showSettings)
            {
                using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                {
                    int maximumHistory = EditorGUILayout.IntField("Maximum History Items", HistoryPanelCore.MaximumHistoryItems);
                    if (changeCheckScope.changed)
                    {
                        HistoryPanelCore.SetMaximumHistory(maximumHistory);
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                showHistory = EditorGUILayout.BeginFoldoutHeaderGroup(showHistory, "History");
                if (showHistory)
                {
                    EditorGUIUtility.SetIconSize(new Vector2(16, 16));

                    EditorGUI.indentLevel++;
                    for (int i = 0; i < HistoryPanelCore.history.SelectionData.Count; i++)
                    {
                        SelectionData selectionData =
                            HistoryPanelCore.history.SelectionData[i];

                        DrawSelectionData(selectionData, i, HistoryPanelCore.history.PointInTime);
                    }

                    EditorGUI.indentLevel--;

                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSelectionData(SelectionData selectionData, int itemIndex, int pointInTime)
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

        [MenuItem("Tools/History/Open History")]
        private static void Init()
        {
            HistoryPanelEditorWindow window = (HistoryPanelEditorWindow) GetWindow(typeof(HistoryPanelEditorWindow));
            window.Show();
            window.titleContent = new GUIContent("History");
        }
        
    }
}
