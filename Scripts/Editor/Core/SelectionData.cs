using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory
{
    [Serializable]
    internal class SelectionData : IEquatable<SelectionData>
    {
        [Serializable]
        private struct Entry
        {
            public string guid;
            public long localID;
            public EntityId instanceID;

            public bool IsAsset => !string.IsNullOrEmpty(guid);
        }

        [SerializeField]
        private List<Entry> entries = new List<Entry>();

        private string displayName;
        public string DisplayName
        {
	        get
	        {
		        if (string.IsNullOrEmpty(displayName))
		        {
			        displayName = string.Join(", ", GetSelectionObjects().Where(o => o != null).Select(o => o.name));
			        if (displayName.Length > 50)
				        displayName = displayName.Substring(0, 47) + "...";
		        }
		        return displayName;
	        }
        }

        public bool IsValid => GetSelectionObjects().Any(selectionObj => selectionObj != null);

        public SelectionData(Object[] objects)
        {
            displayName = string.Empty;
            for (int i = 0; i < objects.Length; i++)
            {
                Object o = objects[i];
                if (o == null)
                    continue;

                if (EditorUtility.IsPersistent(o) &&
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o, out string guid, out long localID))
                {
                    entries.Add(new Entry { guid = guid, localID = localID });
                }
                else
                {
                    entries.Add(new Entry { instanceID = o.GetEntityId() });
                }
            }
        }

        public Object[] Select()
        {
            Object[] selectedObjects = GetSelectionObjects().Where(o => o != null).ToArray();
            Selection.objects = selectedObjects;
            return selectedObjects;
        }

        private List<Object> GetSelectionObjects()
        {
            List<Object> storedObjs = new List<Object>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
                storedObjs.Add(Resolve(entries[i]));
            return storedObjs;
        }

        private static Object Resolve(Entry entry)
        {
            if (!entry.IsAsset)
                return EditorUtility.EntityIdToObject(entry.instanceID);

            string assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
            if (string.IsNullOrEmpty(assetPath))
                return null;

            Object mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (mainAsset != null && GetLocalID(mainAsset) == entry.localID)
                return mainAsset;

            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < allAssets.Length; i++)
            {
                if (allAssets[i] != null && GetLocalID(allAssets[i]) == entry.localID)
                    return allAssets[i];
            }

            return null;
        }

        private static long GetLocalID(Object asset)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string _, out long localID))
                return localID;

            return 0;
        }

        public bool Equals(SelectionData other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(other, this))
                return true;

            if (entries.Count != other.entries.Count)
                return false;

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                Entry otherEntry = other.entries[i];

                if (!string.Equals(entry.guid, otherEntry.guid, StringComparison.Ordinal))
                    return false;
                if (entry.localID != otherEntry.localID)
                    return false;
                if (!entry.instanceID.Equals(otherEntry.instanceID))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SelectionData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < entries.Count; i++)
                {
                    Entry entry = entries[i];
                    hash = hash * 31 + (entry.guid != null ? entry.guid.GetHashCode() : 0);
                    hash = hash * 31 + entry.localID.GetHashCode();
                    hash = hash * 31 + entry.instanceID.GetHashCode();
                }
                return hash;
            }
        }
    }
}
