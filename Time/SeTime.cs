using System;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Action = System.Action;

namespace CriticalCommonLib.Time;

public class SeTime : ISeTime
{
    private static TimeStamp GetServerTime()
        => new(Framework.GetServerTime() * 1000);

    public TimeStamp ServerTime         { get; private set; }
    public TimeStamp EorzeaTime         { get; private set; }
    public long      EorzeaTotalMinute  { get; private set; }
    public long      EorzeaTotalHour    { get; private set; }
    public short     EorzeaMinuteOfDay  { get; private set; }
    public byte      EorzeaHourOfDay    { get; private set; }
    public byte      EorzeaMinuteOfHour { get; private set; }

    public event Action? Updated;
    public event Action? HourChanged;
    public event Action? WeatherChanged;

    public SeTime()
    {
        Update(null!);
        Service.Framework.Update += Update;
    }

    public void Dispose()
        => Service.Framework.Update -= Update;

    private unsafe TimeStamp GetEorzeaTime()
    {
        var framework = Framework.Instance();
        if (framework == null)
            return ServerTime.ConvertToEorzea();

        return Math.Abs(new TimeStamp(framework->ServerTime * 1000) - ServerTime) < 5000
            ? new TimeStamp(framework->EorzeaTime * 1000)
            : ServerTime.ConvertToEorzea();
    }

    private void Update(global::Dalamud.Game.Framework _)
    {
        ServerTime = GetServerTime();
        EorzeaTime = GetEorzeaTime();
        var minute = EorzeaTime.TotalMinutes;
        if (minute != EorzeaTotalMinute)
        {
            EorzeaTotalMinute  = minute;
            EorzeaMinuteOfDay  = (short)(EorzeaTotalMinute % RealTime.MinutesPerDay);
            EorzeaMinuteOfHour = (byte)(EorzeaMinuteOfDay % RealTime.MinutesPerHour);
        }

        var hour = EorzeaTotalMinute / RealTime.MinutesPerHour;
        if (hour != EorzeaTotalHour)
        {
            EorzeaTotalHour = hour;
            EorzeaHourOfDay = (byte)(EorzeaMinuteOfDay / RealTime.MinutesPerHour);
            HourChanged?.Invoke();
        }

        Updated?.Invoke();
    }
}