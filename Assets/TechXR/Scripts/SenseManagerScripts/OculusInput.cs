using System.Collections.Generic;
using co.techxr.unity.exceptions;
using UnityEngine;

namespace TechXR.Core.Sense
{
    public class OculusInput : IEvent
    {
        private Dictionary<ButtonName, OculusButton> techxrToOculus = new Dictionary<ButtonName, OculusButton>();
        private Dictionary<OculusButton, KeyCode> oculusToKeycode = new Dictionary<OculusButton, KeyCode>();

        public OculusInput()
        {
            InitialiseDict();
        }

        private void InitialiseDict()
        {
            // Oculus Fixed Mapping with Keycode
            oculusToKeycode[OculusButton.A] = KeyCode.JoystickButton0;
            oculusToKeycode[OculusButton.B] = KeyCode.JoystickButton1;
            oculusToKeycode[OculusButton.X] = KeyCode.JoystickButton2;
            oculusToKeycode[OculusButton.Y] = KeyCode.JoystickButton3;
            oculusToKeycode[OculusButton.LeftTrigger] = KeyCode.JoystickButton14;
            oculusToKeycode[OculusButton.LeftSide] = KeyCode.JoystickButton4;
            oculusToKeycode[OculusButton.RightTrigger] = KeyCode.JoystickButton15;
            oculusToKeycode[OculusButton.RightSide] = KeyCode.JoystickButton5;

            // TechXR to Oculus
            techxrToOculus[ButtonName.A] = OculusButton.A;
            techxrToOculus[ButtonName.B] = OculusButton.B;
            techxrToOculus[ButtonName.C] = OculusButton.X;
            techxrToOculus[ButtonName.D] = OculusButton.Y;
            techxrToOculus[ButtonName.L] = OculusButton.RightTrigger;
            techxrToOculus[ButtonName.U] = OculusButton.RightSide;
        }

        public void MapOculusKeys(ButtonName techxrButton, OculusButton oculusButton)
        {
            techxrToOculus[techxrButton] = oculusButton;
        }

        public bool GetButton(ButtonName buttonName)
        {
            return Input.GetKey(oculusToKeycode[techxrToOculus[buttonName]]);
        }

        public bool GetButtonDown(ButtonName buttonName)
        {
            return Input.GetKeyDown(oculusToKeycode[techxrToOculus[buttonName]]);
        }

        public bool GetButtonUp(ButtonName buttonName)
        {
            return Input.GetKeyUp(oculusToKeycode[techxrToOculus[buttonName]]);
        }

        public void Update()
        {

        }

        public float GetAxis(JoystickAxis joystickAxis)
        {
            //throw new System.NotImplementedException();
            //
            return 0;
        }
    }
}