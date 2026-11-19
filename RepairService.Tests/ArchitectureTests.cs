using System.Reflection;
using RepairService.Core.Domain;

namespace RepairService.Tests;

public class ArchitectureTests
{
    private const BindingFlags AllMembers =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    [Fact]
    public void DomainTypes_DoNotReferenceOuterLayers()
    {
        // Arrange
        string[] outerLayers = ["RepairService.Core.Services", "RepairService.Core.Storage", "RepairService.App"];
        Type[] domainTypes = typeof(RepairTicket).Assembly.GetTypes()
            .Where(t => t.Namespace == "RepairService.Core.Domain")
            .ToArray();

        // Act
        string[] violations = domainTypes
            .SelectMany(t => UsedTypes(t).Select(used => (Owner: t, Used: used)))
            .Where(pair => outerLayers.Any(ns => pair.Used.Namespace?.StartsWith(ns, StringComparison.Ordinal) == true))
            .Select(pair => $"{pair.Owner.Name} → {pair.Used.FullName}")
            .ToArray();

        // Assert
        Assert.True(domainTypes.Length > 5);
        Assert.Empty(violations);
    }

    private static IEnumerable<Type> UsedTypes(Type type)
    {
        IEnumerable<Type> fields = type.GetFields(AllMembers).Select(f => f.FieldType);
        IEnumerable<Type> properties = type.GetProperties(AllMembers).Select(p => p.PropertyType);
        IEnumerable<MethodBase> methods = type.GetMethods(AllMembers).Cast<MethodBase>()
            .Concat(type.GetConstructors(AllMembers));
        IEnumerable<Type> signatures = methods.SelectMany(m => m.GetParameters().Select(p => p.ParameterType))
            .Concat(type.GetMethods(AllMembers).Select(m => m.ReturnType));
        return fields.Concat(properties).Concat(signatures).SelectMany(Unwrap);
    }

    private static IEnumerable<Type> Unwrap(Type type)
    {
        Type element = type.HasElementType ? type.GetElementType()! : type;
        return element.IsGenericType
            ? element.GetGenericArguments().SelectMany(Unwrap).Prepend(element)
            : [element];
    }
}
