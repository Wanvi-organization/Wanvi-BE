﻿using Microsoft.AspNetCore.Identity;
using Wanvi.Core.Utils;

namespace Wanvi.Contract.Repositories.Entities
{
    public class ApplicationUserToken : IdentityUserToken<Guid>
    {
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
        public virtual ApplicationUser User { get; set; }
        public ApplicationUserToken()
        {
            CreatedTime = CoreHelper.SystemTimeNow;
            LastUpdatedTime = CreatedTime;
        }
    }
}
