using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum VacationRequestState : int
    {
        New = 1,
        InProgress = 2,
        Approved = 3,
        Rejected = 4
    }
}
