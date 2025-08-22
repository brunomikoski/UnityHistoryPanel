using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory {
    internal class FavoritesAdvancedDropdownItem : AdvancedDropdownItem {
        public readonly string GlobalId;
        public readonly string ContainerPath;
        public readonly string PreLabel;
        public readonly Object IconSource;

        public FavoritesAdvancedDropdownItem(string label, string globalId, string containerPath, string preLabel, Object iconSource) : base(label) {
            GlobalId = globalId;
            ContainerPath = containerPath;
            PreLabel = preLabel;
            IconSource = iconSource;
            if (iconSource != null)
                icon = (Texture2D)EditorGUIUtility.ObjectContent(iconSource, iconSource.GetType()).image;
        }
    }

    internal class FavoritesAdvancedDropdown : AdvancedDropdown {
        private List<FavoriteEntry> manualEntries;
        private List<Object> learnedFavorites;
        private List<string> learnedIds;

        public FavoritesAdvancedDropdown(AdvancedDropdownState state) : base(state) {
            minimumSize = new Vector2(360, 420);
        }

        protected override AdvancedDropdownItem BuildRoot() {
            AdvancedDropdownItem root = new AdvancedDropdownItem("Favorites");

            if (manualEntries != null && manualEntries.Count > 0) {
                for (int i = 0; i < manualEntries.Count; i++) {
                    FavoriteEntry e = manualEntries[i];
                    Object resolved = TryResolve(e.globalId);
                    string label = !string.IsNullOrEmpty(e.label) ? e.label : BuildDisplayLabel(e, resolved);
                    root.AddChild(new FavoritesAdvancedDropdownItem(label, e.globalId, e.containerPath, e.label, resolved));
                }
            }

            if (learnedFavorites != null && learnedFavorites.Count > 0) {
                if (manualEntries != null && manualEntries.Count > 0)
                    root.AddSeparator();

                for (int i = 0; i < learnedFavorites.Count; i++) {
                    Object asset = learnedFavorites[i];
                    string gid = learnedIds != null && i < learnedIds.Count ? learnedIds[i] : (asset != null ? GlobalObjectId.GetGlobalObjectIdSlow(asset).ToString() : string.Empty);
                    string label = BuildDisplayLabel(new FavoriteEntry { globalId = gid, containerPath = ComputeContainerPathFromGlobal(gid), objectPath = string.Empty, componentType = string.Empty }, asset);
                    root.AddChild(new FavoritesAdvancedDropdownItem(label, gid, ComputeContainerPathFromGlobal(gid), label, asset));
                }
            }

            return root;
        }

        public void SetFavorites(List<FavoriteEntry> manual, List<Object> learned) {
            manualEntries = manual ?? new List<FavoriteEntry>();

            learnedFavorites = new List<Object>();
            learnedIds = FavoritesManager.GetLearnedFavoriteIds();

            HashSet<int> manualIDs = new HashSet<int>();
            for (int i = 0; i < manualEntries.Count; i++) {
                Object resolved = TryResolve(manualEntries[i].globalId);
                if (resolved != null)
                    manualIDs.Add(resolved.GetInstanceID());
            }

            for (int i = 0; i < learned.Count; i++) {
                Object l = learned[i];
                if (l != null && !manualIDs.Contains(l.GetInstanceID()))
                    learnedFavorites.Add(l);
            }
        }

        protected override void ItemSelected(AdvancedDropdownItem item) {
            FavoritesAdvancedDropdownItem fav = item as FavoritesAdvancedDropdownItem;
            if (fav == null)
                return;

            bool shift = Event.current != null && Event.current.shift;

            Object target = TryResolve(fav.GlobalId);
            if (target == null) {
                EnsureContainerLoaded(fav.ContainerPath, shift);
                target = TryResolve(fav.GlobalId);
            }

            if (target == null)
                return;

            if (shift)
                OpenContainerForObject(target);

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        private static Object TryResolve(string globalId) {
            if (string.IsNullOrEmpty(globalId))
                return null;
            GlobalObjectId gid;
            if (!GlobalObjectId.TryParse(globalId, out gid))
                return null;
            Object[] objs = new Object[1];
            GlobalObjectId[] gids = new GlobalObjectId[1];
            gids[0] = gid;
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(gids, objs);
            return objs[0];
        }

        private static void EnsureContainerLoaded(string path, bool openSingle) {
            if (string.IsNullOrEmpty(path))
                return;

            if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) {
                if (openSingle)
                    UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                UnityEditor.SceneManagement.OpenSceneMode mode = openSingle ? UnityEditor.SceneManagement.OpenSceneMode.Single : UnityEditor.SceneManagement.OpenSceneMode.Additive;
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, mode);
                return;
            }

            if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) {
                Object main = AssetDatabase.LoadMainAssetAtPath(path);
                if (main != null)
                    AssetDatabase.OpenAsset(main);
                return;
            }

            AssetDatabase.LoadMainAssetAtPath(path);
        }

        private static void OpenContainerForObject(Object obj) {
            if (obj == null)
                return;

            SceneAsset scene = obj as SceneAsset;
            if (scene != null) {
                string path = AssetDatabase.GetAssetPath(scene);
                if (!string.IsNullOrEmpty(path)) {
                    UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                }
                return;
            }

            if (PrefabUtility.IsPartOfPrefabAsset(obj)) {
                AssetDatabase.OpenAsset(obj);
            }
        }

        private static string BuildDisplayLabel(FavoriteEntry entry, Object resolved)
        {
            string containerName = string.IsNullOrEmpty(entry.containerPath)
                ? string.Empty
                : System.IO.Path.GetFileNameWithoutExtension(entry.containerPath);

            if (resolved != null)
            {
                if (resolved is GameObject go)
                {
                    string path = GetTransformPath(go);
                    if (!string.IsNullOrEmpty(entry.componentType))
                        return $"{containerName} -> {path} [{entry.componentType}]";
                    return $"{containerName} -> {path}";
                }

                return resolved.name;
            }

            if (!string.IsNullOrEmpty(entry.objectPath))
            {
                if (!string.IsNullOrEmpty(entry.componentType))
                    return $"{containerName} -> {entry.objectPath} [{entry.componentType}]";
                return $"{containerName} -> {entry.objectPath}";
            }

            return containerName.Length > 0 ? containerName : "<Missing>";
        }

        private static string GetTransformPath(GameObject go)
        {
            if (go == null) 
                return string.Empty;
            string path = go.name;
            Transform current = go.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
        
        private static string ComputeContainerPathFromGlobal(string globalId)
        {
            if (!GlobalObjectId.TryParse(globalId, out GlobalObjectId gid))
                return string.Empty;

            return AssetDatabase.GUIDToAssetPath(gid.assetGUID.ToString());
        }
    }
}
