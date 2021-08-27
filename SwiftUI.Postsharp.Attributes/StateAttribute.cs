using System;
using PostSharp.Extensibility;

namespace SwiftUI.Postsharp.Attributes
{
    /// <summary>
    /// The code <c>Console.WriteLine("Hello, world!");</c> is injected at the beginning of each target method.
    /// </summary>

    // Only methods are targets of this attribute. If this attribute is applied to a class or assembly, it means
    // that it targets all methods in that class or assembly:
    [MulticastAttributeUsage(MulticastTargets.Property)]

    // When PostSharp runs on an assembly, if at least one if its declarations is affected by the StateAttribute,
    // then PostSharp will search for an assembly named PostSharp.Community.HelloWorld.Weaver.dll and look for 
    // an exported task named HelloWorldTask in that assembly, and that task will be executed during weaving:
    [RequirePostSharp("SwiftUI.Postsharp.Weaver", "WeaveStatesTask")]
    public class StateAttribute : MulticastAttribute
    {
    }
}
