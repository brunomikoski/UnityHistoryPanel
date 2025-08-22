using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory
{
    [Serializable]
    internal class LearnedFavorite
    {
        public string globalId;
        public int selectionCount;
        public long lastSelectedTicks;
    }

    [Serializable]
    internal class FavoriteEntry
    {
        public string globalId;
        public string containerPath;
        public string objectPath;
        public string componentType;
        public string label;
    }

    [Serializable]
    internal class FavoritesManualData
    {
        public FavoriteEntry[] entries;
    }

    [Serializable]
    internal class FavoritesLearnedData
    {
        public LearnedFavorite[] favorites;
    }

    // ======================= Manager =======================
    internal static class FavoritesManager
    {
        private static string ManualKey = "FavoritesManual";
        private static string LearnedKey = "FavoritesLearned";

        private static List<FavoriteEntry> manualEntries = new List<FavoriteEntry>();
        private static Dictionary<string, LearnedFavorite> learnedFavorites = new Dictionary<string, LearnedFavorite>();

        private static bool manualFavoritesLoaded = false;
        private static bool learnedFavoritesLoaded = false;

        private static bool learnedDirty = false;
        private static double lastLearnedSaveTime = 0.0;
        private const double AUTO_SAVE_INTERVAL_SECONDS = 10.0;

        static FavoritesManager()
        {
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorQuitting;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void EnsureManualFavoritesLoaded()
        {
            if (!manualFavoritesLoaded)
            {
                LoadManualFavorites();
                manualFavoritesLoaded = true;
            }
        }

        private static void EnsureLearnedFavoritesLoaded()
        {
            if (!learnedFavoritesLoaded)
            {
                LoadLearnedFavorites();
                learnedFavoritesLoaded = true;
            }
        }

        private static void OnSelectionChanged()
        {
            EnsureLearnedFavoritesLoaded();

            Object current = Selection.activeObject;
            if (current == null)
                return;

            string currentId = GlobalObjectId.GetGlobalObjectIdSlow(current).ToString();
            LearnedFavorite lf;
            if (!learnedFavorites.TryGetValue(currentId, out lf))
            {
                lf = new LearnedFavorite
                {
                    globalId = currentId,
                    selectionCount = 0,
                    lastSelectedTicks = 0
                };
                learnedFavorites[currentId] = lf;
            }

            lf.selectionCount++;
            lf.lastSelectedTicks = DateTime.UtcNow.Ticks;

            learnedDirty = true;
        }

        private static void OnEditorUpdate()
        {
            if (!learnedDirty)
                return;

            double now = EditorApplication.timeSinceStartup;
            if (now - lastLearnedSaveTime >= AUTO_SAVE_INTERVAL_SECONDS)
                SaveLearnedFavorites();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.ExitingEditMode)
                SaveLearnedFavorites();
        }

        private static void OnEditorQuitting()
        {
            SaveManualFavorites();
            SaveLearnedFavorites();
        }

        public static void ToggleManualFavorite(Object obj)
        {
            if (obj == null)
                return;
            if (IsManualFavorite(obj))
                RemoveManualFavorite(obj);
            else
                AddManualFavorite(obj);
        }

        public static void AddManualFavorite(Object obj)
        {
            if (obj == null)
                return;

            EnsureManualFavoritesLoaded();

            string id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            if (IndexOfManual(id) >= 0)
                return;

            FavoriteEntry entry = new FavoriteEntry();
            entry.globalId = id;
            entry.containerPath = ComputeContainerPath(id);
            entry.objectPath = ComputeObjectPath(obj);
            entry.componentType = GetComponentTypeName(obj);
            entry.label = BuildLabel(entry, obj);

            manualEntries.Add(entry);
            SaveManualFavorites();
        }

        public static void RemoveManualFavorite(Object obj)
        {
            if (obj == null)
                return;

            EnsureManualFavoritesLoaded();

            string id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            int idx = IndexOfManual(id);
            if (idx >= 0)
            {
                manualEntries.RemoveAt(idx);
                SaveManualFavorites();
            }
        }

        public static bool IsManualFavorite(Object obj)
        {
            if (obj == null)
                return false;

            EnsureManualFavoritesLoaded();
            string id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            return IndexOfManual(id) >= 0;
        }

        public static bool AreAllManualFavorites(Object[] objs)
        {
            EnsureManualFavoritesLoaded();
            if (objs == null || objs.Length == 0)
                return false;
            for (int i = 0; i < objs.Length; i++)
            {
                Object o = objs[i];
                if (o == null)
                    return false;
                if (!IsManualFavorite(o))
                    return false;
            }

            return true;
        }

        public static List<FavoriteEntry> GetManualFavoriteEntries()
        {
            EnsureManualFavoritesLoaded();
            return new List<FavoriteEntry>(manualEntries);
        }

        public static List<Object> GetLearnedFavorites()
        {
            EnsureLearnedFavoritesLoaded();

            List<LearnedFavorite> list = new List<LearnedFavorite>(learnedFavorites.Values);
            list.Sort((a, b) =>
            {
                int byCount = b.selectionCount.CompareTo(a.selectionCount);
                if (byCount != 0)
                    return byCount;
                return b.lastSelectedTicks.CompareTo(a.lastSelectedTicks);
            });

            if (list.Count > 20)
                list = list.GetRange(0, 20);

            List<Object> result = new List<Object>();
            GlobalObjectId[] gids = new GlobalObjectId[list.Count];
            for (int i = 0; i < list.Count; i++)
                GlobalObjectId.TryParse(list[i].globalId, out gids[i]);

            Object[] objs = new Object[list.Count];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(gids, objs);

            for (int i = 0; i < objs.Length; i++)
            {
                Object o = objs[i];
                if (o != null)
                    result.Add(o);
            }

            return result;
        }

        public static List<string> GetLearnedFavoriteIds()
        {
            EnsureLearnedFavoritesLoaded();

            List<LearnedFavorite> list = new List<LearnedFavorite>(learnedFavorites.Values);
            list.Sort((a, b) =>
            {
                int byCount = b.selectionCount.CompareTo(a.selectionCount);
                if (byCount != 0)
                    return byCount;
                return b.lastSelectedTicks.CompareTo(a.lastSelectedTicks);
            });

            List<string> ids = new List<string>(list.Count);
            for (int i = 0; i < list.Count; i++)
                ids.Add(list[i].globalId);
            return ids;
        }

        // ---------- Helpers ----------
        private static int IndexOfManual(string globalId)
        {
            for (int i = 0; i < manualEntries.Count; i++)
                if (manualEntries[i].globalId == globalId)
                    return i;
            return -1;
        }

        private static string ComputeContainerPath(string globalId)
        {
            GlobalObjectId gid;
            if (!GlobalObjectId.TryParse(globalId, out gid))
                return string.Empty;
            return AssetDatabase.GUIDToAssetPath(gid.assetGUID.ToString());
        }

        private static string ComputeObjectPath(Object obj)
        {
            Component comp = obj as Component;
            GameObject go = comp != null ? comp.gameObject : obj as GameObject;
            if (go == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            Transform t = go.transform;
            List<string> parts = new List<string>();
            while (t != null)
            {
                parts.Add(t.name);
                t = t.parent;
            }

            for (int i = parts.Count - 1; i >= 0; i--)
            {
                sb.Append(parts[i]);
                if (i > 0)
                    sb.Append('/');
            }

            return sb.ToString();
        }

        private static string GetComponentTypeName(Object obj)
        {
            Component c = obj as Component;
            return c != null ? c.GetType().Name : string.Empty;
        }

        private static string BuildLabel(FavoriteEntry entry, Object liveObj)
        {
            string container = System.IO.Path.GetFileNameWithoutExtension(entry.containerPath);
            if (liveObj is Component)
            {
                return container + " -> " + entry.objectPath + " [" + entry.componentType + "]";
            }

            if (liveObj is GameObject)
            {
                return container + " -> " + entry.objectPath;
            }

            if (liveObj != null)
            {
                return liveObj.name;
            }

            if (!string.IsNullOrEmpty(entry.objectPath))
                return container + " -> " + entry.objectPath + (string.IsNullOrEmpty(entry.componentType) ? string.Empty : " [" + entry.componentType + "]");
            if (!string.IsNullOrEmpty(container))
                return container;
            return "<Missing>";
        }

        private static void SaveManualFavorites()
        {
            FavoritesManualData data = new FavoritesManualData();
            data.entries = manualEntries.ToArray();
            string json = JsonUtility.ToJson(data);
            EditorUserSettings.SetConfigValue(ManualKey, json);
        }

        private static void LoadManualFavorites()
        {
            string json = EditorUserSettings.GetConfigValue(ManualKey);
            if (string.IsNullOrEmpty(json))
                return;

            FavoritesManualData data = JsonUtility.FromJson<FavoritesManualData>(json);
            if (data == null)
                return;

            if (data.entries != null && data.entries.Length > 0)
            {
                manualEntries = new List<FavoriteEntry>(data.entries);
            }
        }

        private static void SaveLearnedFavorites()
        {
            EnsureLearnedFavoritesLoaded();
            FavoritesLearnedData data = new FavoritesLearnedData();
            data.favorites = new LearnedFavorite[learnedFavorites.Values.Count];
            int i = 0;
            foreach (KeyValuePair<string, LearnedFavorite> kvp in learnedFavorites)
                data.favorites[i++] = kvp.Value;
            string json = JsonUtility.ToJson(data);
            EditorUserSettings.SetConfigValue(LearnedKey, json);
            learnedDirty = false;
            lastLearnedSaveTime = EditorApplication.timeSinceStartup;
        }

        private static void LoadLearnedFavorites()
        {
            string json = EditorUserSettings.GetConfigValue(LearnedKey);
            if (string.IsNullOrEmpty(json))
                return;
            FavoritesLearnedData data = JsonUtility.FromJson<FavoritesLearnedData>(json);
            if (data == null || data.favorites == null)
                return;
            learnedFavorites = new Dictionary<string, LearnedFavorite>();
            for (int i = 0; i < data.favorites.Length; i++)
            {
                LearnedFavorite lf = data.favorites[i];
                if (lf != null && !string.IsNullOrEmpty(lf.globalId))
                    learnedFavorites[lf.globalId] = lf;
            }
        }
    }
}
