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
        public static string getFakeSession()
        {
            var o = new Faker("de");
            return o.Hacker.Noun();
        }
        public static string getFakeChat()
        {
            var o = new Faker("de");
            //return o.Hacker.Phrase();
            return o.Rant.Review();
        }
        public static GamemasterUser getFakeUser(GamemasterUser u)
        {
            var o = new Faker<GamemasterUser>()
                .CustomInstantiator(f => u)
                .RuleFor(u => u.Username, (f, u) => f.Internet.UserName(f.Name.FirstName(), f.Name.LastName()))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(f.Name.FirstName(), f.Name.LastName()))
                .FinishWith((f, u) =>
                {
                    Console.WriteLine("User Created! Id={0}", u.Id);
                });
            var user = o.Generate();
            user.Id = u.Id;
            user.RoundId = u.RoundId;
            user.TeamId = u.TeamId;
            user.Username = user.Username + Environment.TickCount.ToString();
            using var rng = new RNGCryptoServiceProvider();
            byte[] pw = new byte[16];
            rng.GetNonZeroBytes(pw);
            user.Password = System.Convert.ToBase64String(pw);
            Console.WriteLine(JsonSerializer.Serialize(user));
            return user;
        }
    }
}
