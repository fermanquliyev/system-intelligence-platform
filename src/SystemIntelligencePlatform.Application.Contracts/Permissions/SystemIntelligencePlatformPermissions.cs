namespace SystemIntelligencePlatform.Permissions;

public static class SystemIntelligencePlatformPermissions
{
    public const string GroupName = "SystemIntelligencePlatform";

    public static class DashboardPermissions
    {
        public const string Default = GroupName + ".Dashboard";
    }

    public static class Applications
    {
        public const string Default = GroupName + ".Applications";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string RegenerateApiKey = Default + ".RegenerateApiKey";
    }

    public static class Incidents
    {
        public const string Default = GroupName + ".Incidents";
        public const string Update = Default + ".Update";
        public const string Resolve = Default + ".Resolve";
        public const string Comment = Default + ".Comment";
        public const string Search = Default + ".Search";
        public const string Assign = Default + ".Assign";
        public const string Copilot = Default + ".Copilot";
        public const string Timeline = Default + ".Timeline";
    }

    public static class LogEvents
    {
        public const string Default = GroupName + ".LogEvents";
        public const string Search = Default + ".Search";
        public const string ViewUnmasked = Default + ".ViewUnmasked";
    }

    public static class AlertRules
    {
        public const string Default = GroupName + ".AlertRules";
        public const string Manage = Default + ".Manage";
    }

    public static class Metrics
    {
        public const string Default = GroupName + ".Metrics";
        public const string Ingest = Default + ".Ingest";
    }

    public static class Playbooks
    {
        public const string Default = GroupName + ".Playbooks";
        public const string Manage = Default + ".Manage";
        public const string Run = Default + ".Run";
    }

    public static class LogSources
    {
        public const string Default = GroupName + ".LogSources";
        public const string Manage = Default + ".Manage";
    }

    public static class InstanceConfiguration
    {
        public const string Default = GroupName + ".InstanceConfiguration";
        public const string Migrate = Default + ".Migrate";
    }
}
