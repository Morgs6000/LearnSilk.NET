namespace LearnSilkNET;

public class Time
{
    public static float ElapsedTime { get; private set; }    
    public static float DeltaTime { get; private set; }

    public static void Update(double deltaTime)
    {
        ElapsedTime += (float)deltaTime;
        DeltaTime = (float)deltaTime;
    }
}
