using System.Data.Common;
using System.Diagnostics;
using System.Reflection;

namespace Handy.Extensions
{
    internal static class SqlCommandExtensions
    {
        public static void AddArguments(this DbCommand dataCommand, object[] arguments, StackFrame stackFrame, MethodBase callingMethod)
        {
            if (!stackFrame.HasMethod())
            {
                return;
            }

            if (arguments.Length == 0)
            {
                return;
            }

            ParameterInfo[] methodParameters = callingMethod.GetParameters();

            for (int index = 0; index < methodParameters.Length; index++)
            {
                ParameterInfo currentParameter = methodParameters[index];
                ParameterAttribute parameterAttribute = currentParameter.GetCustomAttribute<ParameterAttribute>();

                string parameterName = parameterAttribute == null ? $"@{currentParameter.Name}" : $"@{parameterAttribute.Name}";

                DbParameter newParameter = dataCommand.CreateParameter();
                newParameter.ParameterName = parameterName;
                newParameter.Value = arguments[index];

                dataCommand.Parameters.Add(newParameter);
            }
        }
    }
}