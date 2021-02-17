using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.EditorHistoryNavigation
{
    [Serializable]
    internal class SelectionData : ISerializationCallbackReceiver
    {
        private Object[] selection;

        [SerializeField]
        private List<string> guids = new List<string>();

        [SerializeField]
        private List<int> instanceIDs = new List<int>();

        [SerializeField]
        private string displayName;

        private GUIContent cachedGUIContent;
        public GUIContent GUIContent
        {
            get
            {
                if (cachedGUIContent != null)
                    return cachedGUIContent;
                Object first = selection.First();
                cachedGUIContent = new GUIContent(displayName, EditorGUIUtility.ObjectContent(first, first.GetType()).image);
                return cachedGUIContent;
            }
        }

        public bool IsValid => selection.Any(selectionObj => selectionObj != null);

        public SelectionData(Object[] objects)
        {
            selection = objects;
            displayName = string.Empty;
            for (int i = 0; i < selection.Length; i++)
            {
                Object o = selection[i];
                if (o == null)
                    continue;
                if (o is GameObject gameObject)
                {
                    instanceIDs.Add(gameObject.GetInstanceID());
                }
                else
                {
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
                    guids.Add(guid);
                }
            }

            displayName = string.Join(",", selection.Where(o => o != null).Select(o => o.name));
            if (displayName.Length > 50)
                displayName = displayName.Substring(0, 47) + "...";
        }

        public void Select()
        {
            Selection.objects = selection.Where(o => o != null).ToArray();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            List<Object> storedObjs = new List<Object>();
            for (int i = 0; i < guids.Count; i++)
                storedObjs.Add(AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guids[i])));
            for (int i = 0; i < instanceIDs.Count; i++)
                storedObjs.Add(EditorUtility.InstanceIDToObject(instanceIDs[i]));
            selection = storedObjs.ToArray();
        }
    }
}
