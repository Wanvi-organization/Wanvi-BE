using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Core.Utils;
using Wanvi.Repositories.Context;

namespace Wanvi.Repositories.SeedData
{
    public class ApplicationDbContextInitialiser
{
    private readonly DatabaseContext _context;

    public ApplicationDbContextInitialiser(DatabaseContext context)
    {
        _context = context;
    }

    public void Initialise()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                _context.Database.Migrate();
                Seed();
            }

            if (_context.Database.IsMySql())
            {
                _context.Database.Migrate();
                Seed();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            _context.Dispose();
        }
    }

    public void Seed()
    {
        int data = 0;

        //data = _context.ApplicationRoles.Count();
        //if (data is 0)
        //{
        //    ApplicationRole[] roles = CreateRoles();
        //    _context.AddRange(roles);
        //}

        //data = _context.ApplicationUsers.Count();
        //if (data is 0)
        //{
        //    ApplicationUser[] user = CreateUsers();
        //    _context.AddRange(user);
        //}
        //_context.SaveChanges();

        //AssignRoleToUser("admin", "Admin");
        //AssignRoleToUser("staff", "Staff");
        //AssignRoleToUser("user1", "LocalGuide");
        //AssignRoleToUser("user2", "LocalGuide");
        //AssignRoleToUser("user3", "LocalGuide");
        //AssignRoleToUser("user4", "LocalGuide");
        //AssignRoleToUser("user5", "LocalGuide");
        //AssignRoleToUser("user6", "LocalGuide");
        //AssignRoleToUser("user7", "LocalGuide");
        //AssignRoleToUser("user8", "LocalGuide");
        //AssignRoleToUser("user9", "LocalGuide");
        //AssignRoleToUser("user10", "LocalGuide");
        //AssignRoleToUser("user11", "LocalGuide");
        //AssignRoleToUser("user12", "LocalGuide");
        //AssignRoleToUser("user13", "LocalGuide");
        //AssignRoleToUser("user14", "LocalGuide");
        //AssignRoleToUser("user15", "LocalGuide");
        //AssignRoleToUser("user16", "LocalGuide");
        //AssignRoleToUser("user17", "LocalGuide");
        //AssignRoleToUser("user18", "LocalGuide");
        //AssignRoleToUser("user19", "LocalGuide");
        //AssignRoleToUser("user20", "LocalGuide");
        //AssignRoleToUser("user21", "LocalGuide");
        //AssignRoleToUser("user22", "LocalGuide");
        //AssignRoleToUser("user23", "LocalGuide");
        //AssignRoleToUser("user24", "LocalGuide");
        //AssignRoleToUser("user25", "LocalGuide");
        //AssignRoleToUser("user26", "LocalGuide");
        //AssignRoleToUser("user27", "LocalGuide");
        //AssignRoleToUser("user28", "LocalGuide");
        //AssignRoleToUser("user29", "LocalGuide");
        //AssignRoleToUser("user30", "LocalGuide");
        //AssignRoleToUser("user31", "LocalGuide");
        //AssignRoleToUser("user32", "LocalGuide");
        //AssignRoleToUser("user33", "LocalGuide");
        //AssignRoleToUser("user34", "LocalGuide");
        //AssignRoleToUser("user35", "LocalGuide");
        //AssignRoleToUser("user36", "LocalGuide");
        //AssignRoleToUser("user37", "LocalGuide");
        //AssignRoleToUser("user38", "LocalGuide");
        //AssignRoleToUser("user39", "LocalGuide");
        //AssignRoleToUser("user40", "LocalGuide");
        //AssignRoleToUser("user41", "LocalGuide");
        //AssignRoleToUser("user42", "LocalGuide");
        //AssignRoleToUser("user43", "LocalGuide");
        //AssignRoleToUser("user44", "LocalGuide");
        //AssignRoleToUser("user45", "LocalGuide");
        //AssignRoleToUser("user46", "LocalGuide");
        //AssignRoleToUser("user47", "LocalGuide");
        //AssignRoleToUser("user48", "LocalGuide");


        //data = _context.Cities.Count();
        //if (data is 0)
        //{
        //    City[] cities = CreateCities();
        //    _context.AddRange(cities);

        //    District[] districts = CreateDistricts(cities);
        //    _context.AddRange(districts);
        //}

        data = _context.Activities.Count();
        if (data is 0)
        {
            Activity[] activities = CreateActivities();
            _context.AddRange(activities);
        }

        _context.SaveChanges();
    }

    private static ApplicationRole[] CreateRoles()
    {
        ApplicationRole[] roles =
          [
              new ApplicationRole
            {
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new ApplicationRole
            {
                Name = "Staff",
                NormalizedName = "STAFF",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new ApplicationRole
            {
                Name = "Traveler",
                NormalizedName = "TRAVELER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new ApplicationRole
            {
                Name = "LocalGuide",
                NormalizedName = "LOCALGUIDE",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        ];
        return roles;
    }

    private static ApplicationUser[] CreateUsers()
    {
        var passwordHasher = new FixedSaltPasswordHasher<ApplicationUser>(Options.Create(new PasswordHasherOptions()));
        ApplicationUser[] users =
        [
            new ApplicationUser
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                FullName = "Admin",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "79B/1 Nguyễn Thị Tràng, Hiệp Thành, Quận 12, Hồ Chí Minh",
                PhoneNumber = "0123456789",
                PhoneNumberConfirmed = true,
                Email = "admin@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234")
            },
            new ApplicationUser
            {
                UserName = "staff",
                NormalizedUserName = "STAFF",
                FullName = "Staff",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "89 Nguyễn Ảnh Thủ, Hiệp Thành, Quận 12, Hồ Chí Minh",
                PhoneNumber = "0123456788",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234")
            },
            new ApplicationUser
            {
                UserName = "user1",
                NormalizedUserName = "USER1",
                FullName = "Nguyễn Văn A",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "123 Đường 3/2, Quận 10, Hồ Chí Minh",
                PhoneNumber = "0123456781",
                PhoneNumberConfirmed = true,
                Email = "nguyenvana@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.2,
                MinHourlyRate = 250000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user2",
                NormalizedUserName = "USER2",
                FullName = "Trần Thị B",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "456 Nguyễn Thái Học, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456782",
                PhoneNumberConfirmed = true,
                Email = "tranthib@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.5,
                MinHourlyRate = 150000,
                IsPremium = false,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user3",
                NormalizedUserName = "USER3",
                FullName = "Lê Minh C",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "789 Lý Thường Kiệt, Tân Bình, Hồ Chí Minh",
                PhoneNumber = "0123456783",
                PhoneNumberConfirmed = true,
                Email = "leminhc@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.7,
                MinHourlyRate = 300000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user4",
                NormalizedUserName = "USER4",
                FullName = "Phạm Thị D",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "321 Nguyễn Đình Chiểu, Quận 3, Hồ Chí Minh",
                PhoneNumber = "0123456784",
                PhoneNumberConfirmed = true,
                Email = "phamthid@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 2.8,
                MinHourlyRate = 120000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user5",
                NormalizedUserName = "USER5",
                FullName = "Huỳnh Quang E",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "654 Bà Hạt, Quận 10, Hồ Chí Minh",
                PhoneNumber = "0123456785",
                PhoneNumberConfirmed = true,
                Email = "huynhquange@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.9,
                MinHourlyRate = 220000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user6",
                NormalizedUserName = "USER6",
                FullName = "Ngô Bảo F",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "987 Cách Mạng Tháng 8, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456786",
                PhoneNumberConfirmed = true,
                Email = "ngobaof@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.5,
                MinHourlyRate = 180000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user7",
                NormalizedUserName = "USER7",
                FullName = "Đặng Thị G",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "135 Hồ Tùng Mậu, Bình Thạnh, Hồ Chí Minh",
                PhoneNumber = "0123456787",
                PhoneNumberConfirmed = true,
                Email = "dangthig@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.0,
                MinHourlyRate = 250000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user8",
                NormalizedUserName = "USER8",
                FullName = "Vũ Thái H",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "258 Phan Đăng Lưu, Phú Nhuận, Hồ Chí Minh",
                PhoneNumber = "0123456788",
                PhoneNumberConfirmed = true,
                Email = "vuthaikh@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.0,
                MinHourlyRate = 400000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user9",
                NormalizedUserName = "USER9",
                FullName = "Bùi Minh I",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "567 Trường Chinh, Tân Phú, Hồ Chí Minh",
                PhoneNumber = "0123456789",
                PhoneNumberConfirmed = true,
                Email = "buiminhi@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.8,
                MinHourlyRate = 290000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user10",
                NormalizedUserName = "USER10",
                FullName = "Lê Thị K",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "789 Nguyễn Cảnh Chân, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456790",
                PhoneNumberConfirmed = true,
                Email = "lethik@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 2.5,
                MinHourlyRate = 150000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user11",
                NormalizedUserName = "USER11",
                FullName = "Trần Thị L",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "15 Nguyễn Văn Cừ, Quận 5, Hồ Chí Minh",
                PhoneNumber = "0123456711",
                PhoneNumberConfirmed = true,
                Email = "tranthil@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.8,
                MinHourlyRate = 220000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user12",
                NormalizedUserName = "USER12",
                FullName = "Lê Hoàng M",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "88 Lê Lợi, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456712",
                PhoneNumberConfirmed = true,
                Email = "lehoangm@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.5,
                MinHourlyRate = 320000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user13",
                NormalizedUserName = "USER13",
                FullName = "Nguyễn Văn N",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "99 Nguyễn Trãi, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456713",
                PhoneNumberConfirmed = true,
                Email = "nguyenvann@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.0,
                MinHourlyRate = 290000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user14",
                NormalizedUserName = "USER14",
                FullName = "Phạm Thị O",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "105 Trường Chinh, Tân Bình, Hồ Chí Minh",
                PhoneNumber = "0123456714",
                PhoneNumberConfirmed = true,
                Email = "phamthio@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.2,
                MinHourlyRate = 180000,
                IsPremium = false,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user15",
                NormalizedUserName = "USER15",
                FullName = "Vũ Đình P",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "58 Nguyễn Thị Minh Khai, Quận 3, Hồ Chí Minh",
                PhoneNumber = "0123456715",
                PhoneNumberConfirmed = true,
                Email = "vudinhp@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.8,
                MinHourlyRate = 450000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user16",
                NormalizedUserName = "USER16",
                FullName = "Đặng Minh Q",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "45 Lý Thường Kiệt, Tân Bình, Hồ Chí Minh",
                PhoneNumber = "0123456716",
                PhoneNumberConfirmed = true,
                Email = "dangminhq@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.1,
                MinHourlyRate = 300000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user17",
                NormalizedUserName = "USER17",
                FullName = "Hoàng Lan R",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "22 Phan Xích Long, Phú Nhuận, Hồ Chí Minh",
                PhoneNumber = "0123456717",
                PhoneNumberConfirmed = true,
                Email = "hoanglanr@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.6,
                MinHourlyRate = 200000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user18",
                NormalizedUserName = "USER18",
                FullName = "Phạm Văn S",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "79B Điện Biên Phủ, Bình Thạnh, Hồ Chí Minh",
                PhoneNumber = "0123456718",
                PhoneNumberConfirmed = true,
                Email = "phamvans@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.7,
                MinHourlyRate = 400000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user19",
                NormalizedUserName = "USER19",
                FullName = "Lê Thị T",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "158 Đ. số 17, Phường Linh Trung, Thủ Đức, Hồ Chí Minh",
                PhoneNumber = "0123456719",
                PhoneNumberConfirmed = true,
                Email = "lethit@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 2.9,
                MinHourlyRate = 150000,
                IsPremium = false,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user20",
                NormalizedUserName = "USER20",
                FullName = "Nguyễn Thành U",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "60 Trần Hưng Đạo, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456720",
                PhoneNumberConfirmed = true,
                Email = "nguyenthanhu@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 5.0,
                MinHourlyRate = 500000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user21",
                NormalizedUserName = "USER21",
                FullName = "Trần Minh V",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "15 Nguyễn Văn Trỗi, Phú Nhuận, Hồ Chí Minh",
                PhoneNumber = "0123456721",
                PhoneNumberConfirmed = true,
                Email = "tranminhv@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.2,
                MinHourlyRate = 180000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user22",
                NormalizedUserName = "USER22",
                FullName = "Bùi Thị W",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "Số 5 Đ. Số 16, Phường Linh Trung, Thủ Đức, Hồ Chí Minh",
                PhoneNumber = "0123456722",
                PhoneNumberConfirmed = true,
                Email = "buithiw@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.8,
                MinHourlyRate = 450000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user23",
                NormalizedUserName = "USER23",
                FullName = "Đỗ Văn X",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "33B Lê Văn Lương, Quận 7, Hồ Chí Minh",
                PhoneNumber = "0123456723",
                PhoneNumberConfirmed = true,
                Email = "dovanx@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.9,
                MinHourlyRate = 220000,
                IsPremium = false,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user24",
                NormalizedUserName = "USER24",
                FullName = "Lý Thị Y",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "50 Đinh Bộ Lĩnh, Bình Thạnh, Hồ Chí Minh",
                PhoneNumber = "0123456724",
                PhoneNumberConfirmed = true,
                Email = "lythiy@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.5,
                MinHourlyRate = 300000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user25",
                NormalizedUserName = "USER25",
                FullName = "Phan Văn Z",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "12 Trường Chinh, Tân Phú, Hồ Chí Minh",
                PhoneNumber = "0123456725",
                PhoneNumberConfirmed = true,
                Email = "phanvanz@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 2.5,
                MinHourlyRate = 120000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user26",
                NormalizedUserName = "USER26",
                FullName = "Lê Văn B",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "45 Lê Duẩn, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456726",
                PhoneNumberConfirmed = true,
                Email = "levanb@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.6,
                MinHourlyRate = 200000,
                IsPremium = false,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user27",
                NormalizedUserName = "USER27",
                FullName = "Trịnh Thị C",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "78 Nguyễn Cư Trinh, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456727",
                PhoneNumberConfirmed = true,
                Email = "trinhthic@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.9,
                MinHourlyRate = 480000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user28",
                NormalizedUserName = "USER28",
                FullName = "Vũ Văn D",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "24B Hoàng Văn Thụ, Phú Nhuận, Hồ Chí Minh",
                PhoneNumber = "0123456728",
                PhoneNumberConfirmed = true,
                Email = "vuvand@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.1,
                MinHourlyRate = 170000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user29",
                NormalizedUserName = "USER29",
                FullName = "Đặng Thị E",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "77 Nguyễn Văn Nghi, Quận Gò Vấp, Hồ Chí Minh",
                PhoneNumber = "0123456729",
                PhoneNumberConfirmed = true,
                Email = "dangthie@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.6,
                MinHourlyRate = 350000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user30",
                NormalizedUserName = "USER30",
                FullName = "Ngô Văn F",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "89 Lê Trọng Tấn, Tân Phú, Hồ Chí Minh",
                PhoneNumber = "0123456730",
                PhoneNumberConfirmed = true,
                Email = "ngovanf@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.4,
                MinHourlyRate = 190000,
                IsPremium = false,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user31",
                NormalizedUserName = "USER31",
                FullName = "Hoàng Thị G",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "55 Nguyễn Thị Minh Khai, Quận 3, Hồ Chí Minh",
                PhoneNumber = "0123456731",
                PhoneNumberConfirmed = true,
                Email = "hoangthig@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.2,
                MinHourlyRate = 290000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user32",
                NormalizedUserName = "USER32",
                FullName = "Phạm Văn H",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "99 Tô Ký, Quận 12, Hồ Chí Minh",
                PhoneNumber = "0123456732",
                PhoneNumberConfirmed = true,
                Email = "phamvanh@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 2.9,
                MinHourlyRate = 150000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user33",
                NormalizedUserName = "USER33",
                FullName = "Đoàn Thị I",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "12 Lê Hồng Phong, Quận 10, Hồ Chí Minh",
                PhoneNumber = "0123456733",
                PhoneNumberConfirmed = true,
                Email = "doanthiI@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.9,
                MinHourlyRate = 480000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user34",
                NormalizedUserName = "USER34",
                FullName = "Tạ Văn J",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "18 Trần Hưng Đạo, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456734",
                PhoneNumberConfirmed = true,
                Email = "tavanj@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.7,
                MinHourlyRate = 210000,
                IsPremium = false,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user35",
                NormalizedUserName = "USER35",
                FullName = "Lương Thị K",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "74 Hoàng Sa, Bình Thạnh, Hồ Chí Minh",
                PhoneNumber = "0123456735",
                PhoneNumberConfirmed = true,
                Email = "luongthik@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.3,
                MinHourlyRate = 310000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user36",
                NormalizedUserName = "USER36",
                FullName = "Lý Văn L",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "60A Nguyễn Kiệm, Quận Gò Vấp, Hồ Chí Minh",
                PhoneNumber = "0123456736",
                PhoneNumberConfirmed = true,
                Email = "lyvanl@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 2.8,
                MinHourlyRate = 140000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user37",
                NormalizedUserName = "USER37",
                FullName = "Nguyễn Thị M",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "20 Tôn Đức Thắng, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456737",
                PhoneNumberConfirmed = true,
                Email = "nguyenthim@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.7,
                MinHourlyRate = 360000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user38",
                NormalizedUserName = "USER38",
                FullName = "Trần Văn N",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "86 Điện Biên Phủ, Phường 17, Bình Thạnh, Hồ Chí Minh",
                PhoneNumber = "0123456738",
                PhoneNumberConfirmed = true,
                Email = "tranvann@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.5,
                MinHourlyRate = 220000,
                IsPremium = false,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user39",
                NormalizedUserName = "USER39",
                FullName = "Phạm Thị O",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "102 Trường Chinh, Tân Bình, Hồ Chí Minh",
                PhoneNumber = "0123456739",
                PhoneNumberConfirmed = true,
                Email = "phamthio@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.8,
                MinHourlyRate = 450000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user40",
                NormalizedUserName = "USER40",
                FullName = "Lê Văn P",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "200 Lý Thường Kiệt, Quận 10, Hồ Chí Minh",
                PhoneNumber = "0123456740",
                PhoneNumberConfirmed = true,
                Email = "levanp@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 2.7,
                MinHourlyRate = 180000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user41",
                NormalizedUserName = "USER41",
                FullName = "Ngô Thị Q",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "150 Hoàng Văn Thụ, Phú Nhuận, Hồ Chí Minh",
                PhoneNumber = "0123456741",
                PhoneNumberConfirmed = true,
                Email = "ngothiq@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.1,
                MinHourlyRate = 320000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user42",
                NormalizedUserName = "USER42",
                FullName = "Hoàng Văn R",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "222 Cộng Hòa, Tân Bình, Hồ Chí Minh",
                PhoneNumber = "0123456742",
                PhoneNumberConfirmed = true,
                Email = "hoangvanr@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.2,
                MinHourlyRate = 250000,
                IsPremium = false,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user43",
                NormalizedUserName = "USER43",
                FullName = "Võ Thị S",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "99 Phan Xích Long, Bình Thạnh, Hồ Chí Minh",
                PhoneNumber = "0123456743",
                PhoneNumberConfirmed = true,
                Email = "vothis@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.5,
                MinHourlyRate = 400000,
                IsPremium = true,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user44",
                NormalizedUserName = "USER44",
                FullName = "Đặng Văn T",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "85 Nguyễn Trãi, Quận 5, Hồ Chí Minh",
                PhoneNumber = "0123456744",
                PhoneNumberConfirmed = true,
                Email = "dangvant@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.9,
                MinHourlyRate = 280000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user45",
                NormalizedUserName = "USER45",
                FullName = "Tạ Thị U",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "188 Võ Văn Kiệt, Quận 1, Hồ Chí Minh",
                PhoneNumber = "0123456745",
                PhoneNumberConfirmed = true,
                Email = "tathiu@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.6,
                MinHourlyRate = 420000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user46",
                NormalizedUserName = "USER46",
                FullName = "Lý Văn V",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "35 Điện Biên Phủ, Bình Thạnh, Hồ Chí Minh",
                PhoneNumber = "0123456746",
                PhoneNumberConfirmed = true,
                Email = "lyvanv@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 2.9,
                MinHourlyRate = 170000,
                IsPremium = false,
                IsVerified = true
            },
            new ApplicationUser
            {
                UserName = "user47",
                NormalizedUserName = "USER47",
                FullName = "Lương Thị W",
                Gender = false,
                DateOfBirth = DateTime.UtcNow,
                Address = "14A Phan Văn Trị, Quận Gò Vấp, Hồ Chí Minh",
                PhoneNumber = "0123456747",
                PhoneNumberConfirmed = true,
                Email = "luongthiw@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 4.0,
                MinHourlyRate = 300000,
                IsPremium = true,
                IsVerified = false
            },
            new ApplicationUser
            {
                UserName = "user48",
                NormalizedUserName = "USER48",
                FullName = "Phạm Văn X",
                Gender = true,
                DateOfBirth = DateTime.UtcNow,
                Address = "220 Đặng Văn Bi, Thủ Đức, Hồ Chí Minh",
                PhoneNumber = "0123456748",
                PhoneNumberConfirmed = true,
                Email = "phamvanx@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = passwordHasher.HashPassword(null, "1234"),
                AvgRating = 3.3,
                MinHourlyRate = 230000,
                IsPremium = false,
                IsVerified = true
            }
        ];
        return users;
    }

    private static City[] CreateCities()
    {
        City[] cities =
    [
        new City { Name = "An Giang" },
        new City { Name = "Bà Rịa - Vũng Tàu" },
        new City { Name = "Bắc Giang" },
        new City { Name = "Bắc Kạn" },
        new City { Name = "Bắc Ninh"},
        new City { Name = "Bạc Liêu" },
        new City { Name = "Bến Tre" },
        new City { Name = "Bình Dương" },
        new City { Name = "Bình Định" },
        new City { Name = "Bình Phước" },
        new City { Name = "Bình Thuận" },
        new City { Name = "Cà Mau" },
        new City { Name = "Cao Bằng" },
        new City { Name = "Cần Thơ" },
        new City { Name = "Đà Nẵng"},
        new City { Name = "Đắk Lắk" },
        new City { Name = "Đắk Nông" },
        new City { Name = "Điện Biên" },
        new City { Name = "Đồng Nai" },
        new City { Name = "Đồng Tháp" },
        new City { Name = "Gia Lai" },
        new City { Name = "Hà Giang" },
        new City { Name = "Hà Nam" },
        new City { Name = "Hà Nội" },
        new City { Name = "Hà Tĩnh"},
        new City { Name = "Hải Dương" },
        new City { Name = "Hải Phòng" },
        new City { Name = "Hậu Giang" },
        new City { Name = "Hòa Bình" },
        new City { Name = "Hồ Chí Minh" },
        new City { Name = "Hưng Yên" },
        new City { Name = "Khánh Hòa" },
        new City { Name = "Kiên Giang" },
        new City { Name = "Kon Tum" },
        new City { Name = "Lai Châu" },
        new City { Name = "Lâm Đồng" },
        new City { Name = "Lạng Sơn" },
        new City { Name = "Lào Cai" },
        new City { Name = "Long An" },
        new City { Name = "Nam Định" },
        new City { Name = "Nghệ An" },
        new City { Name = "Ninh Bình" },
        new City { Name = "Ninh Thuận" },
        new City { Name = "Phú Thọ" },
        new City { Name = "Phú Yên" },
        new City { Name = "Quảng Bình" },
        new City { Name = "Quảng Nam" },
        new City { Name = "Quảng Ngãi" },
        new City { Name = "Quảng Ninh" },
        new City { Name = "Quảng Trị" },
        new City { Name = "Sóc Trăng" },
        new City { Name = "Sơn La" },
        new City { Name = "Tây Ninh" },
        new City { Name = "Thái Bình" },
        new City { Name = "Thái Nguyên" },
        new City { Name = "Thanh Hóa" },
        new City { Name = "Thừa Thiên - Huế" },
        new City { Name = "Tiền Giang" },
        new City { Name = "Trà Vinh" },
        new City { Name = "Tuyên Quang" },
        new City { Name = "Vĩnh Long" },
        new City { Name = "Vĩnh Phúc" },
        new City { Name = "Yên Bái" }
    ];
        return cities;
    }

    private static District[] CreateDistricts(City[] cities)
    {
        List<District> districts = new List<District>();

        var cityDict = cities.ToDictionary(c => c.Name, c => c);

        var districtData = new Dictionary<string, string[]>
        {
            { "Bắc Ninh", new[] { "Bắc Ninh", "Gia Bình", "Lương Tài", "Quế Võ", "Tiên Du", "Thuận Thành", "Yên Phong", "Từ Sơn" } },
            { "Đà Nẵng", new[] { "Hải Châu", "Cẩm Lệ", "Liên Chiểu", "Thanh Khê", "Sơn Trà", "Ngũ Hành Sơn", "Hoà Vang" } },
            { "Hà Tĩnh", new[] { "Hà Tĩnh", "Hương Sơn", "Hương Khê", "Vũ Quang", "Can Lộc", "Kỳ Anh", "Thạch Hà", "Lộc Hà", "Nghi Xuân", "Cẩm Xuyên", "Đức Thọ", "Nam Đàn" } },
            { "An Giang", new[] { "Long Xuyên", "Châu Đốc", "Tân Châu", "Châu Phú", "Phú Tân", "Châu Thành" } },
            { "Bà Rịa - Vũng Tàu", new[] { "Vũng Tàu", "Bà Rịa", "Châu Đức", "Đất Đỏ", "Long Điền", "Tân Thành", "Xuyên Mộc", "Côn Đảo" } },
            { "Bắc Giang", new[] { "Bắc Giang", "Lạng Giang", "Yên Thế", "Việt Yên", "Sơn Động", "Lục Ngạn", "Lục Nam", "Tân Yên", "Hiệp Hòa", "Gia Bình" } },
            { "Bắc Kạn", new[] { "Bắc Kạn", "Ba Bể", "Chợ Đồn", "Chợ Mới", "Na Rì", "Pác Nặm", "Ngân Sơn", "Bạch Thông" } },
            { "Bạc Liêu", new[] { "Bạc Liêu", "Châu Hưng", "Hòa Bình", "Hồng Dân", "Phước Long", "Vĩnh Lợi", "Huyện Đông Hải" } },
            { "Bến Tre", new[] { "Bến Tre", "Châu Thành", "Chợ Lách", "Giồng Trôm", "Mỏ Cày Bắc", "Mỏ Cày Nam", "Ba Tri", "Thạnh Phú", "Duyên Hải" } },
            { "Bình Dương", new[] { "Thủ Dầu Một", "Thuận An", "Dĩ An", "Bến Cát", "Tân Uyên", "Tân Thành", "Bàu Bàng", "Phú Giáo" } },
            { "Bình Định", new[] { "Quy Nhơn", "An Lão", "Hoài Nhơn", "Phù Cát", "Phù Mỹ", "Tây Sơn", "Vĩnh Thạnh", "An Nhơn", "Kỳ Anh" } },
            { "Bình Phước", new[] { "Đồng Xoài", "Phước Long", "Bù Đăng", "Bù Gia Mập", "Lộc Ninh", "Chơn Thành", "Bắc Bình", "Tân Lập" } },
            { "Bình Thuận", new[] { "Phan Thiết", "La Gi", "Tánh Linh", "Hàm Tân", "Hàm Thuận Bắc", "Hàm Thuận Nam", "Phú Quý", "Tuy Phong", "Đảo Phú Quý" } },
            { "Cà Mau", new[] { "Cà Mau", "Đầm Dơi", "Năm Căn", "Phú Tân", "Cái Nước", "Trần Văn Thời", "Thới Bình", "U Minh" } },
            { "Cao Bằng", new[] { "Cao Bằng", "Bảo Lạc", "Bảo Lâm", "Hòa An", "Nguyên Bình", "Phục Hòa", "Quảng Uyên", "Thạch An", "Trùng Khánh", "Thông Nông" } },
            { "Cần Thơ", new[] { "Ninh Kiều", "Bình Thủy", "Cái Răng", "Ô Môn", "Thốt Nốt", "Phong Điền", "Cờ Đỏ", "Vĩnh Thạnh" } },
            { "Đắk Lắk", new[] { "Buôn Ma Thuột", "Bảo Lộc", "Ea H'leo", "Ea Súp", "Cư M'gar", "Lăk", "Krông Bông", "Krông Ana", "Krông Buk" } },
            { "Đắk Nông", new[] { "Gia Nghĩa", "Cư Jút", "Đắk Mil", "Đắk R'Lấp", "Krông Nô", "Tuy Đức", "Chư Jút" } },
            { "Điện Biên", new[] { "Điện Biên Phủ", "Mường Lay", "Mường Chà", "Mường Nhé", "Tủa Chùa", "Điện Biên Đông", "Mường Ảng", "Nam Po" } },
            { "Đồng Nai", new[] { "Biên Hòa", "Long Khánh", "Trảng Bom", "Long Thành", "Nhơn Trạch", "Vĩnh Cửu" } },
            { "Đồng Tháp", new[] { "Sa Đéc", "Cao Lãnh", "Hồng Ngự", "Tam Nông", "Lấp Vò", "Thanh Bình", "Tân Hồng", "Châu Thành" } },
            { "Gia Lai", new[] { "Pleiku", "An Khê", "Chư Sê", "Chư Păh", "Ia Grai", "Kông Chro", "Krông Pa", "Mang Yang", "Phú Thiện", "Tây Sơn", "Đăk Đoa" } },
            { "Hà Giang", new[] { "Hà Giang", "Quản Bạ", "Yên Minh", "Vị Xuyên", "Bắc Mê", "Mèo Vạc", "Hoàng Su Phì", "Xín Mần", "Đồng Văn" } },
            { "Hà Nam", new[] { "Phủ Lý", "Duy Tiên", "Kim Bảng", "Lý Nhân", "Thanh Liêm", "Bình Lục", "Hà Nam" } },
            { "Hà Nội", new[] { "Ba Đình", "Hoàn Kiếm", "Tây Hồ", "Cầu Giấy", "Đống Đa", "Hai Bà Trưng", "Hoàng Mai", "Long Biên", "Thanh Xuân", "Hà Đông", "Sóc Sơn", "Thanh Oai", "Chương Mỹ", "Mỹ Đức", "Đan Phượng", "Hoài Đức", "Quốc Oai", "Thạch Thất", "Phú Xuyên", "Ứng Hòa", "Mỹ Đức" } },
            { "Hải Dương", new[] { "Hải Dương", "Chí Linh", "Kim Thành", "Kinh Môn", "Cẩm Giàng", "Thanh Miện", "Tứ Kỳ", "Ninh Giang", "Gia Lộc", "Bình Giang", "Thanh Hà", "Nam Sách", "Hải Tân", "Kinh Môn" } },
            { "Hải Phòng", new[] { "Hải Phòng", "Đồ Sơn", "Kiến An", "Hồng Bàng", "Lê Chân", "Ngô Quyền", "Dương Kinh", "Kiến Thụy", "An Dương", "An Lão", "Cát Hải", "Bạch Long Vĩ", "Thủy Nguyên" } },
            { "Hậu Giang", new[] { "Vị Thanh", "Long Mỹ", "Châu Thành", "Châu Thành A", "Ngã Bảy", "Phụng Hiệp", "Viên An", "Mỹ Tú", "Long Phú", "Trà Cú", "Năm Căn", "Đồng Xoài" } },
            { "Hòa Bình", new[] { "Hòa Bình", "Mai Châu", "Lạc Sơn", "Lạc Thủy", "Kim Bôi", "Tân Lạc", "Yên Thủy", "Đà Bắc", "Kỳ Sơn" } },
            { "Hồ Chí Minh", new[] { "Quận 1", "Quận 2", "Quận 3", "Quận 4", "Quận 5", "Quận 6", "Quận 7", "Quận 8", "Quận 9", "Quận 10", "Thủ Đức", "Bình Thạnh", "Phú Nhuận", "Tân Bình", "Tân Phú" } },
            { "Hưng Yên", new[] { "Hưng Yên", "Mỹ Hào", "Phù Cừ", "Kim Động", "Tiên Lữ", "Khoái Châu", "Văn Lâm", "Văn Giang", "Ân Thi", "Yên Mỹ" } },
            { "Khánh Hòa", new[] { "Nha Trang", "Cam Ranh", "Vạn Ninh", "Ninh Hòa", "Khánh Vĩnh", "Diên Khánh", "Cam Lâm", "Khánh Sơn", "Trường Sa" } },
            { "Kiên Giang", new[] { "Rạch Giá", "Tân Hiệp", "Châu Thành", "Hòn Đất", "Kiên Lương", "Giang Thành", "Vĩnh Thuận", "An Biên", "An Minh", "Phú Quốc", "Gò Quao" } },
            { "Kon Tum", new[] { "Kon Tum", "Đắk Hà", "Đắk Tô", "Sa Thầy", "Ngọc Hồi", "Tu Mơ Rông", "Ia H’Drai", "Kon Plông", "Kon Rẫy" } },
            { "Lai Châu", new[] { "Lai Châu", "Mường Tè", "Sìn Hồ", "Phong Thổ", "Tam Đường", "Tân Uyên", "Nậm Nhùn" } },
            { "Lâm Đồng", new[] { "Đà Lạt", "Bảo Lộc", "Lâm Hà", "Đơn Dương", "Di Linh", "Cát Tiên", "Bảo Lâm", "Đức Trọng", "Lạc Dương", "Lâm Đồng" } },
            { "Lạng Sơn", new[] { "Lạng Sơn", "Cao Lộc", "Văn Lãng", "Đình Lập", "Bắc Sơn", "Hữu Lũng", "Chi Lăng", "An Dương" } },
            { "Lào Cai", new[] { "Lào Cai", "Sapa", "Bảo Thắng", "Bảo Yên", "Mường Khương", "Văn Bàn", "Simacai" } },
            { "Long An", new[] { "Tân An", "Bến Lức", "Cần Giuộc", "Cần Đước", "Thủ Thừa", "Tân Hưng", "Châu Thành", "Đức Hòa", "Đức Huệ", "Mỹ Tiền", "Mỹ Đức" } },
            { "Nam Định", new[] { "Nam Định", "Mỹ Lộc", "Vụ Bản", "Nam Trực", "Trực Ninh", "Ý Yên", "Hải Hậu", "Giao Thủy", "Xuân Trường", "Nghĩa Hưng" } },
            { "Nghệ An", new[] { "Vinh", "Cửa Lò", "Hưng Nguyên", "Nghi Lộc", "Quỳnh Lưu", "Nam Đàn", "Thanh Chương", "Đô Lương", "Yên Thành", "Tân Kỳ", "Con Cuông", "Quỳ Hợp", "Kỳ Sơn", "Anh Sơn" } },
            { "Ninh Bình", new[] { "Ninh Bình", "Tam Điệp", "Gia Viễn", "Hoa Lư", "Yên Khánh", "Kim Sơn", "Nho Quan", "Tuyệt Diệu" } },
            { "Ninh Thuận", new[] { "Phan Rang", "Ninh Hải", "Ninh Sơn", "Bác Ái", "Thuận Bắc", "Thuận Nam", "Đầm Rông" } },
            { "Phú Thọ", new[] { "Việt Trì", "Phú Thọ", "Thanh Sơn", "Cẩm Khê", "Đoan Hùng", "Tân Sơn", "Yên Lập", "Hạ Hòa", "Lâm Thao", "Tam Nông", "Thanh Thủy", "Tân Quang" } },
            { "Phú Yên", new[] { "Tuy Hòa", "Sông Cầu", "Đồng Xuân", "Tuy An", "Phú Hòa", "Sơn Hòa", "Tây Hòa", "Vân Canh", "Trường Sa" } },
            { "Quảng Bình", new[] { "Đồng Hới", "Ba Đồn", "Quảng Trạch", "Lệ Thủy", "Minh Hóa", "Tuyên Hóa", "Bố Trạch" } },
            { "Quảng Nam", new[] { "Tam Kỳ", "Hội An", "Điện Bàn", "Duy Xuyên", "Quế Sơn", "Thăng Bình", "Hiệp Đức", "Nông Sơn", "Phú Ninh", "Tiên Phước", "Nam Giang", "Tây Giang", "Đại Lộc", "Hiệp Đức", "Dai Loc" } },
            { "Quảng Ngãi", new[] { "Quảng Ngãi", "Sơn Tịnh", "Trà Bồng", "Tư Nghĩa", "Mộ Đức", "Ba Tơ", "Bình Sơn", "Lý Sơn", "Nghĩa Hành", "Minh Long", "Đức Phổ", "Tây Sơn" } },
            { "Quảng Ninh", new[] { "Hạ Long", "Uông Bí", "Cẩm Phả", "Móng Cái", "Vân Đồn", "Đầm Hà", "Tiên Yên", "Ba Chẽ", "Bình Liêu", "Đông Triều", "Hải Hà", "Hoành Bồ" } },
            { "Quảng Trị", new[] { "Đông Hà", "Quảng Trị", "Hải Lăng", "Vĩnh Linh", "Gio Linh", "Triệu Phong", "Cam Lộ", "Đakrông", "Hướng Hóa", "Con Cuông" } },
            { "Sóc Trăng", new[] { "Sóc Trăng", "Ngã Năm", "Châu Thành", "Kế Sách", "Trần Đề", "Long Phú", "Mỹ Tú", "Vĩnh Châu", "Đầm Dơi" } },
            { "Sơn La", new[] { "Sơn La", "Mộc Châu", "Sông Mã", "Yên Châu", "Mai Sơn", "Phù Yên", "Mường La", "Quỳnh Nhai", "Vân Hồ" } },
            { "Tây Ninh", new[] { "Tây Ninh", "Trảng Bàng", "Châu Thành", "Dương Minh Châu", "Gò Dầu", "Bến Cầu", "Tân Biên", "Hòa Thành", "Hòa Hiệp" } },
            { "Thái Bình", new[] { "Thái Bình", "Quỳnh Phụ", "Hưng Hà", "Đông Hưng", "Vũ Thư", "Tiền Hải", "Thái Thụy", "Kiến Xương", "Duyên Hải" } },
            { "Thái Nguyên", new[] { "Thái Nguyên", "Sông Công", "Phổ Yên", "Đại Từ", "Võ Nhai", "Phú Lương", "Định Hóa", "Đồng Hỷ", "Tân Cương" } },
            { "Thanh Hóa", new[] { "Thanh Hóa", "Sầm Sơn", "Mường Lát", "Quan Hóa", "Quan Sơn", "Ngọc Lặc", "Cẩm Thủy", "Thường Xuân", "Triệu Sơn", "Hậu Lộc", "Hoằng Hóa", "Tĩnh Gia", "Yên Định", "Nông Cống", "Thạch Thành", "Lang Chánh" } },
            { "Thừa Thiên - Huế", new[] { "Huế", "Hương Thủy", "Hương Trà", "Phú Vang", "Phú Lộc", "Quảng Điền", "A Lưới", "Nam Đông" } },
            { "Tiền Giang", new[] { "Mỹ Tho", "Cai Lậy", "Gò Công", "Châu Thành", "Chợ Gạo", "Tân Phú Đông", "Tân Hiệp", "Cái Bè", "Gò Công Tây", "Gò Công Đông" } },
            { "Trà Vinh", new[] { "Trà Vinh", "Càng Long", "Cầu Kè", "Châu Thành", "Tiểu Cần", "Cầu Ngang", "Duyên Hải", "Long Mỹ", "Trà Cú", "Phú Hòa" } },
            { "Tuyên Quang", new[] { "Tuyên Quang", "Yên Sơn", "Chiêm Hóa", "Hàm Yên", "Na Hang", "Lâm Bình", "Sơn Dương" } },
            { "Vĩnh Long", new[] { "Vĩnh Long", "Long Hồ", "Mang Thít", "Tam Bình", "Trà Ôn", "Vũng Liêm", "Bình Minh", "Dũng Liêm" } },
            { "Vĩnh Phúc", new[] { "Vĩnh Yên", "Phúc Yên", "Bình Xuyên", "Lập Thạch", "Sông Lô", "Tam Đảo", "Mê Linh", "Vĩnh Tường", "Yên Lạc" } },
            { "Yên Bái", new[] { "Yên Bái", "Nghĩa Lộ", "Trấn Yên", "Văn Chấn", "Mù Cang Chải", "Lục Yên", "Văn Yên", "Trạm Tấu", "Yên Bình" } }
        };

        foreach (var entry in districtData)
        {
            if (cityDict.TryGetValue(entry.Key, out var city))
            {
                foreach (var districtName in entry.Value)
                {
                    districts.Add(new District { Name = districtName, CityId = city.Id });
                }
            }
        }

        return districts.ToArray();
    }

    private static Activity[] CreateActivities()
    {
        Activity[] activities =
    [
        new Activity
            {
                Name = "Workshop",
                Description = "Các buổi workshop hấp dẫn về nhiều chủ đề, từ kỹ năng sống đến các kỹ thuật sáng tạo, giúp du khách mở rộng kiến thức và trải nghiệm thực tế."
            },
        new Activity
            {
                Name = "Ẩm thực",
                Description = "Khám phá văn hóa ẩm thực phong phú của địa phương thông qua các lớp học nấu ăn, tham quan các khu chợ và thử các món ăn đặc trưng."
            },
        new Activity
            {
                Name = "Thủ công mỹ nghệ",
                Description = "Trải nghiệm các hoạt động thủ công mỹ nghệ truyền thống, từ làm gốm, dệt vải đến tạo ra các sản phẩm thủ công độc đáo."
            },
        new Activity
            {
                Name = "Tình nguyện viên",
                Description = "Tham gia các hoạt động tình nguyện tại địa phương, giúp đỡ cộng đồng, bảo vệ môi trường hoặc hỗ trợ các tổ chức từ thiện."
            },
        new Activity
            {
                Name = "Nghệ thuật",
                Description = "Khám phá và trải nghiệm các hoạt động nghệ thuật như vẽ tranh, điêu khắc, âm nhạc, hoặc tham gia các buổi biểu diễn nghệ thuật đặc sắc."
            }
    ];
        return activities;
    }

    private void AssignRoleToUser(string username, string roleName)
    {
        var user = _context.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
        var role = _context.ApplicationRoles.FirstOrDefault(r => r.Name == roleName);

        if (user != null && role != null)
        {
            if (!_context.UserRoles.Any(ur => ur.UserId == user.Id && ur.RoleId == role.Id))
            {
                _context.UserRoles.Add(new ApplicationUserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
                _context.SaveChanges();
            }
        }
    }
}
}