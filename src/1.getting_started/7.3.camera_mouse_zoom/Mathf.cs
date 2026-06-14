namespace LearnSilkNET;

public struct Mathf
{
    public static float PI => (float)Math.PI;

    public static float DegToRad => (PI * 2.0f) / 360.0f;

    public static float Radians(float degress)
    {
        return DegToRad * degress;
    }
}
