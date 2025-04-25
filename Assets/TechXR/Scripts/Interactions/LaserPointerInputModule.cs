using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TechXR.Core.Sense
{
    /// <summary>
    /// Laser pointer input module for the event system
    /// </summary>
    public class LaserPointerInputModule : StandaloneInputModule // BaseInputModule
    {
        #region PUBLIC_MEMBERS
        /// <summary>
        /// Static class reference
        /// </summary>
        public static LaserPointerInputModule instance { get { return _instance; } }
        /// <summary>
        /// Layer mask to consider for laser pointer interaction
        /// </summary>
        public LayerMask layerMask;
        #endregion // PUBLIC_MEMBERS
        //// storage class for controller specific data
        //private class ControllerData
        //{
        //    public LaserPointerEventData pointerEvent;
        //    public GameObject currentPoint;
        //    public GameObject currentPressed;
        //    public GameObject currentDragging;
        //};
        //
        #region PRIVATE_MEMBERS
        private static LaserPointerInputModule _instance = null;

        private Camera UICamera;
        private PhysicsRaycaster Raycaster;
        private HashSet<IUILaserPointer> m_Controllers;
        // controller data
        private Dictionary<IUILaserPointer, ControllerData> m_ControllerData = new Dictionary<IUILaserPointer, ControllerData>();
        #endregion // PRIVATE_MEMBERS;
        //
        #region MONOBEHAVIOUR_METHODS
        protected override void Awake()
        {
            base.Awake();

            if (_instance != null)
            {
                Debug.LogWarning("Trying to instantiate multiple LaserPointerInputModule.");
                //DestroyImmediate(this.gameObject);
            }

            _instance = this;

            // set the layer mask to everything minus the ignore raycast layer
            layerMask = LayerMask.GetMask(LayerMask.LayerToName(0), LayerMask.LayerToName(1), LayerMask.LayerToName(3), LayerMask.LayerToName(4), LayerMask.LayerToName(5), LayerMask.LayerToName(6), LayerMask.LayerToName(7), LayerMask.LayerToName(8));
        }
        // Start is called before the first frame update
        protected override void Start()
        {
            //print("LaserPointerInputModule :: Start()");

            base.Start();

            // Create a new camera that will be used for raycasts
            UICamera = new GameObject("UI Camera").AddComponent<Camera>();
            // Added PhysicsRaycaster so that pointer events are sent to 3d objects
            Raycaster = UICamera.gameObject.AddComponent<PhysicsRaycaster>();
            UICamera.clearFlags = CameraClearFlags.Nothing;
            UICamera.enabled = false;
            UICamera.fieldOfView = 5;
            UICamera.nearClipPlane = 0.01f;

            // Find canvases in the scene and assign our custom
            // UICamera to them
            Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                if (canvas.worldCamera == null)
                {
                    canvas.worldCamera = UICamera;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
        #endregion // MONOBEHAVIOUR_METHODS
        //
        #region PRIVATE_METHODS
        // Select a game object
        private void Select(GameObject go)
        {
            ClearSelection();

            if (ExecuteEvents.GetEventHandler<ISelectHandler>(go))
            {
                base.eventSystem.SetSelectedGameObject(go);
            }
        }
        #endregion // PRIVATE_METHODS
        //
        #region PROTECTED_METHODS
        /// <summary>
        /// Update UICamera position for raycast
        /// </summary>
        /// <param name="controller"></param>
        protected void UpdateCameraPosition(IUILaserPointer controller)
        {
            UICamera.transform.position = controller.transform.position;
            UICamera.transform.rotation = controller.transform.rotation;
        }
        #endregion
        //
        #region PUBLIC_METHODS
        /// <summary>
        /// Add laser pointer controller
        /// </summary>
        /// <param name="controller"></param>
        public void AddController(IUILaserPointer controller)
        {
            m_ControllerData.Add(controller, new ControllerData());
        }

        /// <summary>
        /// Remove laser pointer controller
        /// </summary>
        /// <param name="controller"></param>
        public void RemoveController(IUILaserPointer controller)
        {
            m_ControllerData.Remove(controller);
        }

        /// <summary>
        /// Clear the current selection
        /// </summary>
        public void ClearSelection()
        {
            if (base.eventSystem.currentSelectedGameObject)
            {
                base.eventSystem.SetSelectedGameObject(null);
            }
        }

        /// <summary>
        /// Process the pointer events
        /// </summary>
        public override void Process()
        {
            //
            base.Process();
            //

            Raycaster.eventMask = layerMask;
            //EventSystem.current = eventSystem;

            foreach (var pair in m_ControllerData)
            {
                IUILaserPointer controller = pair.Key;
                ControllerData data = pair.Value;

                // Test if UICamera is looking at a GUI element
                UpdateCameraPosition(controller);

                if (data.PointerEvent == null)
                    data.PointerEvent = new LaserPointerEventData(eventSystem);
                else
                    data.PointerEvent.Reset();

                data.PointerEvent.Controller = controller;
                data.PointerEvent.delta = Vector2.zero;
                data.PointerEvent.position = new Vector2(UICamera.pixelWidth * 0.5f, UICamera.pixelHeight * 0.5f);
                //data.pointerEvent.scrollDelta = Vector2.zero;

                //print("controller: " + data.pointerEvent.Controller.name + ", delta: " + data.pointerEvent.delta + ", position: " + data.pointerEvent.position);

                // trigger a raycast
                eventSystem.RaycastAll(data.PointerEvent, m_RaycastResultCache);
                data.PointerEvent.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
                m_RaycastResultCache.Clear();

                // make sure our controller knows about the raycast result
                // we add 0.01 because that is the near plane distance of our camera and we want the correct distance
                if (data.PointerEvent.pointerCurrentRaycast.distance > 0.0f)
                {
                    //print("distance found!");
                    controller.LimitLaserDistance(data.PointerEvent.pointerCurrentRaycast.distance + 0.01f);
                }

                // stop if no UI element was hit
                //if (data.pointerEvent.pointerCurrentRaycast.gameObject == null)
                    //print("Nothing found!");
                    //return;

                // Send control enter and exit events to our controller
                var hitControl = data.PointerEvent.pointerCurrentRaycast.gameObject;

                if (data.CurrentPoint != hitControl)
                {
                    if (data.CurrentPoint != null)
                    {
                        //data.PointerEvent.Current = data.CurrentPoint;
                        controller.OnExitControl(data.CurrentPoint);
                        //controller.OnExitControl(data.CurrentPoint, data.PointerEvent);
                    }
                    if (hitControl != null)
                    {
                        //data.PointerEvent.Current = hitControl;
                        //data.PointerEvent.pointerEnter = hitControl;
                        controller.OnEnterControl(hitControl);
                        //controller.OnEnterControl(hitControl, data.PointerEvent);
                    }

                }

                data.CurrentPoint = hitControl;

                //eventSystem.SetSelectedGameObject(hitControl);
                //print("data.currentPoint: " + data.currentPoint.name);

                // Handle enter and exit events on the GUI controlls that are hit
                base.HandlePointerExitAndEnter(data.PointerEvent, data.CurrentPoint);

                if (controller.ButtonDown())
                {
                    //print("ButtonDown()");
                    ClearSelection();

                    data.PointerEvent.pressPosition = data.PointerEvent.position;
                    data.PointerEvent.pointerPressRaycast = data.PointerEvent.pointerCurrentRaycast;
                    data.PointerEvent.pointerPress = null;

                    // update current pressed if the curser is over an element
                    if (data.CurrentPoint != null)
                    {
                        // set the current selected game object for event handling
                        eventSystem.SetSelectedGameObject(hitControl);
                        //print("data.currentPoint != null");

                        data.CurrentPressed = data.CurrentPoint;
                        data.PointerEvent.Current = data.CurrentPressed;

                        GameObject newPressed = ExecuteEvents.ExecuteHierarchy(data.CurrentPressed, data.PointerEvent, ExecuteEvents.pointerDownHandler);
                        ExecuteEvents.Execute(controller.gameObject, data.PointerEvent, ExecuteEvents.pointerDownHandler);

                        if (newPressed == null)
                        {
                            // some UI elements might only have click handler and not pointer down handler
                            newPressed = ExecuteEvents.ExecuteHierarchy(data.CurrentPressed, data.PointerEvent, ExecuteEvents.pointerClickHandler);
                            ExecuteEvents.Execute(controller.gameObject, data.PointerEvent, ExecuteEvents.pointerClickHandler);
                            if (newPressed != null)
                            {
                                data.CurrentPressed = newPressed;
                            }
                        }
                        else
                        {
                            data.CurrentPressed = newPressed;
                            // we want to do click on button down at same time, unlike regular mouse processing
                            // which does click when mouse goes up over same object it went down on
                            // reason to do this is head tracking might be jittery and this makes it easier to click buttons
                            ExecuteEvents.Execute(newPressed, data.PointerEvent, ExecuteEvents.pointerClickHandler);
                            ExecuteEvents.Execute(controller.gameObject, data.PointerEvent, ExecuteEvents.pointerClickHandler);

                        }

                        if (newPressed != null)
                        {
                            data.PointerEvent.pointerPress = newPressed;
                            data.CurrentPressed = newPressed;
                            //
                            Select(data.CurrentPressed);
                        }

                        ExecuteEvents.Execute(data.CurrentPressed, data.PointerEvent, ExecuteEvents.beginDragHandler);
                        ExecuteEvents.Execute(controller.gameObject, data.PointerEvent, ExecuteEvents.beginDragHandler);

                        data.PointerEvent.pointerDrag = data.CurrentPressed;
                        data.CurrentDragging = data.CurrentPressed;
                    }
                }// button down end

                if (controller.ButtonUp())
                {
                    //print("ButtonUp()");
                    if (data.CurrentDragging != null)
                    {
                        data.PointerEvent.Current = data.CurrentDragging;
                        //
                        ExecuteEvents.Execute(data.CurrentDragging, data.PointerEvent, ExecuteEvents.endDragHandler);
                        ExecuteEvents.Execute(controller.gameObject, data.PointerEvent, ExecuteEvents.endDragHandler);
                        //
                        if (data.CurrentPoint != null)
                        {
                            ExecuteEvents.ExecuteHierarchy(data.CurrentPoint, data.PointerEvent, ExecuteEvents.dropHandler);
                        }
                        //
                        data.PointerEvent.pointerDrag = null;
                        data.CurrentDragging = null;
                    }
                    if (data.CurrentPressed)
                    {
                        data.PointerEvent.Current = data.CurrentPressed;
                        //
                        ExecuteEvents.Execute(data.CurrentPressed, data.PointerEvent, ExecuteEvents.pointerUpHandler);
                        ExecuteEvents.Execute(controller.gameObject, data.PointerEvent, ExecuteEvents.pointerUpHandler);
                        //
                        data.PointerEvent.rawPointerPress = null;
                        data.PointerEvent.pointerPress = null;
                        data.CurrentPressed = null;
                    }
                }

                // drag handling
                if (data.CurrentDragging != null)
                {
                    data.PointerEvent.Current = data.CurrentPressed;
                    ExecuteEvents.Execute(data.CurrentDragging, data.PointerEvent, ExecuteEvents.dragHandler);
                    ExecuteEvents.Execute(controller.gameObject, data.PointerEvent, ExecuteEvents.dragHandler);
                }

                // update selected element for keyboard focus
                if (base.eventSystem.currentSelectedGameObject != null)
                {
                    data.PointerEvent.Current = eventSystem.currentSelectedGameObject;
                    //
                    ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
                    //ExecuteEvents.Execute(controller.gameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
                }
            }
        }
        #endregion //PUBLIC_METHODS
    }
}

