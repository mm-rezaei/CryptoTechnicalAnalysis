
namespace TechnicalAnalysisTools.Shared.Enumerations
{
    public enum CommandTypes : int
    {
        SessionKey = -2,
        ImAlive = -1,
        Authenticate = 0,
        SuccessfulAuthenticate,
        ClientInitializedData,
        MenuItemChanged,
        ServerStatusPropertyChanged,
        StartTechnicalAnalysis,
        StopTechnicalAnalysis,
        MarketData,
        Alarms,
        AlarmsHistory,
        NewAlarm,
        EditAlarm,
        RunAlarms,
        RunAlarmsResponse,
        RunTemplateAlarm,
        RunTemplateAlarmResponse,
        ReadAlarmScript,
        ReadAlarmScriptResponse,
        EvaluateAlarm,
        EvaluateAlarmResponse,
        SeenAlarm,
        SeenAlarmResponse,
        SeenAllAlarm,
        EnableDisableAlarm,
        EnableDisableAlarmResponse,
        DeleteAlarm,
        DeleteAlarmResponse,
        LiveHistory,
        LiveHistoryResponse,
        TestNewStrategy,
        TestNewStrategyResponse,
        TestStrategyStop,
        TestStrategyStatus
    }
}
