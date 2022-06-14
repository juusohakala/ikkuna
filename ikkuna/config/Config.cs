using System;
using System.Collections.Generic;
using System.Text;

namespace ikkuna
{
    public class Config
    {
        public List<Hotkey> Hotkeys { get; set; }
        public List<Slot> Slots { get; set; }


        public Slot GetSlotByName(string slotName)
        {

            foreach (var slot in Slots)
            {
                if (slot.SlotName == slotName) return slot;
            }


            return null;
        }
    }
}
