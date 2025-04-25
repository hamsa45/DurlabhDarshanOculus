using System;
using co.techxr.unity.exceptions;

namespace TechXR.Core.Sense
{
    public class SenseButtonUtil
    {
        public static ButtonName getButtonNameFromSenseEvent(string eventName)
        {
            try
            {
                ButtonName buttonName;
                Enum.TryParse<ButtonName>(eventName, true, out buttonName);
                return buttonName;
            }
            catch (Exception e)
            {
                throw new XrException("Expected Button Name (A,B, ...). Found " + eventName);
            }
        }
        public static float joystickToVal(string lastJoystickInput)
        {
            if (lastJoystickInput == "JOYSTICK_UP" || lastJoystickInput == "JOYSTICK_RIGHT")
            {
                return 1f;
            }
            else if (lastJoystickInput == "JOYSTICK_LEFT" || lastJoystickInput == "JOYSTICK_DOWN")
            {
                return -1f;
            }
            else if (lastJoystickInput == "JOYSTICK_CENTER")
            {
                return 0f;
            }
            return 0f;
        }
    }
}