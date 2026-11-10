using System.Reflection;

namespace API1.Configuration
{
    public static class confRN
    {
        public static IServiceCollection AddConfRN(this IServiceCollection services)
        {
            var ruleTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.Contains("RN") && !t.IsAbstract);

            foreach (var ruleType in ruleTypes)
            {
                services.AddScoped(ruleType);
            }
            return services;
        }
    }
}
