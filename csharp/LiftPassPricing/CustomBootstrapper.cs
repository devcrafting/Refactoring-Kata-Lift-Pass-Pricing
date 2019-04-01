using MySql.Data.MySqlClient;
using Nancy;
using Nancy.Configuration;
using Nancy.TinyIoc;

public class CustomBootstrapper : DefaultNancyBootstrapper
{
    protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
    {
        var connection = new MySqlConnection
            {
                ConnectionString = @"Database=lift_pass;Data Source=localhost;User Id=root;Password=mysql"
            };
        connection.Open();
        container.Register<MySqlConnection>(connection);
    }
    public override void Configure(INancyEnvironment environment)
    {
        environment.Tracing(enabled: false, displayErrorTraces: true);
    }
}