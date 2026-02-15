using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TPApp.Application.Services;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Infrastructure.AI;
using TPApp.Infrastructure.Repositories;
using Xunit;

namespace TPApp.Tests.Application
{
    /// <summary>
    /// Unit tests for ProductEmbeddingService text preprocessing and truncation.
    /// </summary>
    public class ProductEmbeddingServiceTests
    {
        private readonly Mock<IEmbeddingService> _mockEmbeddingService;
        private readonly Mock<IEmbeddingRepository> _mockEmbeddingRepository;
        private readonly Mock<ILogger<ProductEmbeddingService>> _mockLogger;
        private readonly AppDbContext _context;

        public ProductEmbeddingServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
                .Options;

            _context = new AppDbContext(options);
            _mockEmbeddingService = new Mock<IEmbeddingService>();
            _mockEmbeddingRepository = new Mock<IEmbeddingRepository>();
            _mockLogger = new Mock<ILogger<ProductEmbeddingService>>();
        }

        [Fact]
        public async Task GenerateProductEmbeddingTextAsync_NormalProduct_ReturnsText()
        {
            // Arrange
            var product = new SanPhamCNTB
            {
                ID = 1,
                Name = "Test Product",
                MoTa = "This is a test description",
                Keywords = "test, product",
                NCUId = 100
            };

            _context.SanPhamCNTBs.Add(product);
            await _context.SaveChangesAsync();

            var service = new ProductEmbeddingService(
                _context,
                _mockEmbeddingService.Object,
                _mockEmbeddingRepository.Object,
                _mockLogger.Object);

            // Act
            var result = await service.GenerateProductEmbeddingTextAsync(1);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("Test Product");
            result.Should().Contain("test description");
            result.Should().Contain("test, product");
        }

        [Fact]
        public async Task GenerateProductEmbeddingTextAsync_LongText_TruncatesCorrectly()
        {
            // Arrange
            var longText = new string('A', 10000); // 10,000 characters
            var product = new SanPhamCNTB
            {
                ID = 2,
                Name = "Long Product",
                MoTa = longText,
                NCUId = 100
            };

            _context.SanPhamCNTBs.Add(product);
            await _context.SaveChangesAsync();

            var service = new ProductEmbeddingService(
                _context,
                _mockEmbeddingService.Object,
                _mockEmbeddingRepository.Object,
                _mockLogger.Object);

            // Act
            var result = await service.GenerateProductEmbeddingTextAsync(2);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Length.Should().BeLessOrEqualTo(6000); // MAX_TEXT_LENGTH
        }

        [Fact]
        public async Task GenerateProductEmbeddingTextAsync_EmptyProduct_ReturnsDefaultText()
        {
            // Arrange
            var product = new SanPhamCNTB
            {
                ID = 3,
                Name = "",
                MoTa = "",
                NCUId = 100
            };

            _context.SanPhamCNTBs.Add(product);
            await _context.SaveChangesAsync();

            var service = new ProductEmbeddingService(
                _context,
                _mockEmbeddingService.Object,
                _mockEmbeddingRepository.Object,
                _mockLogger.Object);

            // Act
            var result = await service.GenerateProductEmbeddingTextAsync(3);

            // Assert
            result.Should().Be("No description available");
        }

        [Fact]
        public async Task GenerateProductEmbeddingTextAsync_HTMLContent_RemovesTags()
        {
            // Arrange
            var product = new SanPhamCNTB
            {
                ID = 4,
                Name = "HTML Product",
                MoTa = "<p>This is <strong>bold</strong> text</p>",
                NCUId = 100
            };

            _context.SanPhamCNTBs.Add(product);
            await _context.SaveChangesAsync();

            var service = new ProductEmbeddingService(
                _context,
                _mockEmbeddingService.Object,
                _mockEmbeddingRepository.Object,
                _mockLogger.Object);

            // Act
            var result = await service.GenerateProductEmbeddingTextAsync(4);

            // Assert
            result.Should().NotContain("<p>");
            result.Should().NotContain("<strong>");
            result.Should().NotContain("</p>");
            result.Should().Contain("This is");
            result.Should().Contain("bold");
            result.Should().Contain("text");
        }

        [Fact]
        public async Task UpdateProductEmbeddingAsync_ShortText_SkipsEmbedding()
        {
            // Arrange
            var product = new SanPhamCNTB
            {
                ID = 5,
                Name = "Short",
                NCUId = 100
            };

            _context.SanPhamCNTBs.Add(product);
            await _context.SaveChangesAsync();

            var service = new ProductEmbeddingService(
                _context,
                _mockEmbeddingService.Object,
                _mockEmbeddingRepository.Object,
                _mockLogger.Object);

            // Act
            await service.UpdateProductEmbeddingAsync(5);

            // Assert
            _mockEmbeddingService.Verify(
                x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateProductEmbeddingAsync_NoSupplier_SkipsEmbedding()
        {
            // Arrange
            var product = new SanPhamCNTB
            {
                ID = 6,
                Name = "Product without supplier",
                MoTa = "This product has no supplier",
                NCUId = null
            };

            _context.SanPhamCNTBs.Add(product);
            await _context.SaveChangesAsync();

            var service = new ProductEmbeddingService(
                _context,
                _mockEmbeddingService.Object,
                _mockEmbeddingRepository.Object,
                _mockLogger.Object);

            // Act
            await service.UpdateProductEmbeddingAsync(6);

            // Assert
            _mockEmbeddingService.Verify(
                x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GenerateProductEmbeddingTextAsync_VeryLongFields_LimitsCorrectly()
        {
            // Arrange
            var product = new SanPhamCNTB
            {
                ID = 7,
                Name = "Product with long specs",
                ThongSo = new string('B', 5000), // Should be limited to 2000
                UuDiem = new string('C', 3000), // Should be limited to 1500
                NCUId = 100
            };

            _context.SanPhamCNTBs.Add(product);
            await _context.SaveChangesAsync();

            var service = new ProductEmbeddingService(
                _context,
                _mockEmbeddingService.Object,
                _mockEmbeddingRepository.Object,
                _mockLogger.Object);

            // Act
            var result = await service.GenerateProductEmbeddingTextAsync(7);

            // Assert
            result.Should().NotBeNullOrEmpty();
            // Total should be limited by MAX_TEXT_LENGTH (6000)
            result.Length.Should().BeLessOrEqualTo(6000);
        }
    }
}
