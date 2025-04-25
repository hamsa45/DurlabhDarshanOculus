using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechXR.Core.Sense
{
    /// <summary>
    /// Application level defintions
    /// </summary>
    public class Defs
    {
        #region PUBLIC_MEMBERS
        // Pointer display mode
        public enum PointerDisplayMode
        {
            LaserPointer,
            Teleporter
        };

        // Object layers
        public const string DEFAULT_LAYER = "Default";
        public const string IGNORE_RAYCAST_LAYER = "Ignore Raycast";
        #endregion // PUBLIC_MEMBERS
    }
}

