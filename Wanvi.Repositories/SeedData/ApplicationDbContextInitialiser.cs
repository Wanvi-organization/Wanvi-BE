using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
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

            data = _context.Cities.Count();
            if (data is 0)
            {
                City[] cities = CreateCities();
                _context.AddRange(cities);

                District[] districts = CreateDistricts(cities);
                _context.AddRange(districts);
            }

            _context.SaveChanges();
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

    }
}
