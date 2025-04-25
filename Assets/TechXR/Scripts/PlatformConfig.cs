using System;
using UnityEngine;
namespace TechXR.Core.Sense
{
    public class PlatformConfig
    {
        public static string AUTO_DETECT = "Auto detect";
        public static string ANDROID_12 = "Android 12";
        public static string ANDROID_DEFAULT = "Android default";
        public static string IOS_DEFAULT = "Ios default";
        public static string OCULUS = "Oculus";
        public static string DEFAULT = "Unity default";

        public static string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                SenseInput.SetConfigObject(value);
            }
        }
        private static string name;
    }
}