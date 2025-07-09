using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BrunoMikoski.SelectionHistory
{
    internal class FavoritesAdvancedDropdownItem : AdvancedDropdownItem
    {
        public readonly Object Asset;

        public FavoritesAdvancedDropdownItem(string label, Object asset) : base(label)
        {
            Asset = asset;
            icon = (Texture2D)EditorGUIUtility.ObjectContent(asset, asset.GetType()).image;
        }
    }

    internal class FavoritesAdvancedDropdown : AdvancedDropdown
    {
        private List<Object> manualFavorites;
        private List<Object> learnedFavorites;

        public FavoritesAdvancedDropdown(AdvancedDropdownState state) : base(state)
        {
            minimumSize = new Vector2(300, 400);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem("Favorites");

            if (manualFavorites != null && manualFavorites.Count > 0)
            {
                foreach (Object asset in manualFavorites)
                    root.AddChild(new FavoritesAdvancedDropdownItem(asset.name, asset));
            }

            if (learnedFavorites != null && learnedFavorites.Count > 0)
            {
                if (manualFavorites != null && manualFavorites.Count > 0)
                    root.AddSeparator();

                foreach (Object asset in learnedFavorites)
                    root.AddChild(new FavoritesAdvancedDropdownItem(asset.name, asset));
            }

            return root;
        }

        public void SetFavorites(List<Object> manual, List<Object> learned)
        {
            manualFavorites = manual ?? new List<Object>();
            learnedFavorites = new List<Object>();

            HashSet<int> manualIDs = new HashSet<int>();
            foreach (Object m in manualFavorites)
                manualIDs.Add(m.GetInstanceID());

            foreach (Object l in learned)
                if (l != null && !manualIDs.Contains(l.GetInstanceID()))
                    learnedFavorites.Add(l);
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (!(item is FavoritesAdvancedDropdownItem fav))
                return;

            Object asset = fav.Asset;

            if (asset == null)
                return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            if (asset is SceneAsset)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                if (!string.IsNullOrEmpty(path))
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
            }
            else if (PrefabUtility.IsPartOfPrefabAsset(asset))
            {
                AssetDatabase.OpenAsset(asset);
            }
        }
    }
}
