using System.Windows;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface INormalRangesWindowFactory
{
    void Open(TestType editableTest, Window? owner = null);
}
