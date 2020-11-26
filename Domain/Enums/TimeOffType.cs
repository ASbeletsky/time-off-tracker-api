using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum TimeOffType : int
    {
        ForceMajeureAdministrativeLeave = 1,
        AdministrativeUnpaidLeave = 2,
        SocialLeave = 3,
        SickLeaveWithoutDocuments = 4,
        SickLeaveWithDocuments = 5,
        StudyLeave = 6,
        PaidLeave = 7
    }
}
