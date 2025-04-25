namespace TechXR.Core.Sense
{
    public interface IEvent
    {
        void Update();
        //
        float GetAxis(JoystickAxis joystickAxis);
        //float VerticalAxis();

        bool GetButtonDown(ButtonName button);
        bool GetButton(ButtonName button);
        bool GetButtonUp(ButtonName button);
    }
}
