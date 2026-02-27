namespace SystemIntelligencePlatform.Permissions;

public static class SystemIntelligencePlatformPermissions
{
    public const string GroupName = "SystemIntelligencePlatform";

    public static class Books
    {
        public const string Default = GroupName + ".Books";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

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
    }

    public static class LogEvents
    {
        public const string Default = GroupName + ".LogEvents";
    }
}
