using System;
using System.Configuration;

namespace Forte.EpiCommonUtils.Infrastructure
{
    public static class HostingEnvironment
    {
        public static string Name => ConfigurationManager.AppSettings["episerver:EnvironmentName"];

        public static bool IsLocalDev => "LocalDev".Equals(Name, StringComparison.OrdinalIgnoreCase);
        public static bool IsIntegration => "Integration".Equals(Name, StringComparison.OrdinalIgnoreCase);
        public static bool IsDev => "Dev".Equals(Name, StringComparison.OrdinalIgnoreCase);
        public static bool IsPreProd => "Preproduction".Equals(Name, StringComparison.OrdinalIgnoreCase);
        public static bool IsProd => "Production".Equals(Name, StringComparison.OrdinalIgnoreCase);
    }
}
