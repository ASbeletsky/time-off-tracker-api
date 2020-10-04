using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum TimeOffType : int
    {
        PaidLeave,
        AdministrativeUnpaidLeave,
        StudyLeave,
        ForceMajeureAdministrativeLeave,
        SocialLeave,
        SickLeaveWithDocuments,
        SickLeaveWithoutDocuments
    }
}
