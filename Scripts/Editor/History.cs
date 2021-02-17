using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.EditorHistoryNavigation
{
    [Serializable]
    internal class History
    {
        [SerializeField]
        private List<SelectionData> selectionData = new List<SelectionData>();
        public List<SelectionData> SelectionData => selectionData;

        [SerializeField]
        private int pointInTime = 0;
        public int PointInTime => pointInTime;

        private bool movingInHistory;

        public void AddToHistory(Object[] objects)
        {
            if (movingInHistory)
            {
                movingInHistory = false;
                return;
            }

            SelectionData item = new SelectionData(objects);
            if (!item.IsValid)
                return;
            if (pointInTime < selectionData.Count - 1)
                selectionData.RemoveRange(pointInTime, selectionData.Count - 1 - pointInTime);
            selectionData.Add(item);
            if (selectionData.Count > HistoryPanelCore.MaximumHistoryItems)
                selectionData.RemoveAt(0);
            pointInTime = selectionData.Count - 1;
        }

        public void Back()
        {
            if (pointInTime == 0)
                return;
            for (int i = pointInTime - 1; i >= 0; i--)
            {
                SelectionData data = selectionData[i];
                if (!data.IsValid)
                    continue;
                pointInTime = i;
                movingInHistory = true;
                data.Select();
                break;
            }
        }

        public void Forward()
        {
            if (pointInTime == selectionData.Count - 1)
                return;
            if (pointInTime + 1 > selectionData.Count - 1)
                return;
            pointInTime++;
            movingInHistory = true;
            selectionData[pointInTime].Select();
        }

        public void SetPointInTime(int itemIndex)
        {
            movingInHistory = true;
            pointInTime = itemIndex;
            selectionData[pointInTime].Select();
        }
    }
}
