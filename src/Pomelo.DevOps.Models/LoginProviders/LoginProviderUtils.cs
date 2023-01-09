using System;
using System.Linq;
using System.Reflection;

namespace Pomelo.DevOps.Models.LoginProviders
{
    public static class LoginProviderUtils
    {
        public static Type GetLoginTypeByMode(LoginProviderType mode)
        {
            var _mode = mode.ToString();
            var attributeType = typeof(LoginProviderAttribute);
            return Assembly.GetAssembly(attributeType)
                .DefinedTypes
                .Where(x => x.CustomAttributes.Any(x => x.AttributeType.IsEquivalentTo(attributeType) && x.ConstructorArguments.FirstOrDefault().Value?.ToString() == _mode))
                .FirstOrDefault()
                .AsType();
        }
    }
}
