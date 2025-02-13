using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.SelectionHistory
{
    internal static class FavoritesContextMenu
    {
        [MenuItem("Assets/Add to Favorites", false, 2000)]
        private static void AddToFavoritesAssets()
        {
            foreach (Object obj in Selection.objects)
                FavoritesManager.AddManualFavorite(obj);
        }

        [MenuItem("Assets/Remove from Favorites", false, 2001)]
        private static void RemoveFromFavoritesAssets()
        {
            foreach (Object obj in Selection.objects)
                FavoritesManager.RemoveManualFavorite(obj);
        }

        [MenuItem("Assets/Add to Favorites", true)]
        private static bool ValidateAddToFavoritesAssets()
        {
            foreach (Object obj in Selection.objects)
                if (FavoritesManager.IsManualFavorite(obj))
                    return false;
            return Selection.objects.Length > 0;
        }

        [MenuItem("Assets/Remove from Favorites", true)]
        private static bool ValidateRemoveFromFavoritesAssets()
        {
            foreach (Object obj in Selection.objects)
                if (!FavoritesManager.IsManualFavorite(obj))
                    return false;
            return Selection.objects.Length > 0;
        }

        [MenuItem("CONTEXT/Object/Add to Favorites", false, 2000)]
        private static void AddToFavoritesContext(MenuCommand command)
        {
            FavoritesManager.AddManualFavorite(command.context);
        }

        [MenuItem("CONTEXT/Object/Remove from Favorites", false, 2001)]
        private static void RemoveFromFavoritesContext(MenuCommand command)
        {
            FavoritesManager.RemoveManualFavorite(command.context);
        }

        [MenuItem("CONTEXT/Object/Add to Favorites", true)]
        private static bool ValidateAddToFavoritesContext(MenuCommand command)
        {
            Object obj = command.context;
            return obj != null && !FavoritesManager.IsManualFavorite(obj);
        }

        [MenuItem("CONTEXT/Object/Remove from Favorites", true)]
        private static bool ValidateRemoveFromFavoritesContext(MenuCommand command)
        {
            Object obj = command.context;
            return obj != null && FavoritesManager.IsManualFavorite(obj);
        }

        [MenuItem("GameObject/Add to Favorites", false, 2000)]
        private static void AddToFavoritesGameObject()
        {
            foreach (GameObject obj in Selection.gameObjects)
                FavoritesManager.AddManualFavorite(obj);
        }

        [MenuItem("GameObject/Remove from Favorites", false, 2001)]
        private static void RemoveFromFavoritesGameObject()
        {
            foreach (GameObject obj in Selection.gameObjects)
                FavoritesManager.RemoveManualFavorite(obj);
        }

        [MenuItem("GameObject/Add to Favorites", true)]
        private static bool ValidateAddToFavoritesGameObject()
        {
            foreach (GameObject obj in Selection.gameObjects)
                if (FavoritesManager.IsManualFavorite(obj))
                    return false;
            return Selection.gameObjects.Length > 0;
        }

        [MenuItem("GameObject/Remove from Favorites", true)]
        private static bool ValidateRemoveFromFavoritesGameObject()
        {
            foreach (GameObject obj in Selection.gameObjects)
                if (!FavoritesManager.IsManualFavorite(obj))
                    return false;
            return Selection.gameObjects.Length > 0;
        }
    }
}