using UnityEngine;

public class UseHexColor : MonoBehaviour
{
    public static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}
