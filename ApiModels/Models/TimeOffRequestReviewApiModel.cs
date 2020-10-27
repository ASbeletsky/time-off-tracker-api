using System;
using System.Collections.Generic;
using System.Text;

namespace ApiModels.Models
{
    public class TimeOffRequestReviewApiModel
    {
        public int Id { get; set; }
        public UserApiModel Reviewer { get; set; }
        public int ReviewerId { get; set; }
        public bool? IsApproved { get; set; }
        public TimeOffRequestApiModel Request { get; set; }
        public int RequestId { get; set; }
        public string Comment { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
