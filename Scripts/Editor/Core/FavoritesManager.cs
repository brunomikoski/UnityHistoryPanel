using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory {
    [Serializable]
    internal class LearnedFavorite {
        public string globalId;
        public int selectionCount;
        public long lastSelectedTicks;
    }

    [Serializable]
    internal class FavoritesManualData {
        public string[] favorites;
    }

    [Serializable]
    internal class FavoritesLearnedData {
        public LearnedFavorite[] favorites;
    }

    internal static class FavoritesManager {
        private static string ManualKey = Application.productName + "_FavoritesManual";
        private static string LearnedKey = Application.productName + "_FavoritesLearned";
        private static List<string> manualFavorites = new();
        private static Dictionary<string, LearnedFavorite> learnedFavorites = new();
        private static bool manualFavoritesLoaded = false;
        private static bool learnedFavoritesLoaded = false;
        private static string lastSelectionGlobalId;

        static FavoritesManager() {
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void EnsureManualFavoritesLoaded() {
            if (!manualFavoritesLoaded) {
                LoadManualFavorites();
                manualFavoritesLoaded = true;
            }
        }

        private static void EnsureLearnedFavoritesLoaded() {
            if (!learnedFavoritesLoaded) {
                LoadLearnedFavorites();
                learnedFavoritesLoaded = true;
            }
        }

        private static void OnSelectionChanged() {
            EnsureLearnedFavoritesLoaded();

            Object current = Selection.activeObject;
            if (current == null)
                return;

            String currentId = GlobalObjectId.GetGlobalObjectIdSlow(current).ToString();
            LearnedFavorite lf;
            if (!learnedFavorites.TryGetValue(currentId, out lf)) {
                lf = new LearnedFavorite {
                    globalId = currentId,
                    selectionCount = 0,
                    lastSelectedTicks = 0
                };
                learnedFavorites[currentId] = lf;
            }

            lf.selectionCount++;
            lf.lastSelectedTicks = DateTime.UtcNow.Ticks;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state) {
            if (state is PlayModeStateChange.ExitingPlayMode or PlayModeStateChange.ExitingEditMode)
                SaveLearnedFavorites();
        }

        public static void AddManualFavorite(Object obj) {
            if (obj == null)
                return;
            EnsureManualFavoritesLoaded();
            string id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            if (!manualFavorites.Contains(id)) {
                manualFavorites.Add(id);
                SaveManualFavorites();
            }
        }

        public static void RemoveManualFavorite(Object obj) {
            if (obj == null)
                return;
            EnsureManualFavoritesLoaded();
            string id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            if (manualFavorites.Remove(id))
                SaveManualFavorites();
        }

        public static bool IsManualFavorite(Object obj) {
            if (obj == null)
                return false;
            EnsureManualFavoritesLoaded();
            string id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            return manualFavorites.Contains(id);
        }

        public static List<Object> GetManualFavorites() {
            EnsureManualFavoritesLoaded();
            List<Object> list = new();
            if (manualFavorites.Count == 0)
                return list;
            GlobalObjectId[] gids = new GlobalObjectId[manualFavorites.Count];
            for (int i = 0; i < manualFavorites.Count; i++)
                GlobalObjectId.TryParse(manualFavorites[i], out gids[i]);
            Object[] objs = new Object[manualFavorites.Count];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(gids, objs);
            foreach (Object obj in objs)
                if (obj != null)
                    list.Add(obj);
            return list;
        }

        public static List<Object> GetLearnedFavorites() {
            EnsureLearnedFavoritesLoaded();

            List<LearnedFavorite> list = new List<LearnedFavorite>(learnedFavorites.Values);
            list.Sort((a, b) => {
                int recency = b.lastSelectedTicks.CompareTo(a.lastSelectedTicks);
                return recency != 0 ? recency : b.selectionCount.CompareTo(a.selectionCount);
            });

            if (list.Count > 20)
                list = list.GetRange(0, 20);

            List<Object> result = new List<Object>();
            GlobalObjectId[] gids = new GlobalObjectId[list.Count];
            for (Int32 i = 0; i < list.Count; i++)
                GlobalObjectId.TryParse(list[i].globalId, out gids[i]);

            Object[] objs = new Object[list.Count];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(gids, objs);

            foreach (Object o in objs)
                if (o != null)
                    result.Add(o);

            return result;
        }

        private static void SaveManualFavorites() {
            FavoritesManualData data = new();
            data.favorites = manualFavorites.ToArray();
            string json = JsonUtility.ToJson(data);
            EditorPrefs.SetString(ManualKey, json);
        }

        private static void LoadManualFavorites() {
            if (!EditorPrefs.HasKey(ManualKey))
                return;
            string json = EditorPrefs.GetString(ManualKey);
            FavoritesManualData data = JsonUtility.FromJson<FavoritesManualData>(json);
            if (data?.favorites == null)
                return;
            manualFavorites = new List<string>(data.favorites);
        }

        private static void SaveLearnedFavorites() {
            FavoritesLearnedData data = new();
            data.favorites = new LearnedFavorite[learnedFavorites.Values.Count];
            int i = 0;
            foreach (LearnedFavorite lf in learnedFavorites.Values)
                data.favorites[i++] = lf;
            string json = JsonUtility.ToJson(data);
            EditorPrefs.SetString(LearnedKey, json);
        }

        private static void LoadLearnedFavorites() {
            string json = EditorPrefs.GetString(LearnedKey, "");
            if (string.IsNullOrEmpty(json))
                return;
            FavoritesLearnedData data = JsonUtility.FromJson<FavoritesLearnedData>(json);
            if (data != null && data.favorites != null) {
                learnedFavorites = new Dictionary<string, LearnedFavorite>();
                foreach (LearnedFavorite lf in data.favorites)
                    if (!string.IsNullOrEmpty(lf.globalId))
                        learnedFavorites[lf.globalId] = lf;
            }
        }
    }
}
