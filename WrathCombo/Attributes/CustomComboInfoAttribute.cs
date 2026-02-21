using System;
using System.Runtime.CompilerServices;
using WrathCombo.Extensions;
using static WrathCombo.Window.Text;
using ECommonsJob = ECommons.ExcelServices.Job;

namespace WrathCombo.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal class CustomComboInfoAttribute : Attribute
{
    /// <summary> Initializes a new instance of the <see cref="CustomComboInfoAttribute"/> class. </summary>
    /// <param name="job"> Associated job ID. </param>
    /// <param name="preset"> Preset enum value for localization keys. </param>
    /// <param name="order"> Display order. </param>
    internal CustomComboInfoAttribute(
        ECommonsJob job,
        Preset preset,
        [CallerLineNumber] int order = 0)
    {
        _preset = preset;

        Job = job switch
        {
            ECommonsJob.BTN or ECommonsJob.MIN or ECommonsJob.FSH => ECommonsJob.MIN,
            _ => job
        };

        Order = order;
    }

    // Preset enum value for localization keys
    private readonly Preset _preset;

    /// <summary> Gets the display name from resources. </summary>
    public string Name => GetPresetString($"{_preset}_Name");

    /// <summary> Gets the tooltip/description from resources. </summary>
    public string Description => GetPresetString($"{_preset}_Desc");

    /// <summary> Associated job ID (with gathering jobs mapped to MIN). </summary>
    public ECommonsJob Job { get; }

    /// <summary> Display order (auto-filled via CallerLineNumber). </summary>
    public int Order { get; }

    /// <summary> Gets the job role. </summary>
    public JobRole Role => RoleAttribute.GetRoleFromJob(Job);

    /// <summary> Gets the job name. </summary>
    public string JobName => Job.Name();

    /// <summary> Gets the job shorthand. </summary>
    public string JobShorthand => Job.Shorthand();
}
