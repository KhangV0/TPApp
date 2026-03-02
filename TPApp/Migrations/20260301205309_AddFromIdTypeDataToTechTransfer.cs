using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFromIdTypeDataToTechTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcceptanceReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    EContractId = table.Column<int>(type: "int", nullable: true),
                    NgayNghiemThu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThanhPhanThamGia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KetLuanNghiemThu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VanDeTonDong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChuKyBenA = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ChuKyBenB = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TrangThaiKy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcceptanceReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdvancePaymentConfirmations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EContractId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    SoTienTamUng = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChungTuChuyenTienFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayChuyen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaXacNhanNhanTien = table.Column<bool>(type: "bit", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancePaymentConfirmations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AISearchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryText = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResultCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AISearchLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Album",
                columns: table => new
                {
                    AlbumID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContensID = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Album", x => x.AlbumID);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CatId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TitleEn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    ParentIdEN = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    Viewed = table.Column<double>(type: "float", nullable: true),
                    Sort = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoURL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MainCate = table.Column<bool>(type: "bit", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CatId);
                });

            migrationBuilder.CreateTable(
                name: "ChatConversations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ProductType = table.Column<int>(type: "int", nullable: true),
                    BuyerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SupplierUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsFromProductDetail = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommentsYCTB",
                columns: table => new
                {
                    CommentYCTBId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    TargetId = table.Column<long>(type: "bigint", nullable: false),
                    CommentTypeId = table.Column<byte>(type: "tinyint", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contents = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<byte>(type: "tinyint", nullable: true),
                    UrlRefer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Like = table.Column<int>(type: "int", nullable: true),
                    Share = table.Column<int>(type: "int", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentsYCTB", x => x.CommentYCTBId);
                });

            migrationBuilder.CreateTable(
                name: "Contents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChuongTrinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhieuDangKy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STINFO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhSTINFO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    MenuId = table.Column<int>(type: "int", nullable: true),
                    IsHot = table.Column<bool>(type: "bit", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "varchar(300)", nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    bEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    eEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsNew = table.Column<bool>(type: "bit", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Keyword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Like = table.Column<int>(type: "int", nullable: true),
                    DisLike = table.Column<int>(type: "int", nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsYoutube = table.Column<bool>(type: "bit", nullable: true),
                    ImageBig = table.Column<string>(type: "varchar(300)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkInvite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DemoDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReport = table.Column<bool>(type: "bit", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentsYeuCau",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChuongTrinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhieuDangKy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STINFO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhSTINFO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinhVucId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    MenuId = table.Column<int>(type: "int", nullable: true),
                    IsHot = table.Column<bool>(type: "bit", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    bEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    eEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsNew = table.Column<bool>(type: "bit", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Keyword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Like = table.Column<int>(type: "int", nullable: true),
                    DisLike = table.Column<int>(type: "int", nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsYoutube = table.Column<bool>(type: "bit", nullable: true),
                    ImageBig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkInvite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DemoDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReport = table.Column<bool>(type: "bit", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    UserLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentsYeuCau", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DecisionAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractApprovals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    LegalReviewFormId = table.Column<int>(type: "int", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommentType = table.Column<int>(type: "int", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractSignatureRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    SignatureType = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    RequestRef = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChallengeRef = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CallbackSecret = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSignatureRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractSignatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    SignatureRequestId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    SignatureType = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CertificateSerial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CertificateSubject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CertificateIssuer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SignedHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeStampToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    RawProviderPayload = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSignatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DashboardMonthlyStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    NewProducts = table.Column<int>(type: "int", nullable: false),
                    NewProjects = table.Column<int>(type: "int", nullable: false),
                    NewSuppliers = table.Column<int>(type: "int", nullable: false),
                    NewConsultants = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardMonthlyStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DashboardSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalProducts = table.Column<int>(type: "int", nullable: false),
                    CongNgheCount = table.Column<int>(type: "int", nullable: false),
                    ThietBiCount = table.Column<int>(type: "int", nullable: false),
                    TriTueCount = table.Column<int>(type: "int", nullable: false),
                    TotalProjects = table.Column<int>(type: "int", nullable: false),
                    ActiveProjects = table.Column<int>(type: "int", nullable: false),
                    CompletedProjects = table.Column<int>(type: "int", nullable: false),
                    TotalSuppliers = table.Column<int>(type: "int", nullable: false),
                    ActiveSuppliers = table.Column<int>(type: "int", nullable: false),
                    TotalConsultants = table.Column<int>(type: "int", nullable: false),
                    ActiveConsultants = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardSnapshot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RFQId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    SoHopDong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileHopDong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiKyBenA = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NguoiKyBenB = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThaiKy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityActionLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityActionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityRatings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityRatings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityViewCounters",
                columns: table => new
                {
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityViewCounters", x => new { x.EntityType, x.EntityId });
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    bEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    eEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    DeptId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ForumYCDV",
                columns: table => new
                {
                    ForumYCDVId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenDonVi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinhVucId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DichVuId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActivated = table.Column<bool>(type: "bit", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: true),
                    Like = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<int>(type: "int", nullable: true),
                    ShareFB = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumYCDV", x => x.ForumYCDVId);
                });

            migrationBuilder.CreateTable(
                name: "ForumYCTB",
                columns: table => new
                {
                    ForumYCTBId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenDonVi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActivated = table.Column<bool>(type: "bit", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: true),
                    Like = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<int>(type: "int", nullable: true),
                    ShareFB = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumYCTB", x => x.ForumYCTBId);
                });

            migrationBuilder.CreateTable(
                name: "HandoverReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    EContractId = table.Column<int>(type: "int", nullable: true),
                    DanhMucThietBiJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DanhMucHoSoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaHoanThanhDaoTao = table.Column<bool>(type: "bit", nullable: false),
                    DanhGiaSao = table.Column<int>(type: "int", nullable: true),
                    NhanXet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandoverReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImagesAdver",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SRC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<int>(type: "int", nullable: true),
                    StatusID = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sort = table.Column<int>(type: "int", nullable: true),
                    LanguageID = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagesAdver", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ImplementationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    EContractId = table.Column<int>(type: "int", nullable: true),
                    GiaiDoan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KetQuaThucHien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnhVideoFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BienBanXacNhanFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImplementationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Keyword",
                columns: table => new
                {
                    KeywordID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Keyword = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keyword", x => x.KeywordID);
                });

            migrationBuilder.CreateTable(
                name: "KeywordLienKet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeywordId = table.Column<long>(type: "bigint", nullable: true),
                    TargetId = table.Column<long>(type: "bigint", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    Tittle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Img = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ref1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ref2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ref3 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Reft4 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ref5 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: true),
                    Sort = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeywordLienKet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegalReviewForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    NegotiationFormId = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    HtmlSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiKiemTra = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KetQuaKiemTra = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VanDePhapLy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeXuatChinhSua = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileKiemTra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaDuyet = table.Column<bool>(type: "bit", nullable: false),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    NgayKiemTra = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalReviewForms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Likepage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPage = table.Column<int>(type: "int", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeID = table.Column<int>(type: "int", nullable: true),
                    Like = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likepage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LiquidationReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    EContractId = table.Column<int>(type: "int", nullable: true),
                    GiaTriThanhToanConLai = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoHoaDon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HoaDonFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SanDaChuyenTien = table.Column<bool>(type: "bit", nullable: false),
                    HopDongClosed = table.Column<bool>(type: "bit", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidationReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FunctionID = table.Column<int>(type: "int", nullable: true),
                    ActTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventID = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sort = table.Column<int>(type: "int", nullable: true),
                    MenuPosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<byte>(type: "tinyint", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    eEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NavigateUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowRight = table.Column<byte>(type: "tinyint", nullable: true),
                    TitlePage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    ImageLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.MenuId);
                });

            migrationBuilder.CreateTable(
                name: "MucDo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MucDo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NDAAgreements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BenA = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BenB = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LoaiNDA = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ThoiHanBaoMat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    XacNhanKySo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    DaDongY = table.Column<bool>(type: "bit", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NDAAgreements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NegotiationForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RFQId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    GiaChotCuoiCung = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DieuKhoanThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BienBanThuongLuongFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhThucKy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DaKySo = table.Column<bool>(type: "bit", nullable: false),
                    SellerSigned = table.Column<bool>(type: "bit", nullable: false),
                    BuyerSigned = table.Column<bool>(type: "bit", nullable: false),
                    SellerOtpCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SellerOtpExpire = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BuyerOtpCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BuyerOtpExpire = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SellerSignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BuyerSignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NegotiationForms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NhaCungUng",
                columns: table => new
                {
                    CungUngId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HinhDaiDien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NguoiDaiDien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LinhVucId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChucNangChinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DichVu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SanPham = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IsActivated = table.Column<bool>(type: "bit", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true),
                    TenVietTat = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LoaiHinhToChuc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaSoThue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChungNhan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaCungUng", x => x.CungUngId);
                });

            migrationBuilder.CreateTable(
                name: "NhaTuVan",
                columns: table => new
                {
                    TuVanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QueryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HocHam = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoQuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinhVucId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DichVu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KetQuaNghienCuu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IsActivated = table.Column<bool>(type: "bit", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true),
                    MaDinhDanh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TongTrichDan = table.Column<int>(type: "int", nullable: true),
                    HIndex = table.Column<int>(type: "int", nullable: true),
                    QuaTrinhDaoTao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuaTrinhCongTac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CongBoKhoaHoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SangChe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuAnNghienCuu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KinhNghiem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoSoDinhKem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HiepHoiKhoaHoc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaTuVan", x => x.TuVanId);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProviderResponse = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastTriedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhieuYeuCauCNTB",
                columns: table => new
                {
                    PhieuYeuCauId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenDonVi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActivated = table.Column<bool>(type: "bit", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: true),
                    Like = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<int>(type: "int", nullable: true),
                    ShareFB = table.Column<int>(type: "int", nullable: true),
                    Ngayyeucau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuYeuCauCNTB", x => x.PhieuYeuCauId);
                });

            migrationBuilder.CreateTable(
                name: "PilotTestReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    EContractId = table.Column<int>(type: "int", nullable: true),
                    MoTaThuNghiem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KetQuaThuNghiem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VanDePhatSinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaiPhap = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileKetQua = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileBaoCao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatYeuCau = table.Column<bool>(type: "bit", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PilotTestReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortletHoiNhieu",
                columns: table => new
                {
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    COUNTYCTB = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "PortletTraLoiNhieu",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    COUNTYCTB = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ProjectContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    SignedFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SignedFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    Sha256Original = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Sha256Signed = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ReadyToSignAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SelectedSellerId = table.Column<int>(type: "int", nullable: true),
                    SelectedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSteps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rating",
                columns: table => new
                {
                    RatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanPhamId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Contents = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    UrlRefer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RatingValue = table.Column<int>(type: "int", nullable: true),
                    TypeID = table.Column<int>(type: "int", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rating", x => x.RatingId);
                });

            migrationBuilder.CreateTable(
                name: "RFQRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaRFQ = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    YeuCauKyThuat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TieuChuanNghiemThu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HanChotNopHoSo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    DaGuiNhaCungUng = table.Column<bool>(type: "bit", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFQRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "RootSite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    SiteName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DomainURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourcePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiLienHeId = table.Column<int>(type: "int", nullable: true),
                    ThongTinLienHe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootSite", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SanPhamCNTB",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuyTrinhHinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsYoutube = table.Column<bool>(type: "bit", nullable: true),
                    XuatXuId = table.Column<int>(type: "int", nullable: true),
                    MucDoId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThongSo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UuDiem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalPrice = table.Column<double>(type: "float", nullable: true),
                    SellPrice = table.Column<double>(type: "float", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaiThuong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerType = table.Column<int>(type: "int", nullable: true),
                    OwnerEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NCUId = table.Column<int>(type: "int", nullable: true),
                    Khachhang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StoreId = table.Column<int>(type: "int", nullable: true),
                    IsSellOff = table.Column<bool>(type: "bit", nullable: true),
                    IsHot = table.Column<bool>(type: "bit", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaBan = table.Column<int>(type: "int", nullable: true),
                    TinhTrangHang = table.Column<int>(type: "int", nullable: true),
                    TongSo = table.Column<int>(type: "int", nullable: true),
                    XuatXu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TinhTP = table.Column<int>(type: "int", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneOther = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YahooId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkypeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    eEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    SoBang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayCapBang = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiHan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoQuanChuTri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoQuanChuQuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiDeTai = table.Column<int>(type: "int", nullable: true),
                    LoaiDeTaiKhac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    MoTaNgan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true),
                    TRLLevel = table.Column<int>(type: "int", nullable: true),
                    TransferMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferMethodKhac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetCustomer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClaimsCount = table.Column<int>(type: "int", nullable: true),
                    DevelopmentStage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CooperationGoal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CooperationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaBanDuKien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChiPhiPhatSinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaoHanhHoTro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrochureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChungNhanISO = table.Column<bool>(type: "bit", nullable: true),
                    ChungNhanQuatest = table.Column<bool>(type: "bit", nullable: true),
                    ChungNhanKhac = table.Column<bool>(type: "bit", nullable: true),
                    ChungNhanKhacText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DevelopmentStageValue = table.Column<int>(type: "int", nullable: true),
                    InvestmentGoal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestmentGoalKhac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamCNTB", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SanPhamCNTBCategory",
                columns: table => new
                {
                    SanPhamCNTBId = table.Column<int>(type: "int", nullable: false),
                    CatId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamCNTBCategory", x => new { x.SanPhamCNTBId, x.CatId });
                });

            migrationBuilder.CreateTable(
                name: "SanPhamEmbeddings",
                columns: table => new
                {
                    SanPhamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NCUId = table.Column<int>(type: "int", nullable: false),
                    Embedding = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamEmbeddings", x => x.SanPhamId);
                });

            migrationBuilder.CreateTable(
                name: "SearchIndexContents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FutherIndex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemovedUnicode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefId = table.Column<long>(type: "bigint", nullable: true),
                    ImgPreview = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    URL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AbsPath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IndexTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Noted = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LanguageId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchIndexContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchQueryLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Keyword = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NormalizedKeyword = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ResultCount = table.Column<int>(type: "int", nullable: false),
                    SearchMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LanguageId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExecutionTimeMs = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchQueryLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCart",
                columns: table => new
                {
                    RecordId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    StoreId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCart", x => x.RecordId);
                });

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "StepPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    RoleType = table.Column<int>(type: "int", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanSubmit = table.Column<bool>(type: "bit", nullable: false),
                    CanApprove = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Store",
                columns: table => new
                {
                    StoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slogan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactUs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    URLWEB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImgLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateId = table.Column<int>(type: "int", nullable: true),
                    StoreTypeId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Viewed = table.Column<float>(type: "real", nullable: true),
                    Mark = table.Column<float>(type: "real", nullable: true),
                    TotalVote = table.Column<int>(type: "int", nullable: true),
                    IsHot = table.Column<bool>(type: "bit", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    bEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    eEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BigBanner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageSlider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YahooId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkypeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TinhThanhId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneOther = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsNewCar = table.Column<bool>(type: "bit", nullable: true),
                    IsOldCar = table.Column<bool>(type: "bit", nullable: true),
                    IsVip = table.Column<bool>(type: "bit", nullable: true),
                    HeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Map = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DailyOrder = table.Column<int>(type: "int", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Store", x => x.StoreId);
                });

            migrationBuilder.CreateTable(
                name: "SubMenuIds",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SYS_PARAMETERS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Val = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Val2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: true),
                    Activated = table.Column<bool>(type: "bit", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYS_PARAMETERS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TechnicalDocHandovers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    EContractId = table.Column<int>(type: "int", nullable: true),
                    DanhMucHoSo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaiLieuKyThuat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaiLieuHuongDanSuDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaiLieuBaoTri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Database = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaBanGiaoDayDu = table.Column<bool>(type: "bit", nullable: false),
                    NgayBanGiao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalDocHandovers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TechTransferRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChucVu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DonVi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenCongNghe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTaNhuCau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinhVuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NganSachDuKien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FromId = table.Column<int>(type: "int", nullable: true),
                    TypeData = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechTransferRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimKiemDoiTac",
                columns: table => new
                {
                    TimDoiTacId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HinhDaiDien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenDonVi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SanPhamId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenSanPham = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhThuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IsActivated = table.Column<bool>(type: "bit", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Viewed = table.Column<int>(type: "int", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimKiemDoiTac", x => x.TimDoiTacId);
                });

            migrationBuilder.CreateTable(
                name: "TrainingHandovers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    EContractId = table.Column<int>(type: "int", nullable: true),
                    NoiDungDaoTao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DanhSachNguoiThamGia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoNguoiThamGia = table.Column<int>(type: "int", nullable: true),
                    SoGioDaoTao = table.Column<int>(type: "int", nullable: true),
                    TaiLieuDaoTao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoHuongDan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BienBanDaoTao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaHoanThanh = table.Column<bool>(type: "bit", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingHandovers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsUser = table.Column<byte>(type: "tinyint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActivated = table.Column<bool>(type: "bit", nullable: true),
                    IsShowHome = table.Column<bool>(type: "bit", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserTypeId = table.Column<int>(type: "int", nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: true),
                    TinhTP = table.Column<int>(type: "int", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileOrther = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiHan = table.Column<int>(type: "int", nullable: true),
                    TongPhi = table.Column<int>(type: "int", nullable: true),
                    Vip = table.Column<int>(type: "int", nullable: true),
                    bActiveVip = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApproveVip = table.Column<bool>(type: "bit", nullable: true),
                    DatePost = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateUp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumberPost = table.Column<int>(type: "int", nullable: true),
                    NumberUp = table.Column<int>(type: "int", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true),
                    PhoneVerified = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    AccountTypeId = table.Column<int>(type: "int", nullable: false),
                    VerificationLevel = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "vAccountType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vAccountType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VSImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    ContentId = table.Column<long>(type: "bigint", nullable: true),
                    FunctionId = table.Column<int>(type: "int", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VSImage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XuatXu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XuatXu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<long>(type: "bigint", nullable: false),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ChatConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ESignDocuments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<int>(type: "int", nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESignDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ESignDocuments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStepStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataRefTable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataRefId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStepStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectStepStates_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectWorkflowStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    OverallStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectWorkflowStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectWorkflowStates_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitionLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    FromStep = table.Column<int>(type: "int", nullable: true),
                    ToStep = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitionLogs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectAccessLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAccessLogs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectAccessLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectConsultants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectConsultants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectConsultants_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectConsultants_Users_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProposalSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RFQId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    GiaiPhapKyThuat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaoGiaSoBo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ThoiGianTrienKhai = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HoSoNangLucDinhKem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiSua = table.Column<int>(type: "int", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalSubmissions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProposalSubmissions_Users_NguoiTao",
                        column: x => x.NguoiTao,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "RFQInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    RFQId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    InvitedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ViewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFQInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFQInvitations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RFQInvitations_RFQRequests_RFQId",
                        column: x => x.RFQId,
                        principalTable: "RFQRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RFQInvitations_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOtps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OtpType = table.Column<int>(type: "int", nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOtps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOtps_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVerificationDocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewStatus = table.Column<int>(type: "int", nullable: false),
                    ReviewNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVerificationDocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVerificationDocs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ESignAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESignAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ESignAuditLogs_ESignDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "ESignDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ESignSignatures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    SignerUserId = table.Column<int>(type: "int", nullable: false),
                    SignerRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SignatureHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    OtpCodeHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OtpSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OtpVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESignSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ESignSignatures_ESignDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "ESignDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProposalScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProposalId = table.Column<int>(type: "int", nullable: false),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    TechnicalScore = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    PriceScore = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    TimelineScore = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    OverallScore = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalScores_ProposalSubmissions_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "ProposalSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProposalScores_Users_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_ChatConversation_Product_Buyer_Supplier",
                table: "ChatConversations",
                columns: new[] { "ProductId", "BuyerUserId", "SupplierUserId" },
                unique: true,
                filter: "[ProductId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ConversationId",
                table: "ChatMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "UX_EntityRatings_User_Entity",
                table: "EntityRatings",
                columns: new[] { "UserId", "EntityType", "EntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ESignAuditLogs_DocumentId",
                table: "ESignAuditLogs",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignDocuments_ProjectId",
                table: "ESignDocuments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ESignSignatures_DocumentId",
                table: "ESignSignatures",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAccessLogs_ProjectId",
                table: "ProjectAccessLogs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAccessLogs_UserId",
                table: "ProjectAccessLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectConsultants_ConsultantId",
                table: "ProjectConsultants",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectConsultants_ProjectId",
                table: "ProjectConsultants",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_ProjectId",
                table: "ProjectMembers",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStepStates_ProjectId",
                table: "ProjectStepStates",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectWorkflowStates_ProjectId",
                table: "ProjectWorkflowStates",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalScores_ConsultantId",
                table: "ProposalScores",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalScores_ProposalId",
                table: "ProposalScores",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalSubmissions_NguoiTao",
                table: "ProposalSubmissions",
                column: "NguoiTao");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalSubmissions_ProjectId",
                table: "ProposalSubmissions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQInvitations_ProjectId",
                table: "RFQInvitations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQInvitations_RFQId",
                table: "RFQInvitations",
                column: "RFQId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQInvitations_SellerId",
                table: "RFQInvitations",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOtps_UserId",
                table: "UserOtps",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVerificationDocs_UserId",
                table: "UserVerificationDocs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitionLogs_ProjectId",
                table: "WorkflowTransitionLogs",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcceptanceReports");

            migrationBuilder.DropTable(
                name: "AdvancePaymentConfirmations");

            migrationBuilder.DropTable(
                name: "AISearchLogs");

            migrationBuilder.DropTable(
                name: "Album");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "CommentsYCTB");

            migrationBuilder.DropTable(
                name: "Contents");

            migrationBuilder.DropTable(
                name: "ContentsYeuCau");

            migrationBuilder.DropTable(
                name: "ContractApprovals");

            migrationBuilder.DropTable(
                name: "ContractAuditLogs");

            migrationBuilder.DropTable(
                name: "ContractComments");

            migrationBuilder.DropTable(
                name: "ContractSignatureRequests");

            migrationBuilder.DropTable(
                name: "ContractSignatures");

            migrationBuilder.DropTable(
                name: "DashboardMonthlyStats");

            migrationBuilder.DropTable(
                name: "DashboardSnapshot");

            migrationBuilder.DropTable(
                name: "EContracts");

            migrationBuilder.DropTable(
                name: "EntityActionLogs");

            migrationBuilder.DropTable(
                name: "EntityRatings");

            migrationBuilder.DropTable(
                name: "EntityViewCounters");

            migrationBuilder.DropTable(
                name: "ESignAuditLogs");

            migrationBuilder.DropTable(
                name: "ESignSignatures");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "ForumYCDV");

            migrationBuilder.DropTable(
                name: "ForumYCTB");

            migrationBuilder.DropTable(
                name: "HandoverReports");

            migrationBuilder.DropTable(
                name: "ImagesAdver");

            migrationBuilder.DropTable(
                name: "ImplementationLogs");

            migrationBuilder.DropTable(
                name: "Keyword");

            migrationBuilder.DropTable(
                name: "KeywordLienKet");

            migrationBuilder.DropTable(
                name: "LegalReviewForms");

            migrationBuilder.DropTable(
                name: "Likepage");

            migrationBuilder.DropTable(
                name: "LiquidationReports");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "MucDo");

            migrationBuilder.DropTable(
                name: "NDAAgreements");

            migrationBuilder.DropTable(
                name: "NegotiationForms");

            migrationBuilder.DropTable(
                name: "NhaCungUng");

            migrationBuilder.DropTable(
                name: "NhaTuVan");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PhieuYeuCauCNTB");

            migrationBuilder.DropTable(
                name: "PilotTestReports");

            migrationBuilder.DropTable(
                name: "PortletHoiNhieu");

            migrationBuilder.DropTable(
                name: "PortletTraLoiNhieu");

            migrationBuilder.DropTable(
                name: "ProjectAccessLogs");

            migrationBuilder.DropTable(
                name: "ProjectConsultants");

            migrationBuilder.DropTable(
                name: "ProjectContracts");

            migrationBuilder.DropTable(
                name: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "ProjectSteps");

            migrationBuilder.DropTable(
                name: "ProjectStepStates");

            migrationBuilder.DropTable(
                name: "ProjectWorkflowStates");

            migrationBuilder.DropTable(
                name: "ProposalScores");

            migrationBuilder.DropTable(
                name: "Rating");

            migrationBuilder.DropTable(
                name: "RFQInvitations");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "RootSite");

            migrationBuilder.DropTable(
                name: "SanPhamCNTB");

            migrationBuilder.DropTable(
                name: "SanPhamCNTBCategory");

            migrationBuilder.DropTable(
                name: "SanPhamEmbeddings");

            migrationBuilder.DropTable(
                name: "SearchIndexContents");

            migrationBuilder.DropTable(
                name: "SearchQueryLog");

            migrationBuilder.DropTable(
                name: "ShoppingCart");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropTable(
                name: "StepPermissions");

            migrationBuilder.DropTable(
                name: "Store");

            migrationBuilder.DropTable(
                name: "SubMenuIds");

            migrationBuilder.DropTable(
                name: "SYS_PARAMETERS");

            migrationBuilder.DropTable(
                name: "TechnicalDocHandovers");

            migrationBuilder.DropTable(
                name: "TechTransferRequests");

            migrationBuilder.DropTable(
                name: "TimKiemDoiTac");

            migrationBuilder.DropTable(
                name: "TrainingHandovers");

            migrationBuilder.DropTable(
                name: "UserOtps");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "UserVerificationDocs");

            migrationBuilder.DropTable(
                name: "vAccountType");

            migrationBuilder.DropTable(
                name: "VSImage");

            migrationBuilder.DropTable(
                name: "WorkflowTransitionLogs");

            migrationBuilder.DropTable(
                name: "XuatXu");

            migrationBuilder.DropTable(
                name: "ChatConversations");

            migrationBuilder.DropTable(
                name: "ESignDocuments");

            migrationBuilder.DropTable(
                name: "ProposalSubmissions");

            migrationBuilder.DropTable(
                name: "RFQRequests");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
