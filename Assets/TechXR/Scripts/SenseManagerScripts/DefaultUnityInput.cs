using System;
using co.techxr.unity.exceptions;
using UnityEngine;

namespace TechXR.Core.Sense
{
    public class DefaultUnityInput : IEvent
    {
        int i = 0;
        int j = 0;
        public bool GetButton(ButtonName buttonName)
        {
            switch (buttonName)
            {
                case ButtonName.A:
                    return Input.GetKey(KeyCode.JoystickButton1) || Input.GetKey(KeyCode.Alpha1);
                case ButtonName.B:
                    return Input.GetKey(KeyCode.JoystickButton2) || Input.GetKey(KeyCode.Alpha2);
                case ButtonName.C:
                    return Input.GetKey(KeyCode.JoystickButton0) || Input.GetKey(KeyCode.Alpha3);
                case ButtonName.D:
                    return Input.GetKey(KeyCode.JoystickButton3) || Input.GetKey(KeyCode.Alpha4);
                case ButtonName.L:
                    return Input.GetKey(KeyCode.JoystickButton4) || Input.GetKey(KeyCode.L);
                case ButtonName.U:
                    return Input.GetKey(KeyCode.JoystickButton5) || Input.GetKey(KeyCode.U);

                default: return false;
            }
        }

        public bool GetButtonDown(ButtonName buttonName)
        {
            switch (buttonName)
            {
                case ButtonName.A:
                    return Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.Alpha1);
                case ButtonName.B:
                    return Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.Alpha2);
                case ButtonName.C:
                    return Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Alpha3);
                case ButtonName.D:
                    return Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.Alpha4);
                case ButtonName.L:
                    return Input.GetKeyDown(KeyCode.JoystickButton4) || Input.GetKeyDown(KeyCode.L);
                case ButtonName.U:
                    return Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.U);

                default: return false;
            }
        }

        public bool GetButtonUp(ButtonName buttonName)
        {
            switch (buttonName)
            {
                case ButtonName.A:
                    return Input.GetKeyUp(KeyCode.JoystickButton1) || Input.GetKeyUp(KeyCode.Alpha1);
                case ButtonName.B:
                    return Input.GetKeyUp(KeyCode.JoystickButton2) || Input.GetKeyUp(KeyCode.Alpha2);
                case ButtonName.C:
                    return Input.GetKeyUp(KeyCode.JoystickButton0) || Input.GetKeyUp(KeyCode.Alpha3);
                case ButtonName.D:
                    return Input.GetKeyUp(KeyCode.JoystickButton3) || Input.GetKeyUp(KeyCode.Alpha4);
                case ButtonName.L:
                    return Input.GetKeyUp(KeyCode.JoystickButton4) || Input.GetKeyUp(KeyCode.L);
                case ButtonName.U:
                    return Input.GetKeyUp(KeyCode.JoystickButton5) || Input.GetKeyUp(KeyCode.U);

                default: return false;
            }
        }

        public void Update()
        {

        }

        float IEvent.GetAxis(JoystickAxis joystickAxis)
        {
            switch (joystickAxis)
            {
                case JoystickAxis.HORIZONTAL:
                    {
                        float x = Input.GetAxis("Horizontal");
                        //if (i++%50 == 0) Debug.Log("X = " + x);
                        return x;
                    }
                case JoystickAxis.VERTICAL:
                    {
                        float y = Input.GetAxis("Vertical");
                        //if (j++ % 50 == 0) Debug.Log("Y = " + y);
                        return y;
                    }
            }
            throw new XrException(string.Format("Please provide the axis for joystick input"));
        }
    }
}