using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TechXR.Core.Sense
{
    /// <summary>
    /// Concrete laser pointer class to be added to the controller
    /// </summary>
    public class LaserPointer : IUILaserPointer//, I_XR
    {
        #region PUBLIC_MEMBERS
        //[HideInInspector]
        public ButtonName SenseXRTriggerButton;
        //
        [HideInInspector]
        public OculusButton OculusTriggerButton;
        [HideInInspector]
        public bool UsingOculus;
        //
        /// <summary>
        /// Use this variable if the laser is set to toggle on demand
        /// </summary>
        public bool ButtonState = false;
        #endregion // PUBLIC_MEMBERS
        //
        #region PRIVATE_MEMBERS
        [SerializeField]
        //private GameObject[] m_PointerHandle;
        //private GameObject m_LastActivePointerHandle;
        //private bool m_ButtonState = new bool();
        private bool m_PrevButtonState = new bool();
        private bool m_ButtonChanged = new bool();
        //
        private GameObject m_CurrentGameObject;
        //
        private ButtonName m_TriggerButton;
        #endregion // PRIVATE_MEMBERS;
        //

        //  Public static method to return I_XR handle.
        //  I_XR to be used as the gateway for controller.
        //public static I_XR getXR()
        //{
        //    GameObject xr = GameObject.Find("SenseXR");
        //    if (xr != null)
        //    {
        //        LaserPointer laserPointer = xr.GetComponent(typeof(LaserPointer)) as LaserPointer;
        //        I_XR i_XR = (I_XR)laserPointer;
        //        return i_XR;
        //    }
        //    else
        //    {
        //        throw new System.Exception("Unable to Find Controller");
        //    }

        //}
        //  Event listener
        //public event Notify ProcessCompleted; // event

        #region MONOBEHAVIOUR_METHODS
        private void Awake()
        {
            if (UsingOculus)
            {
                SenseInput.MapTechXRWithOculus(SenseXRTriggerButton, OculusTriggerButton);
            }

            //
            m_TriggerButton = SenseXRTriggerButton;
        }
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            //  Notify all listeners of the SenseEvents that have occured
            //OnProcessCompleted();
            // update here if the buttong mapping changes
            //if ((Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.U)) || (Input.GetKeyUp(KeyCode.JoystickButton5) || (Input.GetKeyUp(KeyCode.U))))
            //if (SenseInput.GetButtonDown("u") || SenseInput.GetButtonUp("u"))
            //if (SenseInput.GetButtonDown(SenseXRTriggerButton.ToString()) || SenseInput.GetButtonUp(SenseXRTriggerButton.ToString()))
            //if (SenseInput.GetButtonDown(m_TriggerButton) || SenseInput.GetButtonUp(m_TriggerButton))
            //{
            //    ButtonState = !ButtonState;
            //}
            if (SenseInput.GetButtonDown(m_TriggerButton))
            {
                ButtonState = true;
            }
            else if (SenseInput.GetButtonUp(m_TriggerButton))
            {
                ButtonState = false;
            }
            //
            if (ButtonState == m_PrevButtonState)
            {
                m_ButtonChanged = false;
            }
            else
            {
                m_ButtonChanged = true;
                m_PrevButtonState = ButtonState;
            }

            //if (ButtonDown())
            //    Debug.Log("Button down!");
            //if (ButtonUp())
            //    Debug.Log("Button up!");
        }
        #endregion // MONOBEHAVIOUR_METHODS
        //
        #region PRIVATE_METHODS
        #endregion // PRIVATE_METHODS
        //
        #region PUBLIC_METHODS
        /// <summary>
        /// Get button down state. Returns true if state changed and button down
        /// </summary>
        /// <returns></returns>
        public override bool ButtonDown()
        {
            return m_ButtonChanged && ButtonState;
        }

        /// <summary>
        /// Get button up state. Returns true if state changed and button up
        /// </summary>
        /// <returns></returns>
        public override bool ButtonUp()
        {
            return m_ButtonChanged && !ButtonState;
        }

        /// <summary>
        /// On pointer enter control
        /// </summary>
        /// <param name="control"></param>
        public override void OnEnterControl(GameObject control)
        {
            //Debug.Log("OnEnterControl " + control.name);
            m_CurrentGameObject = control;
        }

        /// <summary>
        /// On pointer exit control
        /// </summary>
        /// <param name="control"></param>
        public override void OnExitControl(GameObject control)
        {
            //Debug.Log("OnExitControl " + control.name);
            m_CurrentGameObject = null;
        }

        /// <summary>
        /// Toggle the lser pointer display on/off
        /// </summary>
        /// <param name="flag"></param>
        public override void ToggleDisplay(bool flag)
        {
            base.ToggleDisplay(flag);

            //foreach (GameObject pointerHandle in m_PointerHandle)
            //{
            //    if (pointerHandle.activeSelf)
            //    {
            //        m_LastActivePointerHandle = pointerHandle;
            //        break;
            //    }
            //}

            //m_LastActivePointerHandle.SetActive(flag);
        }

        //Vector3 I_XR.getXrPosition()
        //{
        //    return transform.position;
        //}

        //Quaternion I_XR.getQuaternion()
        //{
        //    return transform.rotation;
        //}
        //RaycastHit I_XR.getHitPoint()
        //{
        //    return hitInfo;
        //}

        //public Transform getTransform()
        //{
        //    return transform;
        //}

        //public bool addListener_FireButton(Notify notify)
        //{
        //    ProcessCompleted += notify;
        //    return true;
        //}
        //protected virtual void OnProcessCompleted() //protected virtual method
        //{
        //    SenseEvent[] senseEvents = getSenseEvents();

        //    //if Event list is non-empty then call delegate (if it is not null)
        //    if (senseEvents.Length > 0) ProcessCompleted?.Invoke(senseEvents);
        //}

        // Capture and return all the events that have happened in this frame
        //private SenseEvent[] getSenseEvents()
        //{
        //    List<SenseEvent> eventList = new List<SenseEvent>();

        //    if (Input.GetButtonDown("Fire1")) eventList.Add(SenseEvent.FIRE1_DOWN);
        //    if (Input.GetButtonDown("Fire2")) eventList.Add(SenseEvent.FIRE2_DOWN);
        //    if (Input.GetButtonDown("Fire3")) eventList.Add(SenseEvent.FIRE3_DOWN);
        //    if (Input.GetButtonDown("Fire4")) eventList.Add(SenseEvent.FIRE4_DOWN);
        //    if (Input.GetButtonDown("Fire5")) eventList.Add(SenseEvent.FIRE5_DOWN);

        //    if (Input.GetButtonUp("Fire1")) eventList.Add(SenseEvent.FIRE1_UP);
        //    if (Input.GetButtonUp("Fire2")) eventList.Add(SenseEvent.FIRE2_UP);
        //    if (Input.GetButtonUp("Fire3")) eventList.Add(SenseEvent.FIRE3_UP);
        //    if (Input.GetButtonUp("Fire4")) eventList.Add(SenseEvent.FIRE4_UP);
        //    if (Input.GetButtonUp("Fire5")) eventList.Add(SenseEvent.FIRE5_UP);

        //    if (Input.GetButton("Fire1")) eventList.Add(SenseEvent.FIRE1_PRESS_CONTINUE);
        //    if (Input.GetButton("Fire2")) eventList.Add(SenseEvent.FIRE2_PRESS_CONTINUE);
        //    if (Input.GetButton("Fire3")) eventList.Add(SenseEvent.FIRE3_PRESS_CONTINUE);
        //    if (Input.GetButton("Fire4")) eventList.Add(SenseEvent.FIRE4_PRESS_CONTINUE);
        //    if (Input.GetButton("Fire5")) eventList.Add(SenseEvent.FIRE5_PRESS_CONTINUE);

        //    return eventList.ToArray();
        //}

        //public Collider[] getAllColliders()
        //{
        //    Collider[] colliders = new Collider[5];
        //    colliders[0] = hitInfo.collider;        //  Sphere
        //    colliders[1] = m_Pointer.GetComponent<Collider>();  // Ray

        //    return colliders;
        //}

        //public void ignoreCollision(GameObject gameObject, bool ignore)
        //{
        //    if (ignore) gameObject.layer = 2;
        //    else gameObject.layer = 0;
        //}

        /// <summary>
        /// Get the current gameobject under the pointer
        /// </summary>
        /// <returns></returns>
        public GameObject GetCurrentGameObject()
        {
            return m_CurrentGameObject;
        }

        /// <summary>
        /// Return the RaycastHit
        /// </summary>
        /// <returns></returns>
        public RaycastHit GetRaycastHit()
        {
            return m_RaycastHit;
        }
        #endregion //PUBLIC_METHODS
    }
}
