using FluentAssertions;
using TPApp.Application.Services;
using Xunit;

namespace TPApp.Tests.Application
{
    /// <summary>
    /// Unit tests for cosine similarity calculation.
    /// </summary>
    public class CosineSimilarityTests
    {
        [Fact]
        public void IdenticalVectors_ReturnsOne()
        {
            // Arrange
            var vector1 = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };
            var vector2 = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };

            // Act
            var similarity = AISupplierMatchingService.CalculateCosineSimilarity(vector1, vector2);

            // Assert
            similarity.Should().BeApproximately(1.0, 0.0001);
        }

        [Fact]
        public void OrthogonalVectors_ReturnsZero()
        {
            // Arrange
            var vector1 = new float[] { 1.0f, 0.0f, 0.0f };
            var vector2 = new float[] { 0.0f, 1.0f, 0.0f };

            // Act
            var similarity = AISupplierMatchingService.CalculateCosineSimilarity(vector1, vector2);

            // Assert
            similarity.Should().BeApproximately(0.0, 0.0001);
        }

        [Fact]
        public void NullVectors_ReturnsZero()
        {
            // Arrange
            float[]? vector1 = null;
            float[]? vector2 = null;

            // Act
            var similarity = AISupplierMatchingService.CalculateCosineSimilarity(vector1!, vector2!);

            // Assert
            similarity.Should().Be(0.0);
        }

        [Fact]
        public void DifferentLengthVectors_ReturnsZero()
        {
            // Arrange
            var vector1 = new float[] { 1.0f, 2.0f, 3.0f };
            var vector2 = new float[] { 1.0f, 2.0f };

            // Act
            var similarity = AISupplierMatchingService.CalculateCosineSimilarity(vector1, vector2);

            // Assert
            similarity.Should().Be(0.0);
        }

        [Fact]
        public void TypicalVectors_ReturnsExpectedSimilarity()
        {
            // Arrange
            var vector1 = new float[] { 1.0f, 2.0f, 3.0f };
            var vector2 = new float[] { 2.0f, 3.0f, 4.0f };

            // Act
            var similarity = AISupplierMatchingService.CalculateCosineSimilarity(vector1, vector2);

            // Assert
            // Expected: (1*2 + 2*3 + 3*4) / (sqrt(1+4+9) * sqrt(4+9+16))
            // = (2 + 6 + 12) / (sqrt(14) * sqrt(29))
            // = 20 / (3.742 * 5.385) = 20 / 20.15 ≈ 0.9925
            similarity.Should().BeApproximately(0.9925, 0.001);
        }

        [Fact]
        public void ZeroMagnitudeVector_ReturnsZero()
        {
            // Arrange
            var vector1 = new float[] { 0.0f, 0.0f, 0.0f };
            var vector2 = new float[] { 1.0f, 2.0f, 3.0f };

            // Act
            var similarity = AISupplierMatchingService.CalculateCosineSimilarity(vector1, vector2);

            // Assert
            similarity.Should().Be(0.0);
        }

        [Fact]
        public void OppositeVectors_ReturnsNegativeOne()
        {
            // Arrange
            var vector1 = new float[] { 1.0f, 2.0f, 3.0f };
            var vector2 = new float[] { -1.0f, -2.0f, -3.0f };

            // Act
            var similarity = AISupplierMatchingService.CalculateCosineSimilarity(vector1, vector2);

            // Assert
            similarity.Should().BeApproximately(-1.0, 0.0001);
        }
    }
}
