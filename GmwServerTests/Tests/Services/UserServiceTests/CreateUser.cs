using System.Net.Mail;
using Moq;
using GmwServer;
using System.Net;
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

        var actEx = await Assert.ThrowsAsync<DbUpdateException>(() => {
            db.Users.Add(inpUser);
            return db.SaveChangesAsync();
        });

        Assert.True(
            actEx.InnerException!.Message.Contains("UNIQUE constraint failed: Users.Email"),
            "Database did not produce expected unique constraint violation message.");
    }

    [Theory, MemberData(nameof(CreateUserTestsData))]
    public async Task CreateUserTests(TestCase test){
        await SetupDatabase(DefaultDbContextOptions, test.Setups);

        var inpMailAddress = (MailAddress)test.Inputs["email"]!;

        var actor = new UserService(_dbContextFactoryMock.Object);
        var act = await actor.CreateUser(inpMailAddress);

        var exp = CreateExpectedServiceResult((Dictionary<string, object?>)test.Expected["service result"]!);

        Assert.Equal(exp.IsError, act.IsError);
        Assert.Equal(exp.Status, act.Status);
        Assert.Equal(exp.GetError(), act.GetError());

        if (exp.IsError){
            Assert.Null(act.GetData());
            return;
        }

        using var db = new GmwServerDbContext(DefaultDbContextOptions);

        var actUsers = await
            (from u in db.Users
            where u.Id == (UserId)act.GetData()!
            select u)
            .ToListAsync();
        Assert.Single(actUsers);

        var actUser = actUsers.First();
        Assert.Equal(inpMailAddress, actUser.Email, MailAddressEqualityComparer.Instance);
        Assert.Equal(inpMailAddress.User, actUser.DisplayName);
    }

    private IServiceResult CreateExpectedServiceResult(Dictionary<string, object?> properties){
        var mock = new Mock<IServiceResult>();

        mock.Setup(e => e.IsError).Returns((bool)properties["is error"]!);
        mock.Setup(e => e.Status).Returns((HttpStatusCode)properties["status"]!);

        mock.Setup(e => e.GetError()).Returns(properties["error"]);

        return mock.Object;
    }

    public static IEnumerable<object[]> CreateUserTestsData => BundleTestCases(
        new TestCase("Email doesn't exist, empty user table")
            .WithInput("email", new MailAddress("john.doe@example.com"))
            .WithExpected(
                "service result",
                new Dictionary<string, object?>{
                    {"is error", false},
                    {"status", HttpStatusCode.Created},
                    {"error", null}
                })


        ,new TestCase("Email doesn't exist, user table populated")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Users", new User[]{
                        new User{
                            Id = new UserId(Guid.Parse("be6de9fa-dfd6-4e05-a344-5e868f99b0d0")),
                            Email = new MailAddress("john.doe@somesite.com"),
                            DisplayName = "john.doe",
                        },
                        new User{
                            Id = new UserId(Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5")),
                            Email = new MailAddress("alice.foo@bar.com"),
                            DisplayName = "Alice"
                        }
                    }}
                }
            )
            .WithInput("email", new MailAddress("john.doe@example.com"))
            .WithExpected(
                "service result",
                new Dictionary<string, object?>{
                    {"is error", false},
                    {"status", HttpStatusCode.Created},
                    {"error", null}
                })


        ,new TestCase("Email already exists")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Users", new User[]{
                        new User{
                            Id = new UserId(Guid.Parse("be6de9fa-dfd6-4e05-a344-5e868f99b0d0")),
                            Email = new MailAddress("john.doe@somesite.com"),
                            DisplayName = "john.doe",
                        },
                        new User{
                            Id = new UserId(Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5")),
                            Email = new MailAddress("alice.foo@bar.com"),
                            DisplayName = "Alice"
                        }
                    }}
                }
            )
            .WithInput("email", new MailAddress("john.doe@somesite.com"))
            .WithExpected(
                "service result",
                new Dictionary<string, object?>{
                    {"is error", true},
                    {"status", HttpStatusCode.UnprocessableEntity},
                    {"error", "Email address already registered."}
                })


        // TODO need Fact that checks that you cannot add the same email

    );
}