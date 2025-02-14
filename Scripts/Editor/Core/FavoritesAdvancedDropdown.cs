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
                foreach (var asset in manualFavorites)
                {
                    var item = new FavoritesAdvancedDropdownItem(asset.name, asset);
                    root.AddChild(item);
                }
            }
            if ((manualFavorites != null && manualFavorites.Count > 0) &&
                (learnedFavorites != null && learnedFavorites.Count > 0))
            {
                root.AddSeparator();
            }

            if (learnedFavorites != null && learnedFavorites.Count > 0)
            {
                foreach (var asset in learnedFavorites)
                {
                    var item = new FavoritesAdvancedDropdownItem(asset.name, asset);
                    root.AddChild(item);
                }
            }
            return root;
        }

        public void SetFavorites(List<Object> manual, List<Object> learned)
        {
            manualFavorites = manual;
            learnedFavorites = learned;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is FavoritesAdvancedDropdownItem favItem)
            {
                Selection.activeObject = favItem.Asset;
                EditorGUIUtility.PingObject(favItem.Asset);
            }
        }
    }
}