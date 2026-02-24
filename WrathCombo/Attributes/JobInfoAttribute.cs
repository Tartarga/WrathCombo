using ECommons.ExcelServices;
using System;
using System.Runtime.CompilerServices;
using WrathCombo.Extensions;
using static WrathCombo.CustomComboNS.Functions.Jobs;

namespace WrathCombo.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal class JobInfoAttribute : Attribute
{
    /// <summary> Initializes a new instance of the <see cref="JobInfoAttribute"/> class. </summary>
    /// <param name="job"> Associated job ID. </param>
    /// <param name="order"> Display order. </param>
    ///
    internal JobInfoAttribute(
        Job job,
        JobRole jobRoleIcon = JobRole.All,
        [CallerLineNumber] int order = 0)
    {
        Job = job switch
        {
            Job.BTN or Job.MIN or Job.FSH => Job.MIN,
            _ => job
        };

        Name = "";
        Description = "";

        Order = order;
        Role = GetRoleFromJob(Job);
        RoleForIcon = jobRoleIcon == JobRole.All ? null : jobRoleIcon;
        JobName = Job.Name();
        JobShorthand = Job.Shorthand();
    }

    /// <summary> Gets the display name. </summary>
    public string Name { get; set; }

    /// <summary> Gets the description. </summary>
    public string Description { get; set; }

    /// <summary> Associated job ID (with gathering jobs mapped to MIN). </summary>
    public Job Job { get; }

    /// <summary> Display order (auto-filled via CallerLineNumber). </summary>
    public int Order { get; }

    /// <summary> Gets the job role. </summary>
    public JobRole Role { get; }

    /// <summary> Gets the job role used for adding an icon override. </summary>
    public JobRole? RoleForIcon { get; }

    /// <summary> Gets the job name. </summary>
    public string JobName { get; }

    /// <summary> Gets the job shorthand. </summary>
    public string JobShorthand { get; }
}
