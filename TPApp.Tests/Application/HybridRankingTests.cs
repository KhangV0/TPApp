using FluentAssertions;
using TPApp.Application.Services;
using Xunit;

namespace TPApp.Tests.Application
{
    /// <summary>
    /// Unit tests for hybrid ranking score calculation.
    /// </summary>
    public class HybridRankingTests
    {
        [Fact]
        public void CalculateHybridScore_WithAllFactors_ReturnsCorrectScore()
        {
            // Arrange
            double maxProductScore = 0.9;
            double averageTop3Score = 0.85;
            int rating = 4;
            int viewed = 500;
            int maxViewed = 1000;

            // Act
            var score = AISupplierMatchingService.CalculateHybridScore(
                maxProductScore, averageTop3Score, rating, viewed, maxViewed);

            // Assert
            // Expected: 0.6*0.9 + 0.2*0.85 + 0.1*(4/5) + 0.1*(500/1000)
            // = 0.54 + 0.17 + 0.08 + 0.05 = 0.84
            score.Should().BeApproximately(0.84, 0.0001);
        }

        [Fact]
        public void CalculateHybridScore_WithZeroRating_HandlesGracefully()
        {
            // Arrange
            double maxProductScore = 1.0;
            double averageTop3Score = 0.95;
            int rating = 0;
            int viewed = 100;
            int maxViewed = 1000;

            // Act
            var score = AISupplierMatchingService.CalculateHybridScore(
                maxProductScore, averageTop3Score, rating, viewed, maxViewed);

            // Assert
            // Expected: 0.6*1.0 + 0.2*0.95 + 0.1*(0/5) + 0.1*(100/1000)
            // = 0.6 + 0.19 + 0.0 + 0.01 = 0.8
            score.Should().BeApproximately(0.8, 0.0001);
        }

        [Fact]
        public void CalculateHybridScore_WithMaxValues_ReturnsOne()
        {
            // Arrange
            double maxProductScore = 1.0;
            double averageTop3Score = 1.0;
            int rating = 5;
            int viewed = 1000;
            int maxViewed = 1000;

            // Act
            var score = AISupplierMatchingService.CalculateHybridScore(
                maxProductScore, averageTop3Score, rating, viewed, maxViewed);

            // Assert
            // Expected: 0.6*1.0 + 0.2*1.0 + 0.1*(5/5) + 0.1*(1000/1000)
            // = 0.6 + 0.2 + 0.1 + 0.1 = 1.0
            score.Should().BeApproximately(1.0, 0.0001);
        }

        [Fact]
        public void CalculateHybridScore_WithZeroViewed_HandlesGracefully()
        {
            // Arrange
            double maxProductScore = 0.8;
            double averageTop3Score = 0.75;
            int rating = 3;
            int viewed = 0;
            int maxViewed = 1000;

            // Act
            var score = AISupplierMatchingService.CalculateHybridScore(
                maxProductScore, averageTop3Score, rating, viewed, maxViewed);

            // Assert
            // Expected: 0.6*0.8 + 0.2*0.75 + 0.1*(3/5) + 0.1*(0/1000)
            // = 0.48 + 0.15 + 0.06 + 0.0 = 0.69
            score.Should().BeApproximately(0.69, 0.0001);
        }

        [Fact]
        public void CalculateHybridScore_WithZeroMaxViewed_HandlesGracefully()
        {
            // Arrange
            double maxProductScore = 0.7;
            double averageTop3Score = 0.65;
            int rating = 2;
            int viewed = 100;
            int maxViewed = 0;

            // Act
            var score = AISupplierMatchingService.CalculateHybridScore(
                maxProductScore, averageTop3Score, rating, viewed, maxViewed);

            // Assert
            // Expected: 0.6*0.7 + 0.2*0.65 + 0.1*(2/5) + 0.1*(0)
            // = 0.42 + 0.13 + 0.04 + 0.0 = 0.59
            score.Should().BeApproximately(0.59, 0.0001);
        }

        [Fact]
        public void CalculateHybridScore_SemanticScoreDominates()
        {
            // Arrange - High semantic score, low other factors
            double maxProductScore = 1.0;
            double averageTop3Score = 0.95;
            int rating = 1;
            int viewed = 10;
            int maxViewed = 1000;

            // Act
            var score = AISupplierMatchingService.CalculateHybridScore(
                maxProductScore, averageTop3Score, rating, viewed, maxViewed);

            // Assert
            // Expected: 0.6*1.0 + 0.2*0.95 + 0.1*(1/5) + 0.1*(10/1000)
            // = 0.6 + 0.19 + 0.02 + 0.001 = 0.811
            score.Should().BeApproximately(0.811, 0.001);
            score.Should().BeGreaterThan(0.8); // Semantic score (60%) dominates
        }
    }
}
