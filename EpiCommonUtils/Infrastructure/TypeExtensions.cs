using System;
using EPiServer.Web.Mvc;

namespace Forte.EpiCommonUtils.Infrastructure;

public static class TypeExtensions
{
    public static bool IsPageController(this Type controllerType) =>
        controllerType != null && controllerType.IsAssignableToGenericDefinition(typeof(PageController<>));

    public static bool IsAssignableToGenericDefinition(this Type type, Type genericType)
    {
        if (genericType.IsInterface)
        {
            throw new InvalidOperationException("This method is valid only for concrete types.");
        }

        while (type != null && type != typeof(object))
        {
            var potentialGenericTypeDefinition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (genericType == potentialGenericTypeDefinition)
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }
}
