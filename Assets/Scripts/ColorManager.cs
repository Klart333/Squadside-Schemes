using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ColorManager : Singleton<ColorManager>
{
    public List<Colors> colors = new List<Colors>();

    public ColorName getRandomColorName()
    {
        ColorName colorName = colors[Random.Range(0, colors.Count - 1)].colorName;
        return colorName;
    }

    public Color GetColor(ColorName colorName)
    {
        for (int i = 0; i < colors.Count - 1; i++)
        {
            if (colors[i].colorName == colorName)
            {
                return colors[i].color;
            }
        }

        return Color.black;
    }
}

[Serializable]
public struct Colors
{
    public ColorName colorName;
    public Color color;
}

[Serializable]
public enum ColorName
{
    Red,
    Orange,
    Yellow,
    LightGreen,
    Green,
    LightBlue,
    Blue,
    DarkBlue,
    DarkPurple,
    LightPurple,
    Pink,
    DarkPink,
    NoColor
}
