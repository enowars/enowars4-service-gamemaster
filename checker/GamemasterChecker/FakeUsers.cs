using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Extensions.FileProviders.Physical;

namespace GamemasterChecker
{
    public class FakeUsers
    {
        public static string GetFakeSession()
        {
            var o = new Faker("de");
            return o.Random.Bool() switch
            {
                true => o.Company.Bs(),
                false => o.Company.CatchPhrase()
            };
        }
        public static string GetUserAgent()
        {
            //var o = new Faker("de");
            //return o.Internet.UserAgent();
            return UserAgents.GetRandomUserAgent();
        }
        public static string GetFakeChat()
        {
            var o = new Faker("de");
            return o.Random.Bool() switch
            {
                true => o.Hacker.Phrase(),
                false => o.Rant.Review()
            };
        }
        public static GamemasterUser GetFakeUser(long roundId, long teamId, string? flag)
        {
            var o = new Faker<GamemasterUser>("de")
                .RuleFor(u => u.Username, (f, u) => f.Internet.UserNameUnicode(f.Name.FirstName(), f.Name.LastName()))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(f.Name.FirstName(), f.Name.LastName()))
                .FinishWith((f, u) =>
                {
                    Console.WriteLine("User Created! Id={0}", u.Id);
                });
            var user = o.Generate();
            user.RoundId = roundId;
            user.TeamId = teamId;
            user.Flag = flag;
            user.Username += Environment.TickCount.ToString();
            using var rng = new RNGCryptoServiceProvider();
            byte[] pw = new byte[16];
            rng.GetNonZeroBytes(pw);
            user.Password = System.Convert.ToBase64String(pw);
            Console.WriteLine(JsonSerializer.Serialize(user));
            return new GamemasterUser()
            {
                RoundId = roundId,
                TeamId = teamId,
                Email = "Test",
                Password = "ultrasecurepw",
                Flag = flag,
                Username = $"Herbert{Environment.TickCount}|{Utils.Random.Next()}"
            };
        }
    }
}
