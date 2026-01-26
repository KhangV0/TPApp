using Microsoft.EntityFrameworkCore;
using TPApp.Entities;

namespace TPApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public List<int> UspSelectSubMenu(int menuId)
        {
            return SubMenuIds
                .FromSqlInterpolated($"EXEC uspSelectSubMenu {menuId}")
                .AsEnumerable()              
                .Select(x => x.MenuId)       
                .ToList();
        }

        public DbSet<SubMenuIdDto> SubMenuIds { get; set; }

        public DbSet<SanPhamCNTB> SanPhamCNTBs { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<ContentsYeuCau> ContentsYeuCaus { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<KeywordEntity> KeywordEntities { get; set; }
        public DbSet<KeywordLienKet> KeywordLienKets { get; set; }
        public DbSet<NhaCungUng> NhaCungUngs { get; set; }
        public DbSet<VSImage> VSImages { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ForumYCTB> ForumYCTBs { get; set; }
        public DbSet<ForumYCDV> ForumYCDVs { get; set; }
        public DbSet<CommentsYCTB> CommentsYCTBs { get; set; }
        public DbSet<uspPortletCountTichcuu_Result> PortletHoiNhieu { get; set; }
        public DbSet<uspPortletCountTichcuuTraloi_Result> PortletTraLoiNhieu { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SubMenuIdDto>().HasNoKey();

            modelBuilder.Entity<uspPortletCountTichcuu_Result>().HasNoKey();
            modelBuilder.Entity<uspPortletCountTichcuuTraloi_Result>().HasNoKey();
        }

        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PhieuYeuCauCNTB> PhieuYeuCauCNTBs { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<TimKiemDoiTac> TimKiemDoiTacs { get; set; }
        public DbSet<NhaTuVan> NhaTuVans { get; set; }
        public DbSet<ImageAdver> ImageAdvers { get; set; }





    }
}
