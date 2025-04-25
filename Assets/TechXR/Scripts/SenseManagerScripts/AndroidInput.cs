using UnityEngine;

namespace TechXR.Core.Sense
{
    public class AndroidInput : IEvent
    {
        private IEvent m_Event;

        public AndroidInput()
        {
            if (Application.platform == RuntimePlatform.Android)
                Debug.Log("=================" + AndroidVersion.SDK_INT + "==================");
            if (Application.platform == RuntimePlatform.Android && AndroidVersion.SDK_INT > 30)
            {
                m_Event = new Android12Input();
                Debug.Log("=================Android12==================");
            }
            else
            {
                m_Event = new DefaultUnityInput();
                Debug.Log("=================Default==================");
            }
        }

        public bool GetButton(ButtonName buttonName)
        {
            return m_Event.GetButton(buttonName);
        }

        public bool GetButtonDown(ButtonName buttonName)
        {
            return m_Event.GetButtonDown(buttonName);
        }

        public bool GetButtonUp(ButtonName buttonName)
        {
            return m_Event.GetButtonUp(buttonName);
        }
        public float GetAxis(JoystickAxis joystickAxis)
        {
            return m_Event.GetAxis(joystickAxis);
        }



        public void Update()
        {
            m_Event.Update();
        }
    }
}