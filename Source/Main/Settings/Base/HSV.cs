using System;
using UnityEngine;

namespace DFerrisIgnorance;

internal class HSV
{
    public static Color ToRGBA(float H, float S, float V, float A = 1f)
    {
        if (S == 0f)
        {
            return new Color(V, V, V, A);
        }

        if (V == 0f)
        {
            return new Color(0f, 0f, 0f, A);
        }

        Color black = Color.black;
        float num = H * 6f;
        int num2 = Mathf.FloorToInt(num);
        float num3 = num - (float)num2;
        float num4 = V * (1f - S);
        float num5 = V * (1f - S * num3);
        float num6 = V * (1f - S * (1f - num3));
        switch (num2 + 1)
        {
            case 0:
                black.r = V;
                black.g = num4;
                black.b = num5;
                break;
            case 1:
                black.r = V;
                black.g = num6;
                black.b = num4;
                break;
            case 2:
                black.r = num5;
                black.g = V;
                black.b = num4;
                break;
            case 3:
                black.r = num4;
                black.g = V;
                black.b = num6;
                break;
            case 4:
                black.r = num4;
                black.g = num5;
                black.b = V;
                break;
            case 5:
                black.r = num6;
                black.g = num4;
                black.b = V;
                break;
            case 6:
                black.r = V;
                black.g = num4;
                black.b = num5;
                break;
            case 7:
                black.r = V;
                black.g = num6;
                black.b = num4;
                break;
        }

        black.r = Mathf.Clamp(black.r, 0f, 1f);
        black.g = Mathf.Clamp(black.g, 0f, 1f);
        black.b = Mathf.Clamp(black.b, 0f, 1f);
        black.a = Mathf.Clamp(A, 0f, 1f);
        return black;
    }

    public static void ToHSV(Color rgbColor, out float H, out float S, out float V)
    {
        if (rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
        {
            HSV.RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
            return;
        }

        if (rgbColor.g > rgbColor.r)
        {
            HSV.RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
            return;
        }

        HSV.RGBToHSVHelper(0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
    }
    
    private static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
    {
        V = dominantcolor;
        if (V != 0f)
        {
            float num;
            if (colorone > colortwo)
            {
                num = colortwo;
            }
            else
            {
                num = colorone;
            }

            float num2 = V - num;
            if (num2 != 0f)
            {
                S = num2 / V;
                H = offset + (colorone - colortwo) / num2;
            }
            else
            {
                S = 0f;
                H = offset + (colorone - colortwo);
            }

            H /= 6f;
            if (H < 0f)
            {
                H += 1f;
                return;
            }
        }
        else
        {
            S = 0f;
            H = 0f;
        }
    }
}
