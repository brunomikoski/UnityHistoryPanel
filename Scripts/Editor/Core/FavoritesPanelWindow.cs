using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory
{
    internal class FavoritesPanelWindow : EditorWindow
    {
        private const string ALL_FILTER = "All";
        private const float MIN_WIDTH = 360f;
        private const float MIN_HEIGHT = 420f;
        private TwoPaneSplitView _splitView;
        private ScrollView _filterPane;
        private ListView _itemsList;
        private VisualElement _listContainer;
        private VisualElement _emptyState;
        private string _activeFilter = ALL_FILTER;
        private List<FavoriteDisplayItem> _allItems = new List<FavoriteDisplayItem>();
        private List<FavoriteDisplayItem> _filteredItems = new List<FavoriteDisplayItem>();
        private HashSet<string> _filterTypes = new HashSet<string>();

        private class FavoriteDisplayItem
        {
            public string Label;
            public string GlobalId;
            public string ContainerPath;
            public string PreLabel;
            public Object IconSource;
            public string TypeFilter;
            public string Source; // "Manual"
        }

        public static void ShowAtPosition(Rect? screenRect = null)
        {
            var window = GetWindow<FavoritesPanelWindow>(true, "Favorites", true);
            window.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            window.maxSize = new Vector2(800, 600);

            if (screenRect.HasValue)
            {
                var rect = screenRect.Value;
                rect.y += rect.height;
                rect.height = MIN_HEIGHT;
                rect.width = Mathf.Max(MIN_WIDTH, rect.width);
                window.position = rect;
            }

            EditorApplication.delayCall += () => window.RefreshContent();
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void CreateGUI()
        {
            var styleSheet = LoadStyleSheet();
            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);

            var root = new VisualElement { name = "favorites-root" };
            root.AddToClassList("favorites-root");
            root.style.flexGrow = 1;
            root.style.minHeight = MIN_HEIGHT;

            var header = new VisualElement { name = "favorites-header" };
            header.Add(new Label("Favorites") { style = { flexGrow = 1 } });
            root.Add(header);

            var content = new VisualElement { name = "favorites-content" };
            content.style.flexGrow = 1;
            content.style.minHeight = 0;

            _splitView = new TwoPaneSplitView(0, 160, TwoPaneSplitViewOrientation.Horizontal);
            _splitView.style.flexGrow = 1;
            _splitView.style.minHeight = 0;

            _filterPane = new ScrollView(ScrollViewMode.Vertical) { name = "favorites-filter-pane" };
            _filterPane.style.flexGrow = 0;
            _filterPane.style.flexShrink = 0;
            _filterPane.style.minWidth = 140;

            _listContainer = new VisualElement { name = "favorites-list-pane" };
            _listContainer.style.flexGrow = 1;
            _listContainer.style.minWidth = 0;

            _itemsList = new ListView
            {
                name = "favorites-list",
                fixedItemHeight = 26,
                makeItem = () => CreateListItem(),
                bindItem = (e, i) => BindListItem(e, i),
                selectionType = SelectionType.Single
            };
            _itemsList.itemsChosen += OnItemChosen;
            _itemsList.selectionChanged += OnSelectionChanged;

            _emptyState = new VisualElement { name = "favorites-empty-state" };
            _emptyState.Add(new Label("No favorites yet.\nAdd manual favorites via right-click > Favorite."));
            _emptyState.style.display = DisplayStyle.None;

            _listContainer.Add(_itemsList);
            _listContainer.Add(_emptyState);
            _emptyState.pickingMode = PickingMode.Ignore;

            _splitView.Add(_filterPane);
            _splitView.Add(_listContainer);
            content.Add(_splitView);
            root.Add(content);

            var tipsFooter = new VisualElement { name = "favorites-tips" };
            tipsFooter.Add(new Label(
                "• Shift+Click — Open scene/prefab\n" +
                "• Ctrl+Click — Open scene additively / Add prefab to scene\n" +
                "• Single-click — Select\n" +
                "• Click outside — Close\n" +
                "• Ctrl+Shift+F — Open panel\n" +
                "• Right-click item — Remove from favorites"));
            root.Add(tipsFooter);

            rootVisualElement.Add(root);
        }

        private static StyleSheet LoadStyleSheet()
        {
            var guids = AssetDatabase.FindAssets("FavoritesPanel t:StyleSheet");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("FavoritesPanel.uss"))
                    return AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            }
            return null;
        }

        private VisualElement CreateListItem()
        {
            var row = new VisualElement();
            row.AddToClassList("favorites-list-item");
            row.AddToClassList("unity-list-view__item");

            var icon = new Image();
            icon.AddToClassList("favorites-item-icon");

            var label = new Label();
            label.AddToClassList("favorites-item-label");

            row.Add(icon);
            row.Add(label);
            row.userData = new object[] { icon, label, -1 };

            row.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                var parts = row.userData as object[];
                if (parts == null || parts.Length < 3) return;
                int idx = parts[2] is int i ? i : -1;
                if (idx < 0 || idx >= _filteredItems.Count) return;

                string globalId = _filteredItems[idx].GlobalId;
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("★ Remove From Favorites", _ =>
                {
                    FavoritesManager.RemoveManualFavoriteByGlobalId(globalId);
                    RefreshContent();
                }, DropdownMenuAction.AlwaysEnabled);
            }));

            return row;
        }

        private void BindListItem(VisualElement element, int index)
        {
            if (index < 0 || index >= _filteredItems.Count)
                return;

            var item = _filteredItems[index];
            var parts = element.userData as object[];
            if (parts == null || parts.Length < 3)
                return;

            parts[2] = index;

            var icon = parts[0] as Image;
            var label = parts[1] as Label;
            if (icon == null || label == null)
                return;

            label.text = item.Label;
            if (item.IconSource != null)
            {
                var content = EditorGUIUtility.ObjectContent(item.IconSource, item.IconSource.GetType());
                if (content != null && content.image != null)
                    icon.image = (Texture2D)content.image;
                else
                    icon.image = null;
            }
            else
            {
                icon.image = null;
            }
        }

        private void OnSelectionChanged(IEnumerable<object> selected)
        {
            foreach (var o in selected)
            {
                if (o is FavoriteDisplayItem item)
                {
                    SelectFavorite(item);
                    break;
                }
            }
        }

        private void OnItemChosen(IEnumerable<object> chosen)
        {
            OnSelectionChanged(chosen);
        }

        private void SelectFavorite(FavoriteDisplayItem item)
        {
            bool shift = Event.current != null && Event.current.shift;
            bool ctrl = Event.current != null && Event.current.control;

            Object target = TryResolve(item.GlobalId);
            if (target == null)
            {
                string pathToOpen = !string.IsNullOrEmpty(item.ContainerPath) ? item.ContainerPath : ComputeContainerPathFromGlobal(item.GlobalId);
                bool openSingle = shift && !ctrl;
                EnsureContainerLoaded(pathToOpen, openSingle);
                target = TryResolve(item.GlobalId);
            }

            if (target == null)
            {
                FavoritesManager.RemoveManualFavoriteByGlobalId(item.GlobalId);
                RefreshContent();
                return;
            }

            if (shift)
                OpenContainerForObject(target);
            else if (ctrl)
                target = OpenAdditiveOrAddToScene(target) ?? target;

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);

            Close();
        }

        private static Object OpenAdditiveOrAddToScene(Object target)
        {
            if (target is SceneAsset scene)
            {
                string path = AssetDatabase.GetAssetPath(scene);
                if (!string.IsNullOrEmpty(path))
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                }
                return target;
            }

            GameObject prefabRoot = null;
            if (target is GameObject go)
            {
                if (PrefabUtility.IsPartOfPrefabAsset(go))
                    prefabRoot = go;
                else if (PrefabUtility.GetOutermostPrefabInstanceRoot(go) != null)
                    prefabRoot = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
            }
            else if (target is Component comp)
            {
                prefabRoot = PrefabUtility.GetCorrespondingObjectFromOriginalSource(comp.gameObject);
            }

            if (prefabRoot != null && PrefabUtility.IsPartOfPrefabAsset(prefabRoot))
            {
                var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                if (activeScene.IsValid())
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot, activeScene);
                    Undo.RegisterCreatedObjectUndo(instance, "Add prefab to scene");
                    return instance;
                }
            }

            return target;
        }

        public void RefreshContent()
        {
            if (_filterPane == null)
                return;

            _allItems.Clear();
            _filterTypes.Clear();
            _filterTypes.Add(ALL_FILTER);

            var manualEntries = FavoritesManager.GetManualFavoriteEntries();

            var seenIds = new HashSet<string>();

            void AddItem(FavoriteEntry entry, Object resolved, string label)
            {
                if (seenIds.Contains(entry.globalId))
                    return;
                seenIds.Add(entry.globalId);

                var typeFilter = GetTypeFilter(entry, resolved);
                _filterTypes.Add(typeFilter);

                _allItems.Add(new FavoriteDisplayItem
                {
                    Label = label,
                    GlobalId = entry.globalId,
                    ContainerPath = entry.containerPath,
                    PreLabel = entry.label,
                    IconSource = resolved,
                    TypeFilter = typeFilter,
                    Source = "Manual"
                });
            }

            foreach (var e in manualEntries)
            {
                var resolved = TryResolve(e.globalId);
                string label = !string.IsNullOrEmpty(e.label) ? e.label : BuildDisplayLabel(e, resolved);
                AddItem(e, resolved, label);
            }

            RebuildFilterButtons();
            ApplyFilter();
        }

        private string GetTypeFilter(FavoriteEntry entry, Object resolved)
        {
            if (resolved == null)
                return "Unknown";

            if (resolved is SceneAsset)
                return "Scene";

            string path = AssetDatabase.GetAssetPath(resolved);
            if (!string.IsNullOrEmpty(path))
            {
                if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    return "Prefab";
                if (path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                    return "ScriptableObject";
                if (path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                    return "Material";
                if (path.EndsWith(".controller", StringComparison.OrdinalIgnoreCase))
                    return "Animator";
            }

            if (resolved is Component c)
                return c.GetType().Name;

            if (resolved is GameObject)
                return "GameObject";

            return resolved.GetType().Name;
        }

        private void RebuildFilterButtons()
        {
            _filterPane.Clear();
            var filterLabel = new Label("Filters");
            filterLabel.AddToClassList("filter-section-label");
            _filterPane.Add(filterLabel);

            var sortedFilters = _filterTypes.OrderBy(f =>
            {
                if (f == ALL_FILTER) return "0";
                return "1_" + f;
            }).ToList();
            if (!sortedFilters.Contains(ALL_FILTER))
                sortedFilters.Insert(0, ALL_FILTER);

            foreach (var filter in sortedFilters)
            {
                var filterName = filter;
                var btn = new Button(() => SetFilter(filterName))
                {
                    text = filter
                };
                btn.AddToClassList("filter-item");
                if (filter == _activeFilter)
                    btn.AddToClassList("selected");
                _filterPane.Add(btn);
            }
        }

        private void SetFilter(string filter)
        {
            _activeFilter = filter;
            RebuildFilterButtons();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            _filteredItems.Clear();
            if (_activeFilter == ALL_FILTER)
                _filteredItems.AddRange(_allItems);
            else
                _filteredItems.AddRange(_allItems.Where(x => x.TypeFilter == _activeFilter));

            _itemsList.itemsSource = _filteredItems;
            _itemsList.Rebuild();

            if (_emptyState != null)
                _emptyState.style.display = _filteredItems.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static Object TryResolve(string globalId)
        {
            if (string.IsNullOrEmpty(globalId))
                return null;
            if (!GlobalObjectId.TryParse(globalId, out var gid))
                return null;
            Object[] objs = new Object[1];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(new[] { gid }, objs);
            return objs[0];
        }

        private static void EnsureContainerLoaded(string path, bool openSingle)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (sceneAsset == null)
                    return;
                if (openSingle)
                    UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                var mode = openSingle ? UnityEditor.SceneManagement.OpenSceneMode.Single : UnityEditor.SceneManagement.OpenSceneMode.Additive;
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, mode);
                return;
            }

            if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                var main = AssetDatabase.LoadMainAssetAtPath(path);
                if (main != null)
                    AssetDatabase.OpenAsset(main);
                return;
            }

            AssetDatabase.LoadMainAssetAtPath(path);
        }

        private static void OpenContainerForObject(Object obj)
        {
            if (obj == null)
                return;

            if (obj is SceneAsset scene)
            {
                string path = AssetDatabase.GetAssetPath(scene);
                if (!string.IsNullOrEmpty(path))
                {
                    UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                }
                return;
            }

            if (PrefabUtility.IsPartOfPrefabAsset(obj))
                AssetDatabase.OpenAsset(obj);
        }

        private static string ComputeContainerPathFromGlobal(string globalId)
        {
            if (!GlobalObjectId.TryParse(globalId, out var gid))
                return string.Empty;
            return AssetDatabase.GUIDToAssetPath(gid.assetGUID.ToString());
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
    }
}
