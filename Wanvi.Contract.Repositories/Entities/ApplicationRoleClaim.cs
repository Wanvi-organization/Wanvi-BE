﻿using Microsoft.AspNetCore.Identity;
using Wanvi.Core.Utils;

namespace Wanvi.Contract.Repositories.Entities
{
    public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
    {
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
        public virtual ApplicationRole Role { get; set; }
        public ApplicationRoleClaim()
        {
            CreatedTime = CoreHelper.SystemTimeNow;
            LastUpdatedTime = CreatedTime;
        }
    }
}
