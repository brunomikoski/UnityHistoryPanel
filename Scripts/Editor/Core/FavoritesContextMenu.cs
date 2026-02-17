using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory
{
    internal static class FavoritesContextMenu
    {
            private const string AssetsAddPath = "Assets/★ Add to Favorites";
            private const string AssetsRemovePath = "Assets/★ Remove From Favorites";
            private const string GameObjectAddPath = "GameObject/★ Add to Favorites";
            private const string GameObjectRemovePath = "GameObject/★ Remove From Favorites";
            private const string ContextAddPath = "CONTEXT/Object/★ Add to Favorites";
            private const string ContextRemovePath = "CONTEXT/Object/★ Remove From Favorites";
            private const int FavoritesMenuPriority = 10000;

            [MenuItem(AssetsAddPath, false, FavoritesMenuPriority)]
            private static void AddFavoriteAssets()
            {
                foreach (Object o in Selection.objects ?? Array.Empty<Object>())
                {
                    if (o != null)
                        FavoritesManager.AddManualFavorite(o);
                }
            }

            [MenuItem(AssetsAddPath, true)]
            private static bool ValidateAddFavoriteAssets()
            {
                Object[] objs = Selection.objects;
                return objs != null && objs.Length > 0 && !FavoritesManager.AreAllManualFavorites(objs);
            }

            [MenuItem(AssetsRemovePath, false, FavoritesMenuPriority)]
            private static void RemoveFavoriteAssets()
            {
                foreach (Object o in Selection.objects ?? Array.Empty<Object>())
                {
                    if (o != null)
                        FavoritesManager.RemoveManualFavorite(o);
                }
            }

            [MenuItem(AssetsRemovePath, true)]
            private static bool ValidateRemoveFavoriteAssets()
            {
                Object[] objs = Selection.objects;
                return objs != null && objs.Length > 0 && FavoritesManager.AreAllManualFavorites(objs);
            }

            [MenuItem(GameObjectAddPath, false, FavoritesMenuPriority)]
            private static void AddFavoriteGameObject()
            {
                foreach (GameObject o in Selection.gameObjects ?? Array.Empty<GameObject>())
                {
                    if (o != null)
                        FavoritesManager.AddManualFavorite(o);
                }
            }

            [MenuItem(GameObjectAddPath, true)]
            private static bool ValidateAddFavoriteGameObject()
            {
                GameObject[] objs = Selection.gameObjects;
                return objs != null && objs.Length > 0 && !FavoritesManager.AreAllManualFavorites(objs);
            }

            [MenuItem(GameObjectRemovePath, false, FavoritesMenuPriority)]
            private static void RemoveFavoriteGameObject()
            {
                foreach (GameObject o in Selection.gameObjects ?? Array.Empty<GameObject>())
                {
                    if (o != null)
                        FavoritesManager.RemoveManualFavorite(o);
                }
            }

            [MenuItem(GameObjectRemovePath, true)]
            private static bool ValidateRemoveFavoriteGameObject()
            {
                GameObject[] objs = Selection.gameObjects;
                return objs != null && objs.Length > 0 && FavoritesManager.AreAllManualFavorites(objs);
            }

            [MenuItem(ContextAddPath, false, FavoritesMenuPriority)]
            private static void AddFavoriteContext(MenuCommand command)
            {
                if (command.context != null)
                    FavoritesManager.AddManualFavorite(command.context);
            }

            [MenuItem(ContextAddPath, true)]
            private static bool ValidateAddFavoriteContext(MenuCommand command)
            {
                return command.context != null && !FavoritesManager.IsManualFavorite(command.context);
            }

            [MenuItem(ContextRemovePath, false, FavoritesMenuPriority)]
            private static void RemoveFavoriteContext(MenuCommand command)
            {
                if (command.context != null)
                    FavoritesManager.RemoveManualFavorite(command.context);
            }

            [MenuItem(ContextRemovePath, true)]
            private static bool ValidateRemoveFavoriteContext(MenuCommand command)
            {
                return command.context != null && FavoritesManager.IsManualFavorite(command.context);
            }

            [MenuItem("Tools/History/Open Favorites %#f", false, 100)]
            private static void OpenFavoritesPanel()
            {
                FavoritesPanelWindow.ShowAtPosition(null);
            }
        }
}
