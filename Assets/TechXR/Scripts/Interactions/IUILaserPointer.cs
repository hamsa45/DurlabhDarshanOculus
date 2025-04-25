using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechXR.Core.Sense
{
    abstract public class IUILaserPointer : MonoBehaviour
    {
        #region PUBLIC_MEMBERS
        /// <summary>
        /// Laser thickness
        /// </summary>
        public float LaserThickness = 0.002f;
        /// <summary>
        /// Laser hitpoint scale
        /// </summary>
        public float LaserHitScale = 0.02f;
        /// <summary>
        /// Laser always on or only on demand
        /// </summary>
        public bool LaserAlwaysOn = true;
        /// <summary>
        /// laser display color
        /// </summary>
        public Color Color;
        #endregion // PUBLIC_MEMBERS
        //
        #region PROTECTED_MEMBERS
        /// <summary>
        /// RaycastHit object
        /// </summary>
        protected RaycastHit m_RaycastHit;
        #endregion // PROTECTED_MEMBERS
        //
        #region PRIVATE_MEMBERS
        //
        // laser pointer toggle flag for cube/controller
        private bool m_IsCube = new bool();
        // track conditions flag for cube/controller
        private bool m_Conditions = new bool();
        //
        // raycast hitpoint object
        private GameObject m_HitPoint;
        // raycast pointer object
        private GameObject m_Pointer;
        // raycast distance limit
        private float m_DistanceLimit;
        // raycast display flag
        private bool m_DisplayFlag = new bool();
        // raycast mode changed flag
        private bool m_ModeChanged = new bool();
        // laser pointer material
        private Material m_Material;
        #endregion // PRIVATE_MEMBERS;
        //
        #region MONOBEHAVIOUR_METHODS
        // Awake
        private void Awake()
        {
            // Create material with shader
            //m_Material = new Material(Shader.Find("TechXR/LaserPointer"));
            //m_Material.SetColor("_Color", Color);
            //
            m_DisplayFlag = true;
            m_IsCube = false;
            //m_InteractionMode = InputManager.InteractionMode.LaserPointer;
            //SenseManager._instance.CurrentPointerMode = Defs.PointerDisplayMode.LaserPointer;
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            //Debug.Log("IUILaserPointer :: Start()");

            // Create Laser Pointer Container gameobject
            GameObject laserPointerContainer = new GameObject("LaserPointerContainer");
            laserPointerContainer.transform.SetParent(transform);
            laserPointerContainer.transform.localPosition = Vector3.zero;
            laserPointerContainer.transform.localRotation = Quaternion.identity;

            // pointer object
            m_Pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_Pointer.transform.SetParent(laserPointerContainer.transform, false);
            m_Pointer.transform.localScale = new Vector3(LaserThickness, LaserThickness, 100.0f);
            m_Pointer.transform.localPosition = new Vector3(0.0f, 0.0f, 50.0f);

            m_Pointer.SetActive(m_DisplayFlag);
            // hitpoint object
            m_HitPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_HitPoint.transform.SetParent(laserPointerContainer.transform, false);
            m_HitPoint.transform.localScale = new Vector3(LaserHitScale, LaserHitScale, LaserHitScale);
            m_HitPoint.transform.localPosition = new Vector3(0.0f, 0.0f, 100.0f);

            m_HitPoint.SetActive(false);

            // remove the colliders on our primitives
            Object.DestroyImmediate(m_HitPoint.GetComponent<SphereCollider>());
            Object.DestroyImmediate(m_Pointer.GetComponent<BoxCollider>());

            // create new material with shader and assign
            m_Material = new Material(Shader.Find("TechXR/LaserPointer"));
            m_Material.SetColor("_Color", Color);

            m_Pointer.GetComponent<MeshRenderer>().material = m_Material;
            m_HitPoint.GetComponent<MeshRenderer>().material = m_Material;

            // initialize concrete class
            Initialize();

            // register with the LaserPointerInputModule
            if (LaserPointerInputModule.instance == null)
            {
                // if LaserPointerInputModule is not found, create a new one
                new GameObject().AddComponent<LaserPointerInputModule>();
            }

            LaserPointerInputModule.instance.AddController(this);
        }

        void OnDestroy()
        {
            if (LaserPointerInputModule.instance != null)
            {
                LaserPointerInputModule.instance.RemoveController(this);
            }
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            /*if (m_IsCube)
                m_Conditions = m_DisplayFlag;
            else
                m_Conditions = m_DisplayFlag && SenseManager._instance.CurrentPointerMode == Defs.PointerDisplayMode.LaserPointer;*/

            //if (m_DisplayFlag && m_InteractionMode == InputManager.InteractionMode.LaserPointer)
            //if (m_DisplayFlag && SenseManager._instance.CurrentPointerMode == Defs.PointerDisplayMode.LaserPointer)
            if(m_Conditions)
            {
                Ray ray = new Ray(transform.position, transform.forward);
                RaycastHit hitInfo;

                bool bHit = Physics.Raycast(ray, out hitInfo);

                m_RaycastHit = hitInfo;

                float distance = 100.0f;

                if (bHit)
                {
                    distance = hitInfo.distance;
                }

                // ugly, but has to do for now
                if (m_DistanceLimit > 0.0f)
                {
                    distance = Mathf.Min(distance, m_DistanceLimit);
                    bHit = true;
                }

                m_Pointer.transform.localScale = new Vector3(LaserThickness, LaserThickness, distance);
                m_Pointer.transform.localPosition = new Vector3(0.0f, 0.0f, distance * 0.5f);

                if (bHit)
                {
                    m_HitPoint.SetActive(true);
                    m_HitPoint.transform.localPosition = new Vector3(0.0f, 0.0f, distance);
                }
                else
                {
                    m_HitPoint.SetActive(false);
                }
            }

            // reset the previous distance limit
            m_DistanceLimit = -1.0f;
        }
        #endregion // MONOBEHAVIOUR_METHODS
        //
        #region PRIVATE_METHODS
        /// <summary>
        /// Toggle pointer display when switching between the two modes
        /// </summary>
        /// <param name="mode"></param>
        private void DisplayToggleOnModeChange(Defs.PointerDisplayMode mode)
        {
            if (mode == Defs.PointerDisplayMode.Teleporter)
            {
                if (m_DisplayFlag)
                {
                    m_ModeChanged = true;
                    ToggleDisplay(false);
                }
            }
            else if (m_ModeChanged)
            {
                m_ModeChanged = false;
                ToggleDisplay(true);
            }
        }
        #endregion // PRIVATE_METHODS
        //
        #region PROTECTED_METHODS
        protected virtual void Initialize() { }
        #endregion
        //
        #region PUBLIC_METHODS
        /**
         * Implement these in the concrete class
         */
        public virtual void OnEnterControl(GameObject control) { }
        public virtual void OnExitControl(GameObject control) { }

        /// <summary>
        /// Toggle the laser pointer display on/off
        /// </summary>
        /// <param name="flag"></param>
        public virtual void ToggleDisplay(bool flag) 
        {
            if (m_Pointer != null)
            {
                m_Pointer.SetActive(flag);
                m_HitPoint.SetActive(flag);
            }
            //
            m_DisplayFlag = flag;
        }
        /// <summary>
        /// Set pointer display mode
        /// </summary>
        /// <param name="mode"></param>
        public virtual void SetPointerDisplayMode(Defs.PointerDisplayMode mode)
        {
            //m_InteractionMode = mode;
            //SenseManager._instance.CurrentPointerMode = mode;

            DisplayToggleOnModeChange(mode);
        }
        abstract public bool ButtonDown();
        abstract public bool ButtonUp();

        /// <summary>
        /// Limits the laser distance for the current frame
        /// </summary>
        /// <param name="distance"></param>
        public virtual void LimitLaserDistance(float distance)
        {
            if (distance < 0.0f)
                return;

            if (m_DistanceLimit < 0.0f)
                m_DistanceLimit = distance;
            else
                m_DistanceLimit = Mathf.Min(m_DistanceLimit, distance);
        }

        /// <summary>
        /// Set the laser pointer color
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color color)
        {
            m_Material.SetColor("_Color", color);
        }
        #endregion //PUBLIC_METHODS
    }
}

