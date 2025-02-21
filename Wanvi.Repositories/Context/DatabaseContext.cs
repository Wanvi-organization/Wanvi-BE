using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;

namespace Wanvi.Repositories.Context
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        #region Entity
        public virtual DbSet<Activity> Activities => Set<Activity>();
        public virtual DbSet<Address> Addresses => Set<Address>();
        public virtual DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
        public virtual DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
        public virtual DbSet<ApplicationUserClaim> ApplicationUserClaims => Set<ApplicationUserClaim>();
        public virtual DbSet<ApplicationUserRole> ApplicationUserRoles => Set<ApplicationUserRole>();
        public virtual DbSet<ApplicationUserLogin> ApplicationUserLogins => Set<ApplicationUserLogin>();
        public virtual DbSet<ApplicationRoleClaim> ApplicationRoleClaims => Set<ApplicationRoleClaim>();
        public virtual DbSet<ApplicationUserToken> ApplicationUserTokens => Set<ApplicationUserToken>();
        public virtual DbSet<Booking> Bookings => Set<Booking>();
        public virtual DbSet<BookingDetail> BookingDetails => Set<BookingDetail>();
        public virtual DbSet<Category> Categories => Set<Category>();
        public virtual DbSet<City> Cities => Set<City>();
        public virtual DbSet<Comment> Comments => Set<Comment>();
        public virtual DbSet<Conversation> Conversations => Set<Conversation>();
        public virtual DbSet<Dashboard> Dashboard => Set<Dashboard>();
        public virtual DbSet<District> Districts => Set<District>();
        public virtual DbSet<Hashtag> Hashtags => Set<Hashtag>();
        public virtual DbSet<Media> Medias => Set<Media>();
        public virtual DbSet<Message> Messages => Set<Message>();
        public virtual DbSet<MessageMedia> MessageMedias => Set<MessageMedia>();
        public virtual DbSet<News> News => Set<News>();
        public virtual DbSet<NewsDetail> NewsDetails => Set<NewsDetail>();
        public virtual DbSet<Payment> Payments => Set<Payment>();
        public virtual DbSet<Post> Posts => Set<Post>();
        public virtual DbSet<PostHashtag> PostHashtags => Set<PostHashtag>();
        public virtual DbSet<PremiumPackage> PremiumPackages => Set<PremiumPackage>();
        public virtual DbSet<Review> Reviews => Set<Review>();
        public virtual DbSet<Schedule> Schedules => Set<Schedule>();
        public virtual DbSet<Subscription> Subscriptions => Set<Subscription>();
        public virtual DbSet<Tour> Tours => Set<Tour>();
        public virtual DbSet<TourActivity> TourActivities => Set<TourActivity>();
        public virtual DbSet<TourAddress> TourAddresses => Set<TourAddress>();
        public virtual DbSet<Voucher> Vouchers => Set<Voucher>();

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dashboard>().HasNoKey();

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
            });

            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles");
            });

            modelBuilder.Entity<ApplicationUserClaim>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            modelBuilder.Entity<ApplicationUserRole>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            modelBuilder.Entity<ApplicationUserLogin>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            modelBuilder.Entity<ApplicationRoleClaim>(entity =>
            {
                entity.ToTable("RoleClaims");
            });

            modelBuilder.Entity<ApplicationUserToken>(entity =>
            {
                entity.ToTable("UserTokens");
            });

            modelBuilder.Entity<ApplicationUserLogin>()
                .HasKey(l => new { l.UserId, l.LoginProvider, l.ProviderKey });
            modelBuilder.Entity<ApplicationUserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<ApplicationUserToken>()
            .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            modelBuilder.Entity<News>()
                .HasOne(n => n.User)
                .WithMany(u => u.News)
                .HasForeignKey(n => n.UserId);

            modelBuilder.Entity<Post>()
                .HasOne(n => n.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(n => n.UserId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.News)
                .WithMany(n => n.Comments)
                .HasForeignKey(c => c.NewsId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<News>()
                .HasOne(n => n.User)
                .WithMany(u => u.News)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NewsDetail>()
                .HasOne(nd => nd.News)
                .WithMany(n => n.NewsDetails)
                .HasForeignKey(nd => nd.NewsId);

            modelBuilder.Entity<City>()
                .HasMany(c => c.Districts)
                .WithOne(d => d.City)
                .HasForeignKey(d => d.CityId);

            modelBuilder.Entity<District>()
                .HasMany(d => d.Addresses)
                .WithOne(a => a.District)
                .HasForeignKey(a => a.DistrictId);

            modelBuilder.Entity<Tour>()
                .HasOne(t => t.PickupAddress)
                .WithMany(a => a.PickupTours)
                .HasForeignKey(t => t.PickupAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tour>()
                .HasOne(t => t.DropoffAddress)
                .WithMany(a => a.DropoffTours)
                .HasForeignKey(t => t.DropoffAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TourAddress>()
                .HasKey(tp => new { tp.TourId, tp.AddressId });
             
            modelBuilder.Entity<TourAddress>()
                .HasOne(tp => tp.Tour)
                .WithMany(t => t.TourAddresses)
                .HasForeignKey(tp => tp.TourId);

            modelBuilder.Entity<TourAddress>()
                .HasOne(tp => tp.Address)
                .WithMany(a => a.TourPoints)
                .HasForeignKey(tp => tp.AddressId);

            modelBuilder.Entity<TourActivity>()
            .HasKey(ta => new { ta.TourId, ta.ActivityId });

            modelBuilder.Entity<TourActivity>()
                .HasOne(ta => ta.Tour)
                .WithMany(t => t.TourActivities)
                .HasForeignKey(ta => ta.TourId);

            modelBuilder.Entity<TourActivity>()
                .HasOne(ta => ta.Activity)
                .WithMany(a => a.TourActivities)
                .HasForeignKey(ta => ta.ActivityId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User1)
                .WithMany()
                .HasForeignKey(c => c.User1Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MessageMedia>()
            .HasKey(mm => new { mm.MessageId, mm.MediaId });

            modelBuilder.Entity<MessageMedia>()
                .HasOne(mm => mm.Message)
                .WithMany(m => m.MessageMedias)
                .HasForeignKey(mm => mm.MessageId);

            modelBuilder.Entity<MessageMedia>()
                .HasOne(mm => mm.Media)
                .WithMany(m => m.MessageMedias)
                .HasForeignKey(mm => mm.MediaId);

            modelBuilder.Entity<News>()
                .HasOne(n => n.Category)
                .WithMany(c => c.News)
                .HasForeignKey(n => n.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Tour)
                .WithMany(t => t.Reviews)
                .HasForeignKey(r => r.TourId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Media>()
                .HasOne(r => r.Tour)
                .WithMany(t => t.Medias)
                .HasForeignKey(r => r.TourId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Media>()
                .HasOne(r => r.Post)
                .WithMany(u => u.Medias)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostHashtag>()
                .HasKey(ph => new { ph.PostId, ph.HashtagId });

            modelBuilder.Entity<PostHashtag>()
                .HasOne(ph => ph.Post)
                .WithMany(p => p.PostHashtags)
                .HasForeignKey(ph => ph.PostId);

            modelBuilder.Entity<PostHashtag>()
                .HasOne(ph => ph.Hashtag)
                .WithMany(h => h.PostHashtags)
                .HasForeignKey(ph => ph.HashtagId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Schedule)
                .WithMany(b => b.Bookings)
                .HasForeignKey(b => b.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasMany(b => b.BookingDetails)
                .WithOne(bd => bd.Booking)
                .HasForeignKey(bd => bd.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasMany(b => b.Payments)
                .WithOne(bp => bp.Booking)
                .HasForeignKey(bp => bp.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Subscription>()
                .HasOne(up => up.ApplicationUser)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Subscription>()
                .HasOne(up => up.PremiumPackage)
                .WithMany()
                .HasForeignKey(up => up.PremiumPackageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tour>()
                .HasOne(t => t.ApplicationUser)
                .WithMany(u => u.Tours)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
