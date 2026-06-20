namespace FinalLabSystem.Models.Enums;

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
