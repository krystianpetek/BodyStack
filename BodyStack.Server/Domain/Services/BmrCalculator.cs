using System.Globalization;

namespace BodyStack.Server.Domain.Services;

/// <summary>
/// Calculates Basal Metabolic Rate (BMR) using the Mifflin-St Jeor equation.
/// </summary>
public sealed class BmrCalculator
{
    /// <summary>
    /// Calculates BMR in kcal/day using the Mifflin-St Jeor formula.
    /// </summary>
    /// <param name="weightKg">Weight in kilograms</param>
    /// <param name="heightCm">Height in centimeters</param>
    /// <param name="age">Age in years</param>
    /// <param name="gender">Gender ("male" or "female")</param>
    /// <returns>BMR in kcal/day</returns>
    public double CalculateBmr(double weightKg, double heightCm, int age, string? gender)
    {
        if (weightKg <= 0)
            throw new ArgumentException("Weight must be greater than 0", nameof(weightKg));
        
        if (heightCm <= 0)
            throw new ArgumentException("Height must be greater than 0", nameof(heightCm));
        
        if (age <= 0)
            throw new ArgumentException("Age must be greater than 0", nameof(age));

        // Mifflin-St Jeor Equation
        // Men: BMR = (10 × weight in kg) + (6.25 × height in cm) - (5 × age in years) + 5
        // Women: BMR = (10 × weight in kg) + (6.25 × height in cm) - (5 × age in years) - 161
        var bmr = (10 * weightKg) + (6.25 * heightCm) - (5 * age);
        
        return gender?.ToLowerInvariant() switch
        {
            "female" => bmr - 161,
            _ => bmr + 5 // male or default
        };
    }

    /// <summary>
    /// Calculates hourly BMR rate.
    /// </summary>
    public double CalculateHourlyBmr(double dailyBmr) => dailyBmr / 24.0;

    /// <summary>
    /// Calculates BMR calories for a specific time period.
    /// </summary>
    public double CalculateBmrForPeriod(double dailyBmr, TimeSpan period)
    {
        return dailyBmr * (period.TotalHours / 24.0);
    }
}
