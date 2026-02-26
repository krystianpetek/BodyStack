using BodyStack.Server.Domain.Services;
using Xunit;

namespace BodyStack.Server.Tests.Domain.Services;

public class BmrCalculatorTests
{
    private readonly BmrCalculator _calculator = new();

    [Theory]
    // BMR = (10 × weight) + (6.25 × height) - (5 × age) + 5 (male) / -161 (female)
    [InlineData(70, 175, 30, "male", 1649)]    // 700 + 1093.75 - 150 + 5 = 1648.75
    [InlineData(60, 165, 25, "female", 1345)]  // 600 + 1031.25 - 125 - 161 = 1345.25
    [InlineData(80, 180, 40, "male", 1730)]    // 800 + 1125 - 200 + 5 = 1730
    [InlineData(55, 160, 35, "female", 1214)]  // 550 + 1000 - 175 - 161 = 1214
    public void CalculateBmr_WithValidInputs_ReturnsCorrectResult(
        double weight, double height, int age, string gender, double expected)
    {
        // Act
        var result = _calculator.CalculateBmr(weight, height, age, gender);

        // Assert
        Assert.Equal(expected, result, precision: 0);
    }

    [Theory]
    [InlineData(70, 175, 30, "MALE", 1649)]
    [InlineData(70, 175, 30, "Male", 1649)]
    public void CalculateBmr_CaseInsensitiveGender_ReturnsCorrectResult(
        double weight, double height, int age, string gender, double expected)
    {
        // Act
        var result = _calculator.CalculateBmr(weight, height, age, gender);

        // Assert
        Assert.Equal(expected, result, precision: 0);
    }

    [Fact]
    public void CalculateBmr_DefaultGender_ReturnsMaleFormula()
    {
        // Arrange
        double weight = 70, height = 175;
        int age = 30;

        // Act
        var result = _calculator.CalculateBmr(weight, height, age, null);

        // Assert
        var expected = (10 * weight) + (6.25 * height) - (5 * age) + 5;
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 175, 30, "male")]
    [InlineData(70, 0, 30, "male")]
    [InlineData(70, 175, 0, "male")]
    [InlineData(-70, 175, 30, "male")]
    public void CalculateBmr_InvalidInputs_ThrowsArgumentException(
        double weight, double height, int age, string gender)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _calculator.CalculateBmr(weight, height, age, gender));
    }

    [Fact]
    public void CalculateHourlyBmr_ReturnsCorrectValue()
    {
        // Arrange
        var dailyBmr = 1641;

        // Act
        var hourly = _calculator.CalculateHourlyBmr(dailyBmr);

        // Assert
        Assert.Equal(dailyBmr / 24.0, hourly);
    }

    [Fact]
    public void CalculateBmrForPeriod_ReturnsCorrectValue()
    {
        // Arrange
        var dailyBmr = 1641;
        var period = TimeSpan.FromHours(12);

        // Act
        var result = _calculator.CalculateBmrForPeriod(dailyBmr, period);

        // Assert
        Assert.Equal(dailyBmr * 0.5, result);
    }
}
