using co.techxr.unity.exceptions;
using UnityEngine;

namespace TechXR.Core.Sense
{
    public class IosInput : IEvent
    {

        public static char XRControllerEventValue = '!';
        public static char XRControllerEventValueForButtonA = '!';
        public static char XRControllerEventValueForButtonB = '!';
        public static char XRControllerEventValueForButtonC = '!';
        public static char XRControllerEventValueForButtonD = '!';
        public static char XRControllerEventValueForButtonL = '!';
        public static char XRControllerEventValueForButtonU = '!';


        public static string JoystickVerticalAxisEventValue = "JOYSTICK_CENTER";
        public static string JoystickHorizontalAxisEventValue = "JOYSTICK_CENTER";


        // utility flags to enable button down event for one frame and disable it in the next frame

        public static bool isXRControllerButtonUpForButtonA = false, isXRControllerButtonUpForButtonAUtility = false,
            isXRControllerButtonDownForButtonA = false, isXRControllerButtonDownForButtonAUtility = false;

        public static bool isXRControllerButtonUpForButtonB = false, isXRControllerButtonUpForButtonBUtility = false,
           isXRControllerButtonDownForButtonB = false, isXRControllerButtonDownForButtonBUtility = false;

        public static bool isXRControllerButtonUpForButtonC = false, isXRControllerButtonUpForButtonCUtility = false,
           isXRControllerButtonDownForButtonC = false, isXRControllerButtonDownForButtonCUtility = false;

        public static bool isXRControllerButtonUpForButtonD = false, isXRControllerButtonUpForButtonDUtility = false,
           isXRControllerButtonDownForButtonD = false, isXRControllerButtonDownForButtonDUtility = false;

        public static bool isXRControllerButtonUpForButtonL = false, isXRControllerButtonUpForButtonLUtility = false,
           isXRControllerButtonDownForButtonL = false, isXRControllerButtonDownForButtonLUtility = false;

        public static bool isXRControllerButtonUpForButtonU = false, isXRControllerButtonUpForButtonUUtility = false,
           isXRControllerButtonDownForButtonU = false, isXRControllerButtonDownForButtonUUtility = false;


        // Ievent's functions implementation start

        // is button pressed
        public bool GetButton(ButtonName buttonName)
        {
            switch (buttonName)
            {
                case ButtonName.A:
                    return XRControllerEventValueForButtonA == 'u';
                case ButtonName.B:
                    return XRControllerEventValueForButtonB == 'h';
                case ButtonName.C:
                    return XRControllerEventValueForButtonC == 'y';
                case ButtonName.D:
                    return XRControllerEventValueForButtonD == 'j';
                case ButtonName.L:
                    return XRControllerEventValueForButtonL == 'o';
                case ButtonName.U:
                    return XRControllerEventValueForButtonU == 'l';

                default: return false;
            }
        }

        // is button down
        public bool GetButtonDown(ButtonName buttonName)
        {
            switch (buttonName)
            {
                case ButtonName.A:
                    return isXRControllerButtonDownForButtonA;
                case ButtonName.B:
                    return isXRControllerButtonDownForButtonB;
                case ButtonName.C:
                    return isXRControllerButtonDownForButtonC;
                case ButtonName.D:
                    return isXRControllerButtonDownForButtonD;
                case ButtonName.L:
                    return isXRControllerButtonDownForButtonL;
                case ButtonName.U:
                    return isXRControllerButtonDownForButtonU;

                default: return false;
            }

        }

        // is button up
        public bool GetButtonUp(ButtonName buttonName)
        {

            switch (buttonName)
            {
                case ButtonName.A:
                    return isXRControllerButtonUpForButtonA;
                case ButtonName.B:
                    return isXRControllerButtonUpForButtonB;
                case ButtonName.C:
                    return isXRControllerButtonUpForButtonC;
                case ButtonName.D:
                    return isXRControllerButtonUpForButtonD;
                case ButtonName.L:
                    return isXRControllerButtonUpForButtonL;
                case ButtonName.U:
                    return isXRControllerButtonUpForButtonU;

                default: return false;
            }
        }

        public float GetAxis(JoystickAxis joystickAxis)
        {
            switch (joystickAxis)
            {
                case JoystickAxis.HORIZONTAL: return HorizontalAxis();
                case JoystickAxis.VERTICAL: return VerticalAxis();

            }
            throw new XrException(string.Format("Please specify Joystick axis"));
        }

        public void Update()
        {

        }

        // Ievent's functions implementation end

        private float HorizontalAxis()
        {
            return SenseButtonUtil.joystickToVal(JoystickHorizontalAxisEventValue);
        }

        private float VerticalAxis()
        {
            return SenseButtonUtil.joystickToVal(JoystickVerticalAxisEventValue);
        }

    }
}
