using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Patients;

public class BarcodeDialogLowStockWarningTests
{
    private static (BarcodeDialogViewModel VM, Mock<ISampleTrackingService> SampleMock, Mock<ILabelPrintService> LabelMock, Mock<IInventoryService> InventoryMock, Mock<IDialogService> DialogMock) CreateVM()
    {
        var sampleMock = new Mock<ISampleTrackingService>();
        var labelMock = new Mock<ILabelPrintService>();
        var inventoryMock = new Mock<IInventoryService>();
        var dialogMock = new Mock<IDialogService>();
        var vm = new BarcodeDialogViewModel(sampleMock.Object, labelMock.Object, inventoryMock.Object, dialogMock.Object);
        return (vm, sampleMock, labelMock, inventoryMock, dialogMock);
    }

    private static SampleTube CreateTube(string tubeType)
    {
        return new SampleTube
        {
            TubeId = 1,
            VisitId = 1,
            TubeType = tubeType,
            BarcodeValue = "BARCODE001",
            Visit = new Visit
            {
                VisitId = 1,
                Patient = new Patient
                {
                    PatientId = 1,
                    FullNameAr = "أحمد",
                    PatientCode = "P001",
                    Sex = "M"
                },
                VisitTests = new List<VisitTest>()
            }
        };
    }

    [Fact]
    public async Task PrintLabelAsync_LowStock_ShowsWarning()
    {
        var (vm, _, _, inventoryMock, dialogMock) = CreateVM();
        var tube = CreateTube("Red Top");
        var label = new BarcodeLabel("أحمد", "Male", "CBC", "BARCODE001", "P001", "Red Top", tube);

        inventoryMock.Setup(i => i.GetByTubeTypeAsync("Red Top"))
            .ReturnsAsync(new TubeMaterial { MaterialName = "Red Top", MaterialNameAr = "أنابيب حمراء", CurrentStock = 5, MinimumStock = 10 });

        vm.PrintBarcodeCommand.Execute(label);
        await Task.Delay(200);

        dialogMock.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PrintLabelAsync_NormalStock_NoWarning()
    {
        var (vm, _, _, inventoryMock, dialogMock) = CreateVM();
        var tube = CreateTube("Red Top");
        var label = new BarcodeLabel("أحمد", "Male", "CBC", "BARCODE001", "P001", "Red Top", tube);

        inventoryMock.Setup(i => i.GetByTubeTypeAsync("Red Top"))
            .ReturnsAsync(new TubeMaterial { MaterialName = "Red Top", MaterialNameAr = "أنابيب حمراء", CurrentStock = 50, MinimumStock = 10 });

        vm.PrintBarcodeCommand.Execute(label);
        await Task.Delay(200);

        dialogMock.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PrintAllAsync_MixedStock_ShowsCombinedWarning()
    {
        var (vm, _, _, inventoryMock, dialogMock) = CreateVM();

        var tube1 = CreateTube("Red Top");
        var tube2 = CreateTube("EDTA");
        var label1 = new BarcodeLabel("أحمد", "Male", "CBC", "BARCODE001", "P001", "Red Top", tube1);
        var label2 = new BarcodeLabel("أحمد", "Male", "CBC", "BARCODE002", "P001", "EDTA", tube2);

        vm.Labels.Add(label1);
        vm.Labels.Add(label2);

        inventoryMock.Setup(i => i.GetByTubeTypeAsync("Red Top"))
            .ReturnsAsync(new TubeMaterial { MaterialName = "Red Top", MaterialNameAr = "أنابيب حمراء", CurrentStock = 5, MinimumStock = 10 });
        inventoryMock.Setup(i => i.GetByTubeTypeAsync("EDTA"))
            .ReturnsAsync(new TubeMaterial { MaterialName = "EDTA", MaterialNameAr = "EDTA", CurrentStock = 2, MinimumStock = 10 });

        vm.PrintAllCommand.Execute(null);
        await Task.Delay(200);

        dialogMock.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PrintAllAsync_AllNormalStock_NoWarning()
    {
        var (vm, _, _, inventoryMock, dialogMock) = CreateVM();

        var tube1 = CreateTube("Red Top");
        var tube2 = CreateTube("EDTA");
        var label1 = new BarcodeLabel("أحمد", "Male", "CBC", "BARCODE001", "P001", "Red Top", tube1);
        var label2 = new BarcodeLabel("أحمد", "Male", "CBC", "BARCODE002", "P001", "EDTA", tube2);

        vm.Labels.Add(label1);
        vm.Labels.Add(label2);

        inventoryMock.Setup(i => i.GetByTubeTypeAsync("Red Top"))
            .ReturnsAsync(new TubeMaterial { MaterialName = "Red Top", MaterialNameAr = "أنابيب حمراء", CurrentStock = 50, MinimumStock = 10 });
        inventoryMock.Setup(i => i.GetByTubeTypeAsync("EDTA"))
            .ReturnsAsync(new TubeMaterial { MaterialName = "EDTA", MaterialNameAr = "EDTA", CurrentStock = 30, MinimumStock = 10 });

        vm.PrintAllCommand.Execute(null);
        await Task.Delay(200);

        dialogMock.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
