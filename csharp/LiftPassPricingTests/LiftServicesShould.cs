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
}