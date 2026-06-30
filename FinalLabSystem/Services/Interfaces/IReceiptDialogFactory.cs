using System.Windows;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface IReceiptDialogFactory
{
    bool Show(VisitFullDto dto, Window? owner = null);
}
