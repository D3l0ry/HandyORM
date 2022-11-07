using System.Diagnostics;
using System.Reflection;

using Microsoft.Data.SqlClient;

namespace Handy.Extensions
{
    internal static class SqlCommandExtensions
    {
        public static void AddArguments(this SqlCommand dataCommand, object[] arguments, StackFrame stackFrame, MethodBase callingMethod)
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
                ParameterAttribute parameterAttribute = methodParameters[index].GetCustomAttribute<ParameterAttribute>();

                string parameterName = parameterAttribute == null ? $"@{currentParameter.Name}" : $"@{parameterAttribute.Name}";

                SqlParameter newParameter = new SqlParameter()
                {
                    ParameterName = parameterName,
                    Value = arguments[index]
                };

                dataCommand.Parameters.Add(newParameter);
            }
        }
    }
}