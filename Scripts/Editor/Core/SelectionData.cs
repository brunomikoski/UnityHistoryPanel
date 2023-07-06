using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory
{
    [Serializable]
    internal class SelectionData
    {
        [SerializeField]
        private List<string> guids = new List<string>();

        [SerializeField]
        private List<int> instanceIDs = new List<int>();

        private string m_displayName;
        public string DisplayName
        {
	        get
	        {
		        if (string.IsNullOrEmpty(m_displayName)) 
		        {
			        m_displayName = string.Join(", ", GetSelectionObjects().Where(o => o != null).Select(o => o.name));
			        if (m_displayName.Length > 50)
				        m_displayName = m_displayName.Substring(0, 47) + "...";
		        }
		        return m_displayName;
	        }
        }

        public bool IsValid => GetSelectionObjects().Any(selectionObj => selectionObj != null);

        public SelectionData(Object[] objects)
        {
            m_displayName = string.Empty;
            for (int i = 0; i < objects.Length; i++)
            {
                Object o = objects[i];
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
        }

        public void Select()
        {
            Selection.objects = GetSelectionObjects().Where(o => o != null).ToArray();
        }

        private List<Object> GetSelectionObjects()
        {
	        List<Object> storedObjs = new List<Object>();
	        for (int i = 0; i < guids.Count; i++)
		        storedObjs.Add(AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guids[i])));
	        for (int i = 0; i < instanceIDs.Count; i++)
		        storedObjs.Add(EditorUtility.InstanceIDToObject(instanceIDs[i]));
	        return storedObjs;
        }
    }
}
