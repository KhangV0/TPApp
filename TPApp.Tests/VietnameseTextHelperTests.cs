using TPApp.Helpers;

namespace TPApp.Tests
{
    public class VietnameseTextHelperTests
    {
        [Fact]
        public void NormalizeKeyword_LowercaseNoAccents_ReturnsAccented()
        {
            // Arrange
            var input = "may ozone";
            
            // Act
            var result = VietnameseTextHelper.NormalizeKeyword(input);
            
            // Assert
            Assert.Equal("máy ozone", result);
        }

        [Fact]
        public void NormalizeKeyword_UppercaseNoAccents_ReturnsAccented()
        {
            // Arrange
            var input = "MAY OZONE";
            
            // Act
            var result = VietnameseTextHelper.NormalizeKeyword(input);
            
            // Assert
            Assert.Equal("máy ozone", result);
        }

        [Fact]
        public void NormalizeKeyword_MixedCase_ReturnsAccented()
        {
            // Arrange
            var input = "May Ozone";
            
            // Act
            var result = VietnameseTextHelper.NormalizeKeyword(input);
            
            // Assert
            Assert.Equal("máy ozone", result);
        }

        [Fact]
        public void NormalizeKeyword_AlreadyAccented_RemainsUnchanged()
        {
            // Arrange
            var input = "máy ozone";
            
            // Act
            var result = VietnameseTextHelper.NormalizeKeyword(input);
            
            // Assert
            Assert.Equal("máy ozone", result);
        }

        [Fact]
        public void NormalizeKeyword_MultipleWords_AllNormalized()
        {
            // Arrange
            var input = "may do nuoc";
            
            // Act
            var result = VietnameseTextHelper.NormalizeKeyword(input);
            
            // Assert
            Assert.Equal("máy đo nước", result);
        }
    }
}
