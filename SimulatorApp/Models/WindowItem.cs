using System;

namespace OHTPmSimulatorV5;

public sealed class WindowItem
{
    public IntPtr Handle { get; }
    public string Title { get; }

    public WindowItem(IntPtr handle, string title)
    {
        Handle = handle;
        Title = title;
    }

    public override string ToString() => Title;
}
