using UnityEngine;

public class UIFlexibleBackground : MonoBehaviour
{
    private const int baseWidth = 60;

    public void ResizeToTrait(Trait trait, bool active)
    {
        int width = 70;
        if (active)
        {
            int length = trait.TraitBreakpoints.Length;
            width = baseWidth + length * 30;
        }

        (transform as RectTransform).SetRight(206 - width);
    }
}

public static class RectTransformExtensions
{
    public static void SetLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }
}
