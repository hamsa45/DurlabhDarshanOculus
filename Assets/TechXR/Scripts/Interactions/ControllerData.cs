using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechXR.Core.Sense
{
    /// <summary>
    /// Data class for the laser pointer controller
    /// </summary>
    internal class ControllerData
    {
        #region PUBLIC_MEMBERS
        /// <summary>
        /// PointerEventData for the LaserPointerEvent
        /// </summary>
        public LaserPointerEventData PointerEvent;
        /// <summary>
        /// GameObject hit by the laser pointer
        /// </summary>
        public GameObject CurrentPoint;
        /// <summary>
        /// GameObject over which the laser pointer is hovering
        /// </summary>
        public GameObject CurrentPressed;
        /// <summary>
        /// GameObject over which the controller was clicked and the pointer dragged
        /// </summary>
        public GameObject CurrentDragging;
        #endregion // PUBLIC_MEMBERS
        //
        #region PRIVATE_MEMBERS
        #endregion // PRIVATE_MEMBERS;
        //
        #region PRIVATE_METHODS
        #endregion // PRIVATE_METHODS
        //
        #region PUBLIC_METHODS
        #endregion //PUBLIC_METHODS
    }
}

