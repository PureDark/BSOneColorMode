using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OneColorMode
{
    public class OneColorModeToggler: MonoBehaviour
    {
        public void Awake()
        {
            ToggleLeftSaber(false);
        } 

        public void ToggleLeftSaber()
        {
            ToggleLeftSaber(!OneColorModeBehaviour.IsLeftSaberOn);
        }

        public void ToggleLeftSaber(bool enabled)
        {
            OneColorModeBehaviour.IsLeftSaberOn = enabled;
        }
    }
}
