using System;
using System.Collections.Generic;
using co.techxr.unity.exceptions;
using UnityEngine;

namespace TechXR.Core.Sense
{
    public class Android12KeyPress : IEvent
    {
        AndroidJavaObject _unityPluginInstance;
        Dictionary<ButtonName, Char> buttonMap = new Dictionary<ButtonName, char>();
        Dictionary<Char, KeyCode> keyMap = new Dictionary<char, KeyCode>();
        public string[] events;
        private string lastHorizontalInput;
        private string lastVerticalInput;

        public Android12KeyPress()
        {
            InitializePlugin();
        }

        void InitializePlugin()
        {
            _unityPluginInstance = new AndroidJavaClass("com.example.inputmanager.EventManager");
            InitializeButtonMap();
        }
        private void InitializeButtonMap()
        {
            buttonMap.Clear();
            buttonMap.Add(ButtonName.A, 'a');
            buttonMap.Add(ButtonName.B, 'b');
            buttonMap.Add(ButtonName.C, 'c');
            buttonMap.Add(ButtonName.D, 'd');
            buttonMap.Add(ButtonName.L, 'l');
            buttonMap.Add(ButtonName.U, 'u');
            initializeKeyMap();
        }
        private void initializeKeyMap()
        {
            foreach (ButtonName key in buttonMap.Keys)
            {
                keyMap.Add(buttonMap[key], getUnityKeyCodeFromChar(buttonMap[key]));
            }
        }

        private KeyCode getUnityKeyCodeFromChar(char v)
        {
            switch (v.ToString().ToLower().ToCharArray()[0])
            {
                case 'a': return KeyCode.A;
                case 'b': return KeyCode.B;
                case 'c': return KeyCode.C;
                case 'd': return KeyCode.D;
                case 'e': return KeyCode.E;
                case 'f': return KeyCode.F;
                case 'g': return KeyCode.G;
                case 'h': return KeyCode.H;
                case 'i': return KeyCode.I;
                case 'j': return KeyCode.J;
                case 'k': return KeyCode.K;
                case 'l': return KeyCode.L;
                case 'm': return KeyCode.M;
                case 'n': return KeyCode.N;
                case 'o': return KeyCode.O;
                case 'p': return KeyCode.P;
                case 'q': return KeyCode.Q;
                case 'r': return KeyCode.R;
                case 's': return KeyCode.S;
                case 't': return KeyCode.T;
                case 'u': return KeyCode.U;
                case 'v': return KeyCode.V;
                case 'w': return KeyCode.W;
                case 'x': return KeyCode.X;
                case 'y': return KeyCode.Y;
                case 'z': return KeyCode.Z;
                default: throw new XrException(string.Format("Mapping not supported {0}", v));
            }
        }
        string[] Events()
        {
            if (_unityPluginInstance == null) return null;
            return _unityPluginInstance.CallStatic<string[]>("getEvents");
        }
        public void Update()
        {
            //Debug.Log("REACHDED IN UPDATE OF ANDROID12");
            string[] latestEvents = Events();
            events = latestEvents;

            if (latestEvents?.Length > 0)
                Debug.Log(string.Format("AAR Events Count({0}) {1}", latestEvents.Length, printEvents(latestEvents)));

            if (latestEvents != null && latestEvents.Length > 0)
            {
                //HandleEvents(latestEvents);
            }
        }
        //public void HandleEvents(string[] eventsList)
        //{
        //    foreach (string eventLog in eventsList)
        //    {
        //        int EventIndex = getIndex(eventLog);
        //        //  Add event to event historyQueue
        //        if (EventIndex >= 0)
        //        {
        //            Debug.Log(string.Format("Handle {0}\t Existing {1}", eventLog, eventHistories[EventIndex]));
        //            eventHistories[EventIndex].AddNewEvent(eventLog);
        //            Debug.Log(string.Format("\t\t After addition {0}", eventHistories[EventIndex]));
        //        }
        //    }
        //}

        string printEvents(string[] printEvents)
        {
            string str = "";
            for (int i = 0; i < printEvents.Length; i++)
            {
                str = string.Format("{0} {1}", str, printEvents[i]);
            }
            return str;
        }


        //--------------  IEvent Functions -----------

        public bool GetButton(ButtonName buttonName)
        {
            KeyCode keyCode = keyMap[buttonMap[buttonName]];
            if (Input.GetKey(keyCode))
            {
                Debug.Log(string.Format("Key Pressed Detected: {0}", keyCode));
                return true;
            }
            else return false;
            //return GetEventHistory(button).isEvent(ButtonState.PRESSED);
        }

        public bool GetButtonDown(ButtonName buttonName)
        {
            KeyCode keyCode = keyMap[buttonMap[buttonName]];
            if (Input.GetKeyDown(keyCode))
            {
                Debug.Log(string.Format("Key Down Detected: {0}", keyCode));
                return true;
            }
            else return false;
            //return GetEventHistory(button).isEvent(ButtonState.DOWN);
        }

        public bool GetButtonUp(ButtonName buttonName)
        {
            KeyCode keyCode = keyMap[buttonMap[buttonName]];
            if (Input.GetKeyUp(keyCode))
            {
                Debug.Log(string.Format("Key Up Detected: {0}", keyCode));
                return true;
            }
            else return false;
            //return GetEventHistory(button).isEvent(ButtonState.UP);
        }
        public float GetAxis(JoystickAxis joystickAxis)
        {
            switch (joystickAxis)
            {
                case JoystickAxis.HORIZONTAL: return HorizontalAxis();
                case JoystickAxis.VERTICAL: return VerticalAxis();
            }
            throw new XrException("Please specify joystick axis");
        }
        private float HorizontalAxis()
        {
            if (events != null)
            {
                float HorizontalInput = 0f;
                if (Array.IndexOf(events, "JOYSTICK_CENTER") < 0)
                {
                    if (Array.IndexOf(events, "JOYSTICK_LEFT") >= 0)
                    {
                        HorizontalInput = -1f;
                        lastHorizontalInput = "JOYSTICK_LEFT";
                    }
                    if (Array.IndexOf(events, "JOYSTICK_RIGHT") >= 0)
                    {
                        HorizontalInput = 1f;
                        lastHorizontalInput = "JOYSTICK_RIGHT";
                    }
                }
                else
                    lastHorizontalInput = "JOYSTICK_CENTER";

            }
            return SenseButtonUtil.joystickToVal(lastHorizontalInput);
        }
        private float VerticalAxis()
        {
            if (events != null)
            {
                float VerticalInput = 0f;
                //string[] events = Events();

                if (Array.IndexOf(events, "JOYSTICK_CENTER") < 0)
                {
                    if (Array.IndexOf(events, "JOYSTICK_UP") >= 0)
                    {
                        VerticalInput = 1f;
                        lastVerticalInput = "JOYSTICK_UP";
                    }
                    if (Array.IndexOf(events, "JOYSTICK_DOWN") >= 0)
                    {
                        VerticalInput = -1f;
                        lastVerticalInput = "JOYSTICK_DOWN";

                    }
                }
                else
                    lastVerticalInput = "JOYSTICK_CENTER";

            }
            return SenseButtonUtil.joystickToVal(lastVerticalInput);
        }
    }
}