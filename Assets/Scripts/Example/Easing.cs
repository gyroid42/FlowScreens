using System.Runtime.CompilerServices;

namespace Example
{
    public static class Easing
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseIn(float t) => t * t;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float EaseOut(float t) => 2 * t - t * t;
    }
}