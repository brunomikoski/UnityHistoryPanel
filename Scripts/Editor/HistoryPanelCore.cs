using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.EditorHistoryNavigation
{
    [InitializeOnLoad]
    internal static class HistoryPanelCore 
    {
        [Serializable]
        internal class SelectionData : ISerializationCallbackReceiver
        {
            private Object[] selection;

            [SerializeField]
            private List<string> guids = new List<string>();

            [SerializeField]
            private List<int> instanceIDs = new List<int>();

            [SerializeField]
            private string displayName;

            private GUIContent cachedGUIContent;
            public GUIContent GUIContent
            {
                get
                {
                    if (cachedGUIContent != null) 
                        return cachedGUIContent;
                    
                    Object first = selection.First();
                    cachedGUIContent = new GUIContent(displayName, EditorGUIUtility.ObjectContent(first, first.GetType()).image);
                    return cachedGUIContent;
                }
            }
            
            public bool IsValid => selection.Any(selectionObj => selectionObj != null);

            public SelectionData(Object[] objects)
            {
                selection = objects;

                displayName = string.Empty;
                for (int i = 0; i < selection.Length; i++)
                {
                    Object o = selection[i];
                    if (o == null)
                        continue;

                    if (o is GameObject gameObject)
                    {
                        instanceIDs.Add(gameObject.GetInstanceID());
                    }
                    else
                    {
                        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
                        guids.Add(guid);
                    }
                    
                }

                displayName = string.Join(",", selection.Where(o => o != null).Select(o => o.name));
                if (displayName.Length > 50)
                    displayName = displayName.Substring(0, 47) + "...";
            }
            
            public void Select()
            {
                Selection.objects = selection.Where(o => o != null).ToArray();
            }

            void ISerializationCallbackReceiver.OnBeforeSerialize()
            {
            }

            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {
                List<Object> storedObjs = new List<Object>();
                for (int i = 0; i < guids.Count; i++)
                    storedObjs.Add(AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guids[i])));

                for (int i = 0; i < instanceIDs.Count; i++)
                    storedObjs.Add(EditorUtility.InstanceIDToObject(instanceIDs[i]));

                selection = storedObjs.ToArray();
            }
        }
        
        [Serializable]
        internal class History
        {
            [SerializeField]
            private List<SelectionData> selectionData = new List<SelectionData>();
            public List<SelectionData> SelectionData => selectionData;

            [SerializeField]
            private int pointInTime = 0;
            public int PointInTime => pointInTime;

            private bool movingInHistory;

            public void AddToHistory(Object[] objects)
            {
                if (movingInHistory)
                {
                    movingInHistory = false;
                    return;
                }

                SelectionData item = new SelectionData(objects);
                if (!item.IsValid)
                    return;

                if (pointInTime < selectionData.Count - 1)
                    selectionData.RemoveRange(pointInTime, selectionData.Count - 1 - pointInTime);

                selectionData.Add(item);
                if (selectionData.Count > MaximumHistoryItems)
                    selectionData.RemoveAt(0);

                pointInTime = selectionData.Count - 1;

            }

            public void Back()
            {
                if (pointInTime == 0)
                    return;
                
                for (int i = pointInTime-1; i >= 0; i--)
                {
                    SelectionData data = selectionData[i];
                    if (!data.IsValid)
                        continue;

                    pointInTime = i;
                    movingInHistory = true;
                    data.Select();
                    break;
                }
            }

            public void Forward()
            {
                if (pointInTime == selectionData.Count - 1)
                    return;

                if (pointInTime + 1 > selectionData.Count - 1)
                    return;

                pointInTime++;
                
                movingInHistory = true;
                selectionData[pointInTime].Select();
            }

            public void SetPointInTime(int itemIndex)
            {
                movingInHistory = true;
                pointInTime = itemIndex;
                selectionData[pointInTime].Select();
            }
        }

        private static int MaximumHistoryItems => 30;

        private static string HISTORY_STORAGE_KEY = Application.productName + "EditorHistoryKey";

        private static History cachedHistory;
        public static History history
        {
            get
            {
                if (cachedHistory != null) 
                    return cachedHistory;

                cachedHistory = new History();
                string historyJson = EditorPrefs.GetString(HISTORY_STORAGE_KEY, string.Empty);

                if (!string.IsNullOrEmpty(historyJson))
                    EditorJsonUtility.FromJsonOverwrite(historyJson, cachedHistory);

                return cachedHistory;
            }
        }
        
        static HistoryPanelCore()
        {
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += SaveHistory;
        }

        private static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj != PlayModeStateChange.ExitingPlayMode && obj != PlayModeStateChange.ExitingEditMode) 
                return;
            SaveHistory();
        }

        private static void SaveHistory()
        {
            if (cachedHistory == null)
                return;

            string json = EditorJsonUtility.ToJson(cachedHistory);
            EditorPrefs.SetString(HISTORY_STORAGE_KEY, json);
        }

        public static void OnSelectionChanged()
        {
            history.AddToHistory(Selection.objects);
        }

        public static void GoBack()
        {
            history.Back();
        }

        public static void GoForward()
        {
            history.Forward();
        }

        public static void ClearHistory()
        {
            EditorPrefs.DeleteKey(HISTORY_STORAGE_KEY);
            cachedHistory = new History();
        }

        public static void SetPointInTime(int itemIndex)
        {
            history.SetPointInTime(itemIndex);
        }
    }
}
