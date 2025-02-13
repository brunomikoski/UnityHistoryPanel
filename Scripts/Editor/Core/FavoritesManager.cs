using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory
{
    [Serializable]
    internal class LearnedFavorite
    {
        public string globalId;
        public float totalTime;
        public int selectionCount;
    }

    [Serializable]
    internal class FavoritesManualData
    {
        public string[] favorites;
    }

    [Serializable]
    internal class FavoritesLearnedData
    {
        public LearnedFavorite[] favorites;
    }

    internal static class FavoritesManager
    {
        private static string ManualKey = Application.productName + "_FavoritesManual";
        private static string LearnedKey = Application.productName + "_FavoritesLearned";
        private static List<string> manualFavorites = new();
        private static Dictionary<string, LearnedFavorite> learnedFavorites = new();
        private static double lastSelectionTime;
        private static string lastSelectionGlobalId;

        static FavoritesManager()
        {
            LoadManualFavorites();
            LoadLearnedFavorites();
            lastSelectionTime = EditorApplication.timeSinceStartup;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            double now = EditorApplication.timeSinceStartup;
            double delta = now - lastSelectionTime;
            if (!string.IsNullOrEmpty(lastSelectionGlobalId))
            {
                if (!learnedFavorites.TryGetValue(lastSelectionGlobalId, out LearnedFavorite lf))
                {
                    lf = new LearnedFavorite
                    {
                        globalId = lastSelectionGlobalId,
                        totalTime = 0,
                        selectionCount = 0
                    };
                    learnedFavorites[lastSelectionGlobalId] = lf;
                }
                lf.totalTime += (float)delta;
            }

            Object current = Selection.activeObject;
            if (current != null)
            {
                string currentId = GlobalObjectId.GetGlobalObjectIdSlow(current).ToString();
                lastSelectionGlobalId = currentId;
                if (!learnedFavorites.TryGetValue(currentId, out LearnedFavorite currentLf))
                {
                    currentLf = new LearnedFavorite
                    {
                        globalId = currentId,
                        totalTime = 0,
                        selectionCount = 0
                    };
                    learnedFavorites[currentId] = currentLf;
                }
                currentLf.selectionCount++;
            }

            lastSelectionTime = now;
            SaveLearnedFavorites();
        }

        public static void AddManualFavorite(Object obj)
        {
            if (obj == null) return;
            string id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            if (!manualFavorites.Contains(id))
            {
                manualFavorites.Add(id);
                SaveManualFavorites();
            }
        }

        public static void RemoveManualFavorite(Object obj)
        {
            if (obj == null) return;
            string id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            if (manualFavorites.Remove(id))
                SaveManualFavorites();
        }

        public static bool IsManualFavorite(Object obj)
        {
            if (obj == null)
                return false;
            string id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            return manualFavorites.Contains(id);
        }

        public static List<Object> GetManualFavorites()
        {
            List<Object> list = new();
            if (manualFavorites.Count == 0)
                return list;
            GlobalObjectId[] gids = new GlobalObjectId[manualFavorites.Count];
            for (int i = 0; i < manualFavorites.Count; i++)
            {
                GlobalObjectId.TryParse(manualFavorites[i], out gids[i]);
            }
            Object[] objs = new Object[manualFavorites.Count];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(gids, objs);
            foreach (Object obj in objs)
            {
                if (obj != null)
                    list.Add(obj);
            }
            return list;
        }

        public static List<Object> GetLearnedFavorites()
        {
            List<LearnedFavorite> lfs = new(learnedFavorites.Values);
            lfs.Sort((a, b) => b.selectionCount.CompareTo(a.selectionCount));
            if (lfs.Count > 20)
                lfs = lfs.GetRange(0, 20);
            List<Object> list = new();
            if (lfs.Count == 0)
                return list;
            GlobalObjectId[] gids = new GlobalObjectId[lfs.Count];
            for (int i = 0; i < lfs.Count; i++)
            {
                GlobalObjectId.TryParse(lfs[i].globalId, out gids[i]);
            }
            Object[] objs = new Object[lfs.Count];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(gids, objs);
            foreach (Object obj in objs)
            {
                if (obj != null)
                    list.Add(obj);
            }
            return list;
        }

        private static void SaveManualFavorites()
        {
            FavoritesManualData data = new();
            data.favorites = manualFavorites.ToArray();
            string json = JsonUtility.ToJson(data);
            EditorPrefs.SetString(ManualKey, json);
        }

        private static void LoadManualFavorites()
        {
            if (!EditorPrefs.HasKey(ManualKey))
                return;
            string json = EditorPrefs.GetString(ManualKey);
            FavoritesManualData data = JsonUtility.FromJson<FavoritesManualData>(json);
            if (data?.favorites == null)
                return;
            manualFavorites = new List<string>(data.favorites);
        }

        private static void SaveLearnedFavorites()
        {
            FavoritesLearnedData data = new();
            data.favorites = new LearnedFavorite[learnedFavorites.Values.Count];
            int i = 0;
            foreach (LearnedFavorite lf in learnedFavorites.Values)
                data.favorites[i++] = lf;
            string json = JsonUtility.ToJson(data);
            SessionState.SetString(LearnedKey, json);
        }

        private static void LoadLearnedFavorites()
        {
            string json = SessionState.GetString(LearnedKey, "");
            if (string.IsNullOrEmpty(json))
                return;
            FavoritesLearnedData data = JsonUtility.FromJson<FavoritesLearnedData>(json);
            if (data != null && data.favorites != null)
            {
                learnedFavorites = new Dictionary<string, LearnedFavorite>();
                foreach (LearnedFavorite lf in data.favorites)
                {
                    if (!string.IsNullOrEmpty(lf.globalId))
                        learnedFavorites[lf.globalId] = lf;
                }
            }
        }
    }
}