using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.EF_Models
{
    public class UsedDaysStatistic
    {
        public int Id { get; set; }
        public TimeOffType Type { get; set; }
        public int NumberDaysUsed { get; set; }
        public string Year { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public TimeOffRequest Request { get; set; }
        public int RequestId { get; set; }
    }
}
