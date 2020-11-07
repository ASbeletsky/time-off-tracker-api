using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApiModels.Models
{
    public class TimeOffRequestApiModel
    {
        public TimeOffRequestApiModel()
        {
            Reviews = new List<TimeOffRequestReviewApiModel>();
            ReviewsIds = new List<int>();
        }
        public int Id { get; set; }
        [Required]
        public int TypeId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        
        public ICollection<TimeOffRequestReviewApiModel> Reviews { get; set; }
        [Required]
        public ICollection<int> ReviewsIds { get; set; }
        public string Comment { get; set; }
        [Required]
        public int StateId { get; set; }
        public int DurationId { get; set; }
        public UserApiModel User { get; set; }
        [Required]
        public int UserId { get; set; }
        public int ParentRequestId { get; set; }
    }
}
