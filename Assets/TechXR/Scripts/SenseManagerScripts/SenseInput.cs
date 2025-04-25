using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using co.techxr.unity.exceptions;

namespace TechXR.Core.Sense
{
    public class SenseInput : MonoBehaviour
    {
        public static SenseInput Instance;
        // movement inputs
        //private static float HorizontalInput;
        //private static float VerticalInput;
        //
        public string PlatformConfiguration = PlatformConfig.AUTO_DETECT;
        //
        //public static bool 
        #region PUBLIC_MEMBERS
        /// <summary>
        /// Enable/Disable the joystick basedd movements
        /// </summary>
        public bool JoystickMovement { get; set; }
        /// <summary>
        /// Enable/Disable the teleport option
        /// </summary>
        public bool TeleportMovement { get; set; }
        #endregion // PUBLIC_MEMBERS
        //
        #region PRIVATE_MEMBERS
        private static IEvent m_Event;
        private static DefaultUnityInput m_DefaultUnityInput;
        private static AndroidInput m_AndroidInput;
        private static Android12KeyPress m_Android12Input;
        private static IosInput m_IosInput;
        private static OculusInput m_OculusInput;
        //
        private static Dictionary<ButtonName, OculusButton> m_TechXRToOculusDict = new Dictionary<ButtonName, OculusButton>();
        //
        private static int i = 0;
        //
        #endregion // PRIVATE_MEMBERS;

        #region MONOBEHAVIOUR_METHODS
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }
            Instance = this;
            //
            JoystickMovement = true;
        }

        private void Start()
        {
            InitialiseAndSetInputConfig();
        }

        // Update is called once per frame
        void Update()
        {
            i++;
            //
            if (m_Event == null)
            {
                Debug.Log("Event not found");
                return;
            }
            //
            m_Event.Update();
        }
        #endregion // MONOBEHAVIOUR_METHODS

        #region PRIVATE_METHODS
        private void InitialiseAndSetInputConfig()
        {
            InitialiseInputObjects();
            //
            if (PlatformConfiguration == PlatformConfig.AUTO_DETECT)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (AndroidVersion.SDK_INT > 30)
                    {
                        SetConfigObject(PlatformConfig.ANDROID_12);
                    }
                    else
                    {
                        SetConfigObject(PlatformConfig.ANDROID_DEFAULT);
                    }
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    SetConfigObject(PlatformConfig.IOS_DEFAULT);
                }
                else
                {
                    SetConfigObject(PlatformConfig.DEFAULT);
                }
            }
            else
            {
                SetConfigObject(PlatformConfiguration);
            }
            //
            Debug.Log("InitialiseAndSetInputConfig");
        }
        //
        private void InitialiseInputObjects()
        {
            m_DefaultUnityInput = new DefaultUnityInput();
            m_AndroidInput = new AndroidInput();
            m_Android12Input = new Android12KeyPress();
            m_IosInput = new IosInput();
            m_OculusInput = new OculusInput();
            //
            MapOculusKeys(m_TechXRToOculusDict);
        }
        private static void MapOculusKeys(Dictionary<ButtonName, OculusButton> d)
        {
            // Map Keys Here by iterating
            foreach (KeyValuePair<ButtonName, OculusButton> keyValuePair in d)
            {
                m_OculusInput.MapOculusKeys(keyValuePair.Key, keyValuePair.Value);
            }
        }
        //
        #endregion // PRIVATE_METHODS
        //
        #region PUBLIC_METHODS
        //
        internal static void SetConfigObject(string configName)
        {
            Debug.Log("Trying to set SenseInput config for " + configName);
            switch (configName)
            {
                case var _ when PlatformConfig.DEFAULT == configName:
                    m_Event = m_DefaultUnityInput;
                    break;
                case var _ when PlatformConfig.ANDROID_DEFAULT == configName:
                    m_Event = m_AndroidInput;
                    break;
                case var _ when PlatformConfig.ANDROID_12 == configName:
                    m_Event = m_Android12Input;
                    break;
                case var _ when PlatformConfig.IOS_DEFAULT == configName:
                    m_Event = m_IosInput;
                    break;
                case var _ when PlatformConfig.OCULUS == configName:
                    m_Event = m_OculusInput;
                    break;
                default:
                    Debug.Log("Selecting Default Unity config");
                    m_Event = m_DefaultUnityInput;
                    break;
            }
        }
        //  sensitity value 0 indicates lowest value, 1 indicates highest
        public static float setEventExpiryTime(float sensitivity)
        {
            return -1;
            //if (m_Android12Input.GetType() == typeof(Android12Input)) return ((Android12Input) m_Android12Input).setSensitivity(sensitivity);
            //else throw new Exception("Sensitivity applicable only for Android12. Android12 Input is null!");
        }
        //
        public static bool GetButtonDown(ButtonName buttonName)
        {
            if (m_Event == null)
            {
                Debug.Log("Event Not Found");
                return false;
            }

            bool returnValue = m_Event.GetButtonDown(buttonName);
            //if (i % 500 == 0 || returnValue == true) Debug.Log(string.Format("SenseInput: Checking Button {0} Down: {1}", buttonName, returnValue));
            return returnValue;
        }
        //
        public static bool GetButton(ButtonName buttonName)
        {
            if (m_Event == null)
            {
                Debug.Log("Event Not Found");
                return false;
            }

            bool returnValue = m_Event.GetButton(buttonName);
            //if (i % 500 == 0 || returnValue == true) Debug.Log(string.Format("SenseInput: Checking Button {0} Pressed: {1}", buttonName, returnValue));
            return returnValue;
        }
        //
        public static bool GetButtonUp(ButtonName buttonName)
        {
            if (m_Event == null)
            {
                Debug.Log("Event Not Found");
                return false;
            }

            bool returnValue = m_Event.GetButtonUp(buttonName);
            //if (i % 500 == 0 || returnValue == true) Debug.Log(string.Format("SenseInput: Checking Button {0} Up: {1}", buttonName, returnValue));
            return returnValue;
        }
        //
        public static float GetAxis(JoystickAxis axis)
        {
            if (m_Event == null)
            {
                Debug.Log("Event Not Found");
                return 0;
            }

            switch (axis)
            {
                case JoystickAxis.HORIZONTAL: return m_Event.GetAxis(JoystickAxis.HORIZONTAL);
                case JoystickAxis.VERTICAL: return m_Event.GetAxis(JoystickAxis.VERTICAL);
            }
            throw new XrException(string.Format("Please specify the axis for joystick input"));
        }
        //
        public static void MapTechXRWithOculus(ButtonName techxrButton, OculusButton oculusButton)
        {
            m_TechXRToOculusDict[techxrButton] = oculusButton;
        }
        #endregion //PUBLIC_METHODS
    }
}
