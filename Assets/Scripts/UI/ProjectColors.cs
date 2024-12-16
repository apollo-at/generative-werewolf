using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectColors
{
    public static string OrangeHex = "#F79F1F";
    public static Color Orange = ColorUtility.TryParseHtmlString(OrangeHex, out Color unityColor) ? unityColor : Color.clear;
    public static string RedHex = "#9B151A";
    public static Color Red = ColorUtility.TryParseHtmlString(RedHex, out Color unityColor) ? unityColor : Color.clear;
    public static string BlueHex = "#1B63BE";
    public static Color Blue = ColorUtility.TryParseHtmlString(BlueHex, out Color unityColor) ? unityColor : Color.clear;
    public static string VioletHex = "#1B1464";
    public static Color Violet = ColorUtility.TryParseHtmlString(VioletHex, out Color unityColor) ? unityColor : Color.clear;
    public static string LilaHex = "#9C9DFF";
    public static Color Lila = ColorUtility.TryParseHtmlString(LilaHex, out Color unityColor) ? unityColor : Color.clear;
    public static string GreenHex = "#C4E538";
    public static Color Green = ColorUtility.TryParseHtmlString(GreenHex, out Color unityColor) ? unityColor : Color.clear;
}
