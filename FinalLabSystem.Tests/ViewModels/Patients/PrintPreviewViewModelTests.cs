using System.Windows.Documents;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Patients;

public class PrintPreviewViewModelTests
{
    [Fact]
    public void Constructor_Sets_Default_Document_To_Empty_FlowDocument()
    {
        var vm = new PrintPreviewViewModel(Mock.Of<IPrintService>());

        Assert.NotNull(vm.Document);
    }

    [Fact]
    public void Document_Setter_RaisesPropertyChanged()
    {
        var vm = new PrintPreviewViewModel(Mock.Of<IPrintService>());
        bool raised = false;
        vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.Document)) raised = true; };

        vm.Document = new FlowDocument();

        Assert.True(raised);
    }

    [Fact]
    public void Description_Setter_RaisesPropertyChanged()
    {
        var vm = new PrintPreviewViewModel(Mock.Of<IPrintService>());
        bool raised = false;
        vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.Description)) raised = true; };

        vm.Description = "test description";

        Assert.True(raised);
    }

    [Fact]
    public void PrintCommand_CallsPrintService_WithDocument_AndDescription()
    {
        var mockService = new Mock<IPrintService>();
        mockService.Setup(s => s.PrintFlowDocumentAsync(It.IsAny<FlowDocument>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var vm = new PrintPreviewViewModel(mockService.Object);
        vm.Document = new FlowDocument();
        vm.Description = "test receipt";

        vm.PrintCommand.Execute(null);

        mockService.Verify(s => s.PrintFlowDocumentAsync(vm.Document, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void PrintCommand_PassesActualDescription_ToPrintFlowDocumentAsync()
    {
        var mockService = new Mock<IPrintService>();
        mockService.Setup(s => s.PrintFlowDocumentAsync(It.IsAny<FlowDocument>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var vm = new PrintPreviewViewModel(mockService.Object);
        vm.Document = new FlowDocument();
        vm.Description = "إيصال المريض";

        vm.PrintCommand.Execute(null);

        mockService.Verify(s => s.PrintFlowDocumentAsync(It.IsAny<FlowDocument>(), "إيصال المريض"), Times.Once);
    }

    [Fact]
    public void CloseCommand_Invokes_RequestClose()
    {
        var vm = new PrintPreviewViewModel(Mock.Of<IPrintService>());
        bool closed = false;
        vm.RequestClose = () => closed = true;

        vm.CloseCommand.Execute(null);

        Assert.True(closed);
    }

    [Fact]
    public void PrintCommand_CanExecute_False_When_Document_Null()
    {
        var vm = new PrintPreviewViewModel(Mock.Of<IPrintService>());

        vm.Document = null!;

        Assert.False(vm.PrintCommand.CanExecute(null));
    }

    [Fact]
    public void PrintCommand_CanExecute_True_When_Document_Is_Not_Null()
    {
        var vm = new PrintPreviewViewModel(Mock.Of<IPrintService>());
        vm.Document = new FlowDocument();

        Assert.True(vm.PrintCommand.CanExecute(null));
    }
}
