using System;
using System.IO;
using System.Threading;

namespace SAR.Libraries.Database.Services
{
    public class BackupService
    {
        private string _dbFilename;
        private string _backupFolder;
        private bool _running;
        private int _runAtHour;
        private DateTime _nextRunTime;

        public BackupService(
            string dbFilename,
            string backupFolder,
            int runAtHour)
        {
            _dbFilename = dbFilename;
            _runAtHour = runAtHour;
            _backupFolder = backupFolder;
            _nextRunTime = GetRunTime();
        }

        private DateTime GetRunTime()
        {
            DateTime now = DateTime.UtcNow;

            DateTime nextRunTime = now.Date;
            nextRunTime = nextRunTime.AddHours(_runAtHour);

            //If we are already after this time add a day
            if (now > nextRunTime)
            {
                nextRunTime = nextRunTime.AddDays(1);
            }

            return nextRunTime;
        }

        public void Start()
        {
            if (!_running)
            {
                _running = true;

                Thread t = new Thread(Run);
                t.Start();
            }
        }

        private string GetBackupFilename()
        {
            DateTime now = DateTime.UtcNow;

            string filename = "backup_" + now.ToString("yyyyMMdd_HHmmss") + ".db";
            return Path.Combine(_backupFolder, filename);
        }

        private void Run()
        {
            do
            {
                DateTime now = DateTime.UtcNow;

                if (now > _nextRunTime)
                {
                    bool success = false;

                    //Try 5 times
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            string filename = GetBackupFilename();

                            File.Copy(_dbFilename, filename);

                            success = true;
                            break;
                        }
                        catch (Exception e)
                        {
                            //Eat the exception
                            Thread.Sleep(1000);
                        }
                    }

                    if (success)
                    {
                        //Succeeded - try again tomorrow
                        _nextRunTime = GetRunTime();
                    }
                    else
                    {
                        //Failed - try again in 15 minutes
                        _nextRunTime = _nextRunTime.AddMinutes(15);
                    }
                }

                Thread.Sleep(1000);
            } while (_running);
        }

        public void Stop()
        {
            _running = false;
        }
    }
}
