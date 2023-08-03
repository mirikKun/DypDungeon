public static class RectHelper
{
    public static float GetCenterOfCovering(int firstStart, int firstEnd, int secondStart, int secondEnd)
    {
        
        float center;
        if (firstStart < secondStart)
        {
            center = secondStart + GetLenghtOfCovering(firstStart, firstEnd, secondStart, secondEnd) / 2f;
        }
        else
        {
            center = firstStart + GetLenghtOfCovering(firstStart, firstEnd, secondStart, secondEnd) / 2f;
        }

        return center;
    }

    public static int GetLenghtOfCovering(int firstStart, int firstEnd, int secondStart, int secondEnd)
    {
        int lenght;
        if (firstStart < secondStart)
        {
            if (firstEnd < secondEnd)
            {
                lenght = firstEnd - secondStart;
            }
            else
            {
                lenght = secondEnd - secondStart;
            }
        }
        else
        {
            if (firstEnd < secondEnd)
            {
                lenght = firstEnd - firstStart;
            }
            else
            {
                lenght = secondEnd - firstStart;
            }
        }

        return lenght;
    }
}