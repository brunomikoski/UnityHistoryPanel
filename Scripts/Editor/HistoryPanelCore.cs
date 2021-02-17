using System;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.EditorHistoryNavigation
{
    [InitializeOnLoad]
    internal static class HistoryPanelCore
    {
        private static string HISTORY_STORAGE_KEY = Application.productName + "EditorHistoryKey";
        private static string MAX_HISTORY_ITEMS_KEY = "MaxHistoryItemsKey";

        public static event Action OnHistoryChangedEvent;

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


        private static int? cachedMaximumHistoryItems;
        internal static int MaximumHistoryItems
        {
            get
            {
                if (cachedMaximumHistoryItems.HasValue)
                    return cachedMaximumHistoryItems.Value;
                cachedMaximumHistoryItems = EditorPrefs.GetInt(MAX_HISTORY_ITEMS_KEY, 30);
                return cachedMaximumHistoryItems.Value;
            }
        }

        static HistoryPanelCore()
        {
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += SaveHistory;
            Selection.selectionChanged += OnSelectionChanged;
        }

        internal static void SetMaximumHistory(int targetMaximum)
        {
            EditorPrefs.SetInt(MAX_HISTORY_ITEMS_KEY, targetMaximum);
            cachedMaximumHistoryItems = targetMaximum;
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

        private static void OnSelectionChanged()
        {
            history.AddToHistory(Selection.objects);
            OnHistoryChangedEvent?.Invoke();
        }

        [MenuItem("Tools/History/Go Back")]
        public static void GoBack()
        {
            history.Back();
        }

        [MenuItem("Tools/History/Go Forward")]
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
