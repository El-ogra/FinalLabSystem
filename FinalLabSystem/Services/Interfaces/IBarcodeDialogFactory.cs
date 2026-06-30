using System.Windows;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Services.Interfaces;

public interface IBarcodeDialogFactory
{
    BarcodeDialogResult Show(int visitId, Window? owner = null);
}
