using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TechXR.Core.Sense
{
    /// <summary>
    /// Data class for the laser pointer event
    /// </summary>
    internal class LaserPointerEventData : PointerEventData
    {
        #region PUBLIC_MEMBERS
        /// <summary>
        /// GameObject hit by the laser pointer
        /// </summary>
        public GameObject Current;
        /// <summary>
        /// LaserPointer controller
        /// </summary>
        public IUILaserPointer Controller;
        /// <summary>
        /// Event data for the laser pointer event
        /// </summary>
        /// <param name="e"></param>
        public LaserPointerEventData(EventSystem e) : base(e) { }
        #endregion // PUBLIC_MEMBERS
        //
        #region PRIVATE_MEMBERS
        #endregion // PRIVATE_MEMBERS;
        //
        #region MONOBEHAVIOUR_METHODS
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        #endregion // MONOBEHAVIOUR_METHODS
        //
        #region PRIVATE_METHODS
        #endregion // PRIVATE_METHODS
        //
        #region PUBLIC_METHODS
        /// <summary>
        /// Reset event data
        /// </summary>
        public override void Reset()
        {
            Current = null;
            Controller = null;
            //
            base.Reset();
        }
        #endregion //PUBLIC_METHODS
    }
}

