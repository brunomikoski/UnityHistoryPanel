using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.SelectionHistory
{
    internal static class FavoritesContextMenu
    {
        private const string AssetsMenuPath = "Assets/Favorite";
            private const string GameObjectMenuPath = "GameObject/Favorite";
            private const string ContextMenuPath = "CONTEXT/Object/Favorite";

            [MenuItem(AssetsMenuPath, false, 2000)]
            private static void ToggleFavoriteAssets()
            {
                Object[] objs = Selection.objects;
                if (objs == null || objs.Length == 0)
                    return;
                bool allFav = FavoritesManager.AreAllManualFavorites(objs);
                for (int i = 0; i < objs.Length; i++)
                {
                    Object o = objs[i];
                    if (o == null)
                        continue;
                    if (allFav)
                        FavoritesManager.RemoveManualFavorite(o);
                    else
                        FavoritesManager.AddManualFavorite(o);
                }
            }

            [MenuItem(AssetsMenuPath, true)]
            private static bool ValidateToggleFavoriteAssets()
            {
                Object[] objs = Selection.objects;
                bool enabled = objs != null && objs.Length > 0;
                if (enabled)
                {
                    bool allFav = FavoritesManager.AreAllManualFavorites(objs);
                    Menu.SetChecked(AssetsMenuPath, allFav);
                }

                return enabled;
            }

            [MenuItem(GameObjectMenuPath, false, 2000)]
            private static void ToggleFavoriteGameObject()
            {
                GameObject[] objs = Selection.gameObjects;
                if (objs == null || objs.Length == 0)
                    return;
                bool allFav = FavoritesManager.AreAllManualFavorites(objs);
                for (int i = 0; i < objs.Length; i++)
                {
                    GameObject o = objs[i];
                    if (o == null)
                        continue;
                    if (allFav)
                        FavoritesManager.RemoveManualFavorite(o);
                    else
                        FavoritesManager.AddManualFavorite(o);
                }
            }

            [MenuItem(GameObjectMenuPath, true)]
            private static bool ValidateToggleFavoriteGameObject()
            {
                GameObject[] objs = Selection.gameObjects;
                bool enabled = objs != null && objs.Length > 0;
                if (enabled)
                {
                    bool allFav = FavoritesManager.AreAllManualFavorites(objs);
                    Menu.SetChecked(GameObjectMenuPath, allFav);
                }

                return enabled;
            }

            [MenuItem(ContextMenuPath, false, 2000)]
            private static void ToggleFavoriteContext(MenuCommand command)
            {
                Object obj = command.context;
                if (obj == null)
                    return;
                FavoritesManager.ToggleManualFavorite(obj);
            }

            [MenuItem(ContextMenuPath, true)]
            private static bool ValidateToggleFavoriteContext(MenuCommand command)
            {
                Object obj = command.context;
                if (obj == null)
                    return false;
                bool isFav = FavoritesManager.IsManualFavorite(obj);
                Menu.SetChecked(ContextMenuPath, isFav);
                return true;
            }

            [MenuItem("Tools/History/Open Favorites %#f", false, 100)]
            private static void OpenFavoritesPanel()
            {
                FavoritesPanelWindow.ShowAtPosition(null);
            }
        }
}
