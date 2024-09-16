using System;
using System.IO;
using SAR.Libraries.Common.Helpers;
using SAR.Libraries.Common.Security;

namespace SAR.Apps.Server
{
    public class Config
    {
        private static Config _config;

        public string JwtSecret { get; set; }
        public string LocalDataFolder { get; set; }
        public int PortNumber { get; set; }

        public string DbFolder => Path.Combine(_config.LocalDataFolder, "database");
        public string FilesFolder => Path.Combine(_config.LocalDataFolder, "files");

        public static Config GetInstance()
        {
            if (_config == null)
            {
                _config = LoadConfig();
            }

            return _config;
        }

        private static Config LoadConfig()
        {
            string folder = Environment.GetEnvironmentVariable("SAR_LOCAL_DATAFOLDER");
            DirectoryHelper.EnsureDirectory(folder);

            string jwtSecret = Environment.GetEnvironmentVariable("SAR_JWT_SECRET");
            if (string.IsNullOrEmpty(jwtSecret))
            {
                string jwtFile = Path.Combine(folder, "jwt.txt");

                if (File.Exists(jwtFile))
                {
                    jwtSecret = File.ReadAllText(jwtFile);
                }
                else
                {
                    jwtSecret = JwtHelper.CreateJwtSecret();
                    File.WriteAllText(jwtFile, jwtSecret);
                }
            }

            var config = new Config
            {
                JwtSecret = jwtSecret,
                LocalDataFolder = folder,
                PortNumber = 8000
            };
            
            string temp = Environment.GetEnvironmentVariable("SAR_PORT_NUMBER");
            if (!String.IsNullOrEmpty(temp))
            {
                if (int.TryParse(temp, out int result))
                {
                    config.PortNumber = result;
                }
            }

            return config;
        }
    }
}
