using System.ComponentModel;

namespace OHTPmSimulatorV5;

public sealed class PmStep : INotifyPropertyChanged
{
    private StepStatus status = StepStatus.Pending;
    public int Index { get; set; }
    public string StepName { get; set; }
    public string Sensor { get; set; }
    public string Condition { get; set; }
    public string Guidance { get; set; }
    public string Note { get; set; } = "";

    public StepStatus Status
    {
        get => status;
        set
        {
            status = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayStatus)));
        }
    }

    public string DisplayStatus => Status switch
    {
        StepStatus.Pending => "Pending",
        StepStatus.InProgress => "In Progress",
        StepStatus.Passed => "Passed",
        StepStatus.AdjustmentRequired => "Adjustment Required",
        StepStatus.RetestPending => "Retest Pending",
        _ => "Pending"
    };

    public PmStep(string stepName, string sensor, string condition, string guidance)
    {
        StepName = stepName;
        Sensor = sensor;
        Condition = condition;
        Guidance = guidance;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public enum StepStatus
{
    Pending,
    InProgress,
    Passed,
    AdjustmentRequired,
    RetestPending
}
