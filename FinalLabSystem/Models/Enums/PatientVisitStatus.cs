using System;

namespace FinalLabSystem.Models.Enums;

[Obsolete("Use the 5 boolean flags on Visit and VisitDisplayStatus instead. (VS-03)")]
public enum PatientVisitStatus
{
    NewNoResults = 0,
    HasUnwrittenResults = 1,
    HasUnreviewedResults = 2,
    HasUnprintedResults = 3,
    HasUndeliveredResults = 4,
    CompleteWithBalance = 5,
    FullyComplete = 6
}
