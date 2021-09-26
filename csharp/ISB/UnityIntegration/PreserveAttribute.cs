using System;

namespace ISB.UnityIntegration
{
    // For preserving code from Unity's managed code stripping.
    // See https://docs.unity3d.com/Manual/ManagedCodeStripping.html
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Field |
        AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Interface |
        AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Struct |
        AttributeTargets.Assembly | AttributeTargets.Enum, Inherited = false)]
    public class PreserveAttribute : Attribute
    {
    }
}
