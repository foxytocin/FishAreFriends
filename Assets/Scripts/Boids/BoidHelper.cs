using Unity.Mathematics;

public static class BoidHelper
{

    const int numViewDirections = 100;
    public static readonly float3[] directions;

    static BoidHelper()
    {
        directions = new float3[BoidHelper.numViewDirections];

        float goldenRatio = (1 + math.sqrt(5)) / 2;
        float angleIncrement = math.PI * 2 * goldenRatio;

        for (int i = 0; i < numViewDirections; i++)
        {
            float t = (float)i / numViewDirections;
            float inclination = math.acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = math.sin(inclination) * math.cos(azimuth);
            float y = math.sin(inclination) * math.sin(azimuth);
            float z = math.cos(inclination);
            directions[i] = new float3(x, y, z);
        }
    }

}