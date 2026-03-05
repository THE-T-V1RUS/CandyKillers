using UnityEngine;

public static class EasingLibrary
{
    const float c1 = 1.70158f;
    const float c2 = 2.5949095f;
    const float c3 = 2.70158f;
    const float c4 = (2 * Mathf.PI) / 3;
    const float c5 = (2 * Mathf.PI) / 4.5f;
    const float n1 = 7.5625f;
    const float d1 = 2.75f;

    public enum EasingType
    {
        easeInSine,
        easeOutSine,
        easeInOutSine,
        easeInQuad,
        easeOutQuad,
        easeInOutQuad,
        easeInCubic,
        easeOutCubic,
        easeInOutCubic,
        easeInQuart,
        easeOutQuart,
        easeInOutQuart,
        easeInQuint,
        easeOutQuint,
        easeInOutQuint,
        easeInExpo,
        easeOutExpo,
        easeInOutExpo,
        easeInCirc,
        easeOutCirc,
        easeInOutCirc,
        easeInBack,
        easeOutBack,
        easeInOutBack,
        easeInElastic,
        easeOutElastic,
        easeInOutElastic,
        easeInBounce,
        easeOutBounce,
        easeInOutBounce,
    }

    public static float DoEase(EasingType type, float input)
    {
        switch (type)
        {
            case EasingType.easeInSine: input = easeInSine(input); break;

            case EasingType.easeOutSine: input = easeOutSine(input); break;

            case EasingType.easeInOutSine: input = easeInOutSine(input); break;

            case EasingType.easeInQuad: input = easeInQuad(input); break;

            case EasingType.easeOutQuad: input = easeOutQuad(input); break;

            case EasingType.easeInOutQuad: input = easeInOutQuad(input); break;

            case EasingType.easeInCubic: input = easeInCubic(input); break;

            case EasingType.easeOutCubic: input = easeOutCubic(input); break;

            case EasingType.easeInOutCubic: input = easeInOutCubic(input); break;

            case EasingType.easeInQuart: input = easeInQuart(input); break;

            case EasingType.easeOutQuart: input = easeOutQuart(input); break;

            case EasingType.easeInOutQuart: input = easeInOutQuart(input); break;

            case EasingType.easeInQuint: input = easeInQuint(input); break;

            case EasingType.easeOutQuint: input = easeOutQuint(input); break;

            case EasingType.easeInOutQuint: input = easeInOutQuint(input); break;

            case EasingType.easeInExpo: input = easeInExpo(input); break;

            case EasingType.easeOutExpo: input = easeOutExpo(input); break;

            case EasingType.easeInOutExpo: input = easeInOutExpo(input); break;

            case EasingType.easeInCirc: input = easeInCirc(input); break;

            case EasingType.easeOutCirc: input = easeOutCirc(input); break;

            case EasingType.easeInOutCirc: input = easeInOutCirc(input); break;

            case EasingType.easeInBack: input = easeInBack(input); break;

            case EasingType.easeOutBack: input = easeOutBack(input); break;

            case EasingType.easeInOutBack: input = easeInOutBack(input); break;

            case EasingType.easeInElastic: input = easeInElastic(input); break;

            case EasingType.easeOutElastic: input = easeOutElastic(input); break;

            case EasingType.easeInOutElastic: input = easeInOutElastic(input); break;

            case EasingType.easeInBounce: input = easeInBounce(input); break;

            case EasingType.easeOutBounce: input = easeOutBounce(input); break;

            case EasingType.easeInOutBounce: input = easeInOutBounce(input); break;
        }

        return input;
    }

    static float easeInSine(float x)
    {
        return 1 - Mathf.Cos((x * Mathf.PI) / 2);
    }

    static float easeOutSine(float x)
    {
        return Mathf.Sin((x * Mathf.PI) / 2);
    }

    static float easeInOutSine(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
    }

    static float easeInQuad(float x)
    {
        return x * x;
    }

    static float easeOutQuad(float x)
    {
        return 1 - (1 - x) * (1 - x);
    }

    static float easeInOutQuad(float x)
    {
        return x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;
    }

    static float easeInCubic(float x)
    {
        return x * x * x;
    }

    static float easeOutCubic(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3);
    }

    static float easeInOutCubic(float x)
    {
        return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
    }

    static float easeInQuart(float x)
    {
        return x * x * x * x;
    }

    static float easeOutQuart(float x)
    {
        return 1 - Mathf.Pow(1 - x, 4);
    }

    static float easeInOutQuart(float x)
    {
        return x < 0.5 ? 8 * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 4) / 2;
    }

    static float easeInQuint(float x)
    {
        return x * x * x * x * x;
    }

    static float easeOutQuint(float x)
    {
        return 1 - Mathf.Pow(1 - x, 5);
    }

    static float easeInOutQuint(float x)
    {
        return x < 0.5 ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }

    static float easeInExpo(float x)
    {
        return x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);
    }

    static float easeOutExpo(float x)
    {
        return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
    }

    static float easeInOutExpo(float x)
    {
        return x == 0
            ? 0
            : x == 1
            ? 1
            : x < 0.5 ? Mathf.Pow(2, 20 * x - 10) / 2
            : (2 - Mathf.Pow(2, -20 * x + 10)) / 2;
    }

    static float easeInCirc(float x)
    {
        return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));
    }

    static float easeOutCirc(float x)
    {
        return Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
    }

    static float easeInOutCirc(float x)
    {
        return x < 0.5
            ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2
            : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2;
    }

    static float easeInBack(float x)
    {
        return c3 * x * x * x - c1 * x * x;
    }

    static float easeOutBack(float x)
    {
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    static float easeInOutBack(float x)
    {
        return x < 0.5
            ? (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
            : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
    }

    static float easeInElastic(float x)
    {
        return x == 0
            ? 0
            : x == 1
            ? 1
            : -Mathf.Pow(2, 10 * x - 10) * Mathf.Sin((x * 10 - 10.75f) * c4);
    }

    static float easeOutElastic(float x)
    {
        return x == 0
            ? 0
            : x == 1
            ? 1
            : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;
    }

    static float easeInOutElastic(float x)
    {
        return x == 0
            ? 0
            : x == 1
            ? 1
            : x < 0.5
            ? -(Mathf.Pow(2, 20 * x - 10) * Mathf.Sin((20 * x - 11.125f) * c5)) / 2
            : (Mathf.Pow(2, -20 * x + 10) * Mathf.Sin((20 * x - 11.125f) * c5)) / 2 + 1;
    }

    static float easeInBounce(float x)
    {
        return 1 - easeOutBounce(1 - x);
    }

    static float easeOutBounce(float x)
    {
        if (x < 1 / d1)
        {
            return n1 * x * x;
        }
        else if (x < 2 / d1)
        {
            return n1 * (x -= 1.5f / d1) * x + 0.75f;
        }
        else if (x < 2.5f / d1)
        {
            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        }
        else
        {
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }

    static float easeInOutBounce(float x)
    {
        return x < 0.5
            ? (1 - easeOutBounce(1 - 2 * x)) / 2
            : (1 + easeOutBounce(2 * x - 1)) / 2;
    }
}