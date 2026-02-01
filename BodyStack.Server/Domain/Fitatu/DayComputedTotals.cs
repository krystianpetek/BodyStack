namespace BodyStack.Server.Domain.Fitatu;

public sealed record DayComputedTotals(
    decimal Energy,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrate,
    decimal Fiber,
    decimal Sugars,
    decimal Salt)
{
    public static DayComputedTotals Zero { get; } = new(0, 0, 0, 0, 0, 0, 0);

    public static DayComputedTotals operator +(DayComputedTotals a, DayComputedTotals b)
        => new(
            a.Energy + b.Energy,
            a.Protein + b.Protein,
            a.Fat + b.Fat,
            a.Carbohydrate + b.Carbohydrate,
            a.Fiber + b.Fiber,
            a.Sugars + b.Sugars,
            a.Salt + b.Salt);
}
