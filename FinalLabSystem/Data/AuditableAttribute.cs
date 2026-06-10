using System;

namespace FinalLabSystem.Data;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AuditableAttribute : Attribute
{
}
