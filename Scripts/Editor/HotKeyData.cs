using System;
using UnityEngine;

namespace BrunoMikoski.EditorHistoryNavigation
{
    [Serializable]
    public class HotKeyData
    {
        [SerializeField]
        private KeyCode keycode;
        [SerializeField]
        private EventModifiers modifiers;
        [SerializeField]
        private int mouseButtonNumber;

        public HotKeyData(KeyCode currentKeyCode, EventModifiers currentModifiers)
        {
            keycode = currentKeyCode;
            modifiers = currentModifiers;
            mouseButtonNumber = -1;
        }

        public HotKeyData(int currentButton)
        {
            keycode = KeyCode.None;
            modifiers = EventModifiers.None;
            mouseButtonNumber = currentButton;
        }

        public HotKeyData()
        {
            
        }

        public string ToDisplay()
        {
            if (mouseButtonNumber != -1)
                return $"Mouse {mouseButtonNumber}";

            string displayString = "";
            if (modifiers.HasFlag(EventModifiers.Alt))
                displayString += "Atl + ";
            if(modifiers.HasFlag(EventModifiers.Command) || modifiers.HasFlag(EventModifiers.Control))
                displayString += "Ctrl + ";
            if(modifiers.HasFlag(EventModifiers.Shift))
                displayString += "Shift + ";

            return $"{displayString}{keycode.ToString()}";
        }

        public bool Match(Event targetEvent)
        {
            return keycode == targetEvent.keyCode && modifiers == targetEvent.modifiers;
        }
    }
}