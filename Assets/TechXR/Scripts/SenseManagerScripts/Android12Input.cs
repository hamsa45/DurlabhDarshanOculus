using System;
using System.Collections.Generic;
using co.techxr.unity.exceptions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TechXR.Core.Sense
{
    public class Android12Input : IEvent
    {
        AndroidJavaObject _unityPluginInstance;
        private EventHistory[] eventHistories = new EventHistory[6];
        private const long MIN_THRESHOLD = 15;
        private const long MAX_THRESHOLD = 50;
        private static long THRESHOLD = 40;
        private float sensitivity = 0;

        private string lastHorizontalInput;
        private string lastVerticalInput;
        public string[] events;
        static int i = 0;
        Dictionary<ButtonName, Char> buttonMap = new Dictionary<ButtonName, char>();
        Dictionary<Char, KeyCode> keyMap = new Dictionary<char, KeyCode>();

        public Android12Input()
        {
            InitializePlugin();
            //  Initialize button event history
            for (int i = 0; i < Enum.GetNames(typeof(ButtonName)).Length; i++)
            {
                eventHistories[i] = new EventHistory(i);
            }
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
        public float setSensitivity(float sensitivity)
        {
            if (sensitivity >= 0 && sensitivity <= 1)
            {
                this.sensitivity = sensitivity;
            }
            else
            {
                Debug.Log("Sensitivty for Android 12 should be between (0, 1). Found " + sensitivity);
            }
            THRESHOLD = MIN_THRESHOLD + (long)(this.sensitivity * (MAX_THRESHOLD - MIN_THRESHOLD));
            Debug.Log("Setting Threshold: " + THRESHOLD);
            return this.sensitivity;
        }
        public void Update()
        {
            //Debug.Log("REACHDED IN UPDATE OF ANDROID12");
            string[] latestEvents = Events();
            events = latestEvents;

            if (latestEvents.Length > 0)
                Debug.Log(string.Format("AAR Events Count({0}) {1}", latestEvents.Length, printEvents(latestEvents)));

            if (latestEvents != null && latestEvents.Length > 0)
            {
                HandleEvents(latestEvents);
            }
        }
        string printEvents(string[] printEvents)
        {
            string str = "";
            for (int i = 0; i < printEvents.Length; i++)
            {
                str = string.Format("{0} {1}", str, printEvents[i]);
            }
            return str;
        }

        public ButtonName? GetButtonNameFromAarEvent(string aarEvent)
        {
            if (aarEvent == "A_UP" || aarEvent == "A_DOWN") return ButtonName.A;
            if (aarEvent == "B_UP" || aarEvent == "B_DOWN") return ButtonName.B;
            if (aarEvent == "C_UP" || aarEvent == "C_DOWN") return ButtonName.C;
            if (aarEvent == "D_UP" || aarEvent == "D_DOWN") return ButtonName.D;
            if (aarEvent == "L_UP" || aarEvent == "L_DOWN") return ButtonName.L;
            if (aarEvent == "U_UP" || aarEvent == "U_DOWN") return ButtonName.U;
            if (aarEvent == "U_UP" || aarEvent == "U_DOWN") return ButtonName.U;
            else return null;
        }
        public ButtonState GetButtonStateFromAarEvent(string aarEvent)
        {
            if (aarEvent.EndsWith("_UP")) return ButtonState.UP;
            else if (aarEvent.EndsWith("_DOWN")) return ButtonState.DOWN;
            else if (aarEvent.EndsWith("_PRESSED")) return ButtonState.PRESSED;
            else throw new XrException("Invalid button state from AAR: " + aarEvent);
        }
        public int getIndex(string EventName)
        {
            try
            {
                return (int)GetButtonNameFromAarEvent(EventName);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public void HandleEvents(string[] eventsList)
        {
            foreach (string eventLog in eventsList)
            {
                int EventIndex = getIndex(eventLog);
                //  Add event to event historyQueue
                if (EventIndex >= 0)
                {
                    Debug.Log(string.Format("Handle {0}\t Existing {1}", eventLog, eventHistories[EventIndex]));
                    eventHistories[EventIndex].AddNewEvent(eventLog);
                    Debug.Log(string.Format("\t\t After addition {0}", eventHistories[EventIndex]));
                }
            }
        }

        public class EventHistory
        {
            ButtonName buttonName;  //  A, B, ...
            EventOrigin eventOrigin;
            List<TimedEvent> queue = new List<TimedEvent>();
            public int clockTicks = 0;

            private string temp;

            public EventHistory(int eventType)
            {
                this.buttonName = (ButtonName)eventType;
            }
            internal void AddNewEvent(string eventLog)
            {
                if (eventLog.Contains("UP")) queue.Insert(0, new TimedEvent(ButtonState.UP));
                else if (eventLog.Contains("DOWN")) queue.Insert(0, new TimedEvent(ButtonState.DOWN));
                else throw new XrException(string.Format("Invalid Event. Expected Button UP or DOWN. Received {0}", eventLog));

                //  Maintain a queue length of 2
                if (queue.Count > 2)
                {
                    for (int i = 2; i < queue.Count; i++) queue.RemoveAt(i);
                }

                //  Reset clockTicks
                //clockTicks = 0;
            }
            public override string ToString()
            {
                TimedEvent latest = getLatestEvent();
                TimedEvent secondLatest = getSecondLatest();
                string str = string.Format("Queue for {0} - {1} {2}", buttonName,
                    latest != null ? latest.printableString(-1) : "null",
                    secondLatest != null ? secondLatest.printableString(latest.timestamp) : "null2");
                return str;
            }
            public TimedEvent getLatestEvent()
            {
                if (queue.Count > 0)
                {
                    return queue[0];
                }
                else return null;
            }
            public TimedEvent getSecondLatest()
            {
                if (queue.Count > 1)
                {
                    return queue[1];
                }
                else return null;
            }
            internal ButtonState? readButtonEvent()
            {

                return null;
            }
            //  Check the queue and return true if matched
            public bool isEvent(ButtonState buttonState)
            {
                TimedEvent latest = getLatestEvent();
                TimedEvent secondLatest = getSecondLatest();

                if (i % 100 == 0 || (latest != null && !latest.isExpired())) Debug.Log(string.Format("Android_12 Checking {0}\t State {1} {2}\t<-\t {3} {4}", string.Format("{0}_{1}", buttonName, buttonState),
                      latest?.getButtonState(), !latest?.isExpired(), secondLatest?.getButtonState(), !secondLatest?.isExpired()));

                if (buttonName == ButtonName.A)
                {
                }
                //  Check the states of previous events and return state intelligently
                if (latest == null && secondLatest == null) return false;
                if (latest != null && secondLatest == null)
                {
                    switch (buttonState)
                    {
                        case ButtonState.DOWN:
                            {
                                if (latest.getButtonState() == ButtonState.DOWN && !latest.isExpired())
                                    return true;
                                else return false;
                            }
                        case ButtonState.PRESSED:
                            {
                                if (latest.getButtonState() == ButtonState.DOWN && latest.isExpired())
                                    return true;
                                else return false;
                            }
                        case ButtonState.UP: return false;   //  UP can be triggered only if there is a past DOWN event
                    }
                }
                else if (latest != null && secondLatest != null)
                {
                    switch (buttonState)
                    {
                        case ButtonState.DOWN:
                            {
                                if (secondLatest.getButtonState() == ButtonState.UP &&
                                    (latest.getButtonState() == ButtonState.DOWN && !latest.isExpired()))
                                    return true;
                                else return false;
                            }
                        case ButtonState.PRESSED:
                            {
                                if (latest.getButtonState() == ButtonState.DOWN &&
                                    (secondLatest.getButtonState() == ButtonState.DOWN || latest.isExpired()))
                                    return true;
                                else return false;
                            }
                        case ButtonState.UP:
                            {
                                //  UP can be triggered only if there is a past DOWN event
                                if (secondLatest.getButtonState() == ButtonState.DOWN &&
                                    (latest.getButtonState() == ButtonState.UP && !latest.isExpired()))
                                    return true;
                                else return false;
                            }
                    }
                }
                else
                {
                    throw new XrException(string.Format("Inconsistent State!"));
                }
                return false;
            }
        }
        public class TimedEvent
        {
            private ButtonState buttonState;
            public long timestamp;
            public int readCount = 0;

            public TimedEvent(ButtonState state)
            {
                this.buttonState = state;
                timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            }
            public bool isExpired()
            {
                long currentInstant = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                return (currentInstant - timestamp > THRESHOLD);
            }
            public ButtonState? getButtonState()
            {
                return buttonState;
            }
            public string printableString(long lastTimestamp)
            {
                if (lastTimestamp <= 0) return string.Format("{0} [ ]", buttonState);
                else return string.Format("{0} [{1}]", buttonState, lastTimestamp - timestamp);
            }
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
        // --------------
        private EventHistory GetEventHistory(string senseButton)
        {
            ButtonName buttonName = SenseButtonUtil.getButtonNameFromSenseEvent(senseButton);
            return eventHistories[(int)buttonName];
        }

    }
}