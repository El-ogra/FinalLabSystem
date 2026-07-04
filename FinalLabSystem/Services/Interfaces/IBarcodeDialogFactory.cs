using System.Windows;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Services.Interfaces;

public interface IBarcodeDialogFactory
{
    BarcodeDialogResult Show(int visitId, int patientId, Window? owner = null);
}
