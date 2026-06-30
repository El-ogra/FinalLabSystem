using System.Reflection;
using FinalLabSystem.Views.Patients;

namespace FinalLabSystem.Tests.ViewModels.Patients;

public class PrintPreviewWindowMvvmPurityTests
{
    [Fact]
    public void CodeBehind_HasNo_document_Field()
    {
        var field = typeof(PrintPreviewWindow).GetField("_document",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.Null(field);
    }

    [Fact]
    public void CodeBehind_HasNo_PrintButton_OnClick_Method()
    {
        var method = typeof(PrintPreviewWindow).GetMethod("PrintButton_OnClick",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.Null(method);
    }

    [Fact]
    public void CodeBehind_HasNo_CloseButton_OnClick_Method()
    {
        var method = typeof(PrintPreviewWindow).GetMethod("CloseButton_OnClick",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.Null(method);
    }

    [Fact]
    public void CodeBehind_HasOnly_InitializeComponent()
    {
        var methods = typeof(PrintPreviewWindow)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Where(m => !m.IsPrivate)
            .Where(m => m.Name != "InitializeComponent")
            .ToList();

        Assert.Empty(methods);
    }

    [Fact]
    public void Constructor_IsParameterless()
    {
        var ctor = typeof(PrintPreviewWindow).GetConstructors(
            BindingFlags.Public | BindingFlags.Instance);

        Assert.Single(ctor);
        Assert.Empty(ctor[0].GetParameters());
    }
}
