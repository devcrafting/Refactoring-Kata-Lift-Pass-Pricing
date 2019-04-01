using System;
using MySql.Data.MySqlClient;
using Xunit;
using FsCheck.Xunit;
using FluentAssertions;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;

public class LiftServicesShould
{
    private readonly IStoreLiftPricer liftPricerRepository;

    public Random random { get; }

    public LiftServicesShould()
    {
        var connection = new MySqlConnection
            {
                ConnectionString = @"Database=lift_pass;Data Source=localhost;User Id=root;Password=mysql"
            };
        connection.Open();
        liftPricerRepository = new LiftPricerRepository(connection);
        random = new Random(1); 
    }

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    public void ReturnGoldenMaster()
    {
        var types = new [] { "night", "1jour" };

        var result =
            Enumerable.Range(0, 99)
                .Cast<int?>()
                .Concat(new int?[] { null })
                .SelectMany(age => types.SelectMany(type => {
                    var date = new DateTime(random.Next(2017, 2019), random.Next(1, 12), random.Next(1, 28));
                    return new [] {
                        $"- {age}, {type}, {date.ToString("yyyy-MM-dd")}\n",
                        liftPricerRepository.Get(type, date).GetPrice(age) + "\n",
                        liftPricerRepository.Get(type, DateTime.Parse("2019-02-18")).GetPrice(age) + "\n"
                    };
                }));

        Approvals.Verify(string.Concat(result));
    }

    [Property]
    public void ReturnPriceForSeveralPerson(DateTime? date, bool isHolidays, uint basePrice)
    {
        var ages = new [] { 3, 7, 14, 35, 38, 65 };
        var liftPricer = new LiftPricer(date, isHolidays, (int)basePrice);
        var ratio = (!date.HasValue || (int)date.Value.DayOfWeek != 1 || isHolidays ? 1 : 0.65);

        var totalPrice = liftPricer.GetPrice(ages);

        totalPrice.Should().Be(
            0 + 2 * (int)Math.Ceiling(basePrice * 0.7)
            + 2 * (int)Math.Ceiling(basePrice * ratio)
            + (int)Math.Ceiling(basePrice * 0.75 * ratio));
    }

    [Property]
    public void ReturnNightPriceForSeveralPerson(DateTime? date, bool isHolidays, uint basePrice)
    {
        var ages = new [] { 3, 7, 14, 35, 38, 65 };
        var liftPricer = new NightLiftPricer(date, (int)basePrice);
        var ratio = (!date.HasValue || (int)date.Value.DayOfWeek != 1 || isHolidays ? 1 : 0.65);

        var totalPrice = liftPricer.GetPrice(ages);

        totalPrice.Should().Be(
            0 + 4 * (int)basePrice
            + (int)Math.Ceiling(basePrice * 0.4));
    }
}