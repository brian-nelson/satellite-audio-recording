﻿using System.IO;
using Autofac;
using LiteDB;
using SAR.Apps.Server.Services;
using SAR.Libraries.Common.Helpers;
using SAR.Libraries.Common.Interfaces;
using SAR.Libraries.Common.Logging;

namespace SAR.Apps.Server.Modules
{
    public class SarWebServerModule : Module
    {
        private readonly Config _config;

        public SarWebServerModule(Config config)
        {
            _config = config;
        }

        protected override void Load(ContainerBuilder builder)
        {
            SetupFolders();

            builder.RegisterType<ConsoleLogger>().As<ISarLogger>();

            builder.Register(c =>
                {
                    string filename = Path.Combine(_config.DbFolder, "sar.db");
                    return new LiteDatabase(filename);

                }).As<LiteDatabase>()
                .SingleInstance();

            builder.Register(c => new JwtService(_config.JwtSecret))
                .As<JwtService>();

            //Local Services
            builder.RegisterType<AuthService>();
        }

        private void SetupFolders()
        {
            DirectoryHelper.EnsureDirectory(_config.DbFolder);
            DirectoryHelper.EnsureDirectory(_config.FilesFolder);
        }
    }
}
