using System.Net.Mail;
using GmwServer;
using Microsoft.EntityFrameworkCore;

namespace GmwServerTests;

public partial class UserServiceTests
{

    [Fact]
    public async Task CannotInsertUserWithSameEmail(){
        var dbData = new Dictionary<string, object[]>{
                {"Users", new []{
                    new User{
                        Id = new UserId(Guid.Parse("be6de9fa-dfd6-4e05-a344-5e868f99b0d0")),
                        Email = new MailAddress("john.doe@somesite.com"),
                        DisplayName = "john.doe",
                    },
                }}};

        await SetupDatabase(DefaultDbContextOptions, new Dictionary<string, object?>{{"database", dbData}});

        var inpUser = new User{
            Id = new UserId(Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5")),
            Email = new MailAddress("john.doe@somesite.com"),
            DisplayName = "John"
        };

        using var db = new GmwServerDbContext(DefaultDbContextOptions);

        db.Users.Add(inpUser);
        db.Invoking(d => d.SaveChanges()).Should().Throw<DbUpdateException>()
            .WithInnerException<Exception>()
            .WithMessage("*UNIQUE constraint failed: Users.Email*");
    }

    [Theory, MemberData(nameof(CreateUserTestsData))]
    public async Task CreateUserTests(TestCase test){
        await SetupDatabase(DefaultDbContextOptions, test.Setups);

        var inpMailAddress = (MailAddress)test.Inputs["email"]!;

        var actor = new UserService(_dbContextFactoryMock.Object);
        var actServiceResult = await actor.CreateUser(inpMailAddress);

        var expServiceResult = (IServiceResult<UserId>)test.Expected["service result"]!;

        actServiceResult.Should().Be(
            expServiceResult,
            new ServiceResultEqaulityComparer<UserId>(
                dataComparer: (_, _) => true
            ));

        using var db = new GmwServerDbContext(DefaultDbContextOptions);
        var actUsers = await
            (from u in db.Users
            where u.Id == actServiceResult.Data!
            select u)
            .ToListAsync();

        if (expServiceResult.IsError){
            actUsers.Should().BeEmpty();
            return;
        }

        actUsers.Should().ContainSingle()
            .And.AllSatisfy(a => {
                a.Email.Should().Be(inpMailAddress, MailAddressEqualityComparer.Instance);
                a.DisplayName.Should().Be(inpMailAddress.User);
            });
    }

    public static IEnumerable<object[]> CreateUserTestsData => BundleTestCases(
        new TestCase("Email doesn't exist, empty user table")
            .WithInput("email", new MailAddress("john.doe@example.com"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder<UserId>()
                    .WithStatus(HttpStatusCode.Created)
                    .WithData(new UserId(Guid.Empty))
                    .Create())


        ,new TestCase("Email doesn't exist, user table populated")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Users", new User[]{
                        new User{
                            Id = UserId.FromString("be6de9fa-dfd6-4e05-a344-5e868f99b0d0"),
                            Email = new MailAddress("john.doe@somesite.com"),
                            DisplayName = "john.doe",
                        },
                        new User{
                            Id = UserId.FromString("48e29af1-b0b7-45e8-bd56-fe2c882be1a5"),
                            Email = new MailAddress("alice.foo@bar.com"),
                            DisplayName = "Alice"
                        }
                    }}
                }
            )
            .WithInput("email", new MailAddress("john.doe@example.com"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder<UserId>()
                    .WithStatus(HttpStatusCode.Created)
                    .WithData(new UserId(Guid.Empty))
                    .Create())


        ,new TestCase("Email already exists")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Users", new User[]{
                        new User{
                            Id = UserId.FromString("be6de9fa-dfd6-4e05-a344-5e868f99b0d0"),
                            Email = new MailAddress("john.doe@somesite.com"),
                            DisplayName = "john.doe",
                        },
                        new User{
                            Id = UserId.FromString("48e29af1-b0b7-45e8-bd56-fe2c882be1a5"),
                            Email = new MailAddress("alice.foo@bar.com"),
                            DisplayName = "Alice"
                        }
                    }}
                }
            )
            .WithInput("email", new MailAddress("john.doe@somesite.com"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder<UserId>()
                    .WithStatus(HttpStatusCode.UnprocessableEntity)
                    .WithError("Email address already registered.")
                    .Create())


    );
}