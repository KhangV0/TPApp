using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace TPApp.Tests.Application
{
    /// <summary>
    /// Unit tests for supplier grouping and product selection logic.
    /// </summary>
    public class SupplierGroupingTests
    {
        [Fact]
        public void GroupBySupplier_MultipleProducts_GroupsCorrectly()
        {
            // Arrange
            var products = new List<(int ProductId, int SupplierId, double Score)>
            {
                (1, 100, 0.95),
                (2, 100, 0.90),
                (3, 200, 0.85),
                (4, 100, 0.80),
                (5, 200, 0.75)
            };

            // Act
            var grouped = products
                .GroupBy(p => p.SupplierId)
                .Select(g => new
                {
                    SupplierId = g.Key,
                    ProductCount = g.Count(),
                    MaxScore = g.Max(p => p.Score)
                })
                .ToList();

            // Assert
            grouped.Should().HaveCount(2);
            
            var supplier100 = grouped.First(g => g.SupplierId == 100);
            supplier100.ProductCount.Should().Be(3);
            supplier100.MaxScore.Should().Be(0.95);

            var supplier200 = grouped.First(g => g.SupplierId == 200);
            supplier200.ProductCount.Should().Be(2);
            supplier200.MaxScore.Should().Be(0.85);
        }

        [Fact]
        public void GroupBySupplier_TakesTop3Products_PerSupplier()
        {
            // Arrange
            var products = new List<(int ProductId, int SupplierId, double Score)>
            {
                (1, 100, 0.95),
                (2, 100, 0.90),
                (3, 100, 0.85),
                (4, 100, 0.80),
                (5, 100, 0.75)
            };

            // Act
            var topProducts = products
                .GroupBy(p => p.SupplierId)
                .SelectMany(g => g.OrderByDescending(p => p.Score).Take(3))
                .ToList();

            // Assert
            topProducts.Should().HaveCount(3);
            topProducts[0].ProductId.Should().Be(1);
            topProducts[0].Score.Should().Be(0.95);
            topProducts[1].ProductId.Should().Be(2);
            topProducts[1].Score.Should().Be(0.90);
            topProducts[2].ProductId.Should().Be(3);
            topProducts[2].Score.Should().Be(0.85);
        }

        [Fact]
        public void GroupBySupplier_CalculatesAverageScore_Correctly()
        {
            // Arrange
            var products = new List<(int ProductId, int SupplierId, double Score)>
            {
                (1, 100, 0.90),
                (2, 100, 0.80),
                (3, 100, 0.70)
            };

            // Act
            var avgScore = products
                .GroupBy(p => p.SupplierId)
                .Select(g => g.OrderByDescending(p => p.Score).Take(3).Average(p => p.Score))
                .First();

            // Assert
            // Expected: (0.90 + 0.80 + 0.70) / 3 = 0.80
            avgScore.Should().BeApproximately(0.80, 0.0001);
        }

        [Fact]
        public void GroupBySupplier_WithFewerThan3Products_TakesAll()
        {
            // Arrange
            var products = new List<(int ProductId, int SupplierId, double Score)>
            {
                (1, 100, 0.95),
                (2, 100, 0.85)
            };

            // Act
            var topProducts = products
                .GroupBy(p => p.SupplierId)
                .SelectMany(g => g.OrderByDescending(p => p.Score).Take(3))
                .ToList();

            // Assert
            topProducts.Should().HaveCount(2);
        }

        [Fact]
        public void GroupBySupplier_OrdersByScoreDescending()
        {
            // Arrange
            var products = new List<(int ProductId, int SupplierId, double Score)>
            {
                (1, 100, 0.70),
                (2, 100, 0.95),
                (3, 100, 0.85)
            };

            // Act
            var topProducts = products
                .GroupBy(p => p.SupplierId)
                .SelectMany(g => g.OrderByDescending(p => p.Score).Take(3))
                .ToList();

            // Assert
            topProducts[0].Score.Should().Be(0.95);
            topProducts[1].Score.Should().Be(0.85);
            topProducts[2].Score.Should().Be(0.70);
        }
    }
}
