using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Infrastructure;

public static class ResultStageRules
{
    public static bool CanPrint(VisitTest vt)
    {
        if (vt.TestResults == null || vt.TestResults.Count == 0)
            return false;
        return vt.TestResults.All(tr => tr.ValidationStatus >= ResultValidationStatus.Reviewed);
    }

    public static bool CanExport(VisitTest vt)
    {
        return vt.IsPrinted;
    }

    public static bool CanDeliver(VisitTest vt)
    {
        return vt.IsPrinted;
    }

    public static ResultValidationStatus GetMinimumValidationStatus(VisitTest vt)
    {
        if (vt.TestResults == null || vt.TestResults.Count == 0)
            return ResultValidationStatus.Entered;
        return vt.TestResults.Min(tr => tr.ValidationStatus);
    }
}
