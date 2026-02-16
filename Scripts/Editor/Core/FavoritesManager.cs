using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory
{
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

    internal static class FavoritesManager
    {
        private static string ManualKey = "FavoritesManual";

        private static List<FavoriteEntry> manualEntries = new List<FavoriteEntry>();

        private static bool manualFavoritesLoaded = false;

        static FavoritesManager()
        {
            EditorApplication.quitting += OnEditorQuitting;
        }

        private static void EnsureManualFavoritesLoaded()
        {
            if (!manualFavoritesLoaded)
            {
                LoadManualFavorites();
                manualFavoritesLoaded = true;
            }
        }

        private static void OnEditorQuitting()
        {
            SaveManualFavorites();
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

        public static bool RemoveManualFavoriteByGlobalId(string globalId)
        {
            if (string.IsNullOrEmpty(globalId))
                return false;

            EnsureManualFavoritesLoaded();

            int idx = IndexOfManual(globalId);
            if (idx >= 0)
            {
                manualEntries.RemoveAt(idx);
                SaveManualFavorites();
                return true;
            }

            return false;
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

    }
}
