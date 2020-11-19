using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Context
{
    public class TimeOffTrackerContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public TimeOffTrackerContext(DbContextOptions<TimeOffTrackerContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TimeOffRequestReview>()
                .HasOne(r => r.Reviewer)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TimeOffRequestReview>()
                .HasOne(review => review.Request)
                .WithMany(request => request.Reviews)
                .HasForeignKey(review => review.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimeOffRequest>()
                .HasOne(req => req.User)
                .WithMany(usr => usr.Requests)
                .HasForeignKey(req => req.UserId);

            modelBuilder.Entity<UsedDaysStatistic>()
                .HasOne(x => x.User)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TimeOffRequest> TimeOffRequests { get; set; }
        public DbSet<TimeOffRequestReview> TimeOffRequestReviews { get; set; }
        public DbSet<UsedDaysStatistic> Statistics { get; set; }
    }
}
