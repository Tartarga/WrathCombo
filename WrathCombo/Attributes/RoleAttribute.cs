using static WrathCombo.CustomComboNS.Functions.Jobs;
using System;

namespace WrathCombo.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal class RoleAttribute : Attribute
{
    public JobRole Role { get; }

    internal RoleAttribute(JobRole role)
    {
        Role = role;
    }
}