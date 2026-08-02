using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.SelectionHistory
{
    [Serializable]
    internal class SelectionHistoryData
    {
        [SerializeField]
        private List<SelectionData> selectionData = new List<SelectionData>();
        public List<SelectionData> SelectionData => selectionData;

        [SerializeField]
        private int pointInTime;
        public int PointInTime => pointInTime;

        private SelectionData pendingNavigationTarget;

        public void AddToHistory(Object[] objects)
        {
            SelectionData navigationTarget = pendingNavigationTarget;
            pendingNavigationTarget = null;

            SelectionData item = new SelectionData(objects);
            if (!item.IsValid)
                return;

            if (navigationTarget != null && navigationTarget.Equals(item))
                return;

            if (pointInTime >= 0 && pointInTime < selectionData.Count &&
                selectionData[pointInTime].Equals(item))
                return;

            if (pointInTime < selectionData.Count - 1)
                selectionData.RemoveRange(pointInTime + 1,
                                          selectionData.Count - pointInTime - 1);

            selectionData.Add(item);

            if (selectionData.Count > SelectionHistoryToolbar.MaximumHistoryItems)
                selectionData.RemoveAt(0);

            pointInTime = selectionData.Count - 1;
        }

        public void Back()
        {
            if (pointInTime <= 0)
                return;

            for (int i = pointInTime - 1; i >= 0; i--)
            {
                if (!selectionData[i].IsValid)
                    continue;

                SelectAt(i);
                break;
            }
        }

        public void Forward()
        {
            if (pointInTime >= selectionData.Count - 1)
                return;

            int i = pointInTime + 1;
            while (i < selectionData.Count && !selectionData[i].IsValid)
                i++;

            if (i >= selectionData.Count)
                return;

            SelectAt(i);
        }

        public void SetPointInTime(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= selectionData.Count)
                return;

            SelectAt(itemIndex);
        }

        private void SelectAt(int index)
        {
            pointInTime = index;
            pendingNavigationTarget = new SelectionData(selectionData[index].Select());
        }
    }

}
