using System;

namespace Project.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class VictoryAttribute : Attribute
    {
    }
}