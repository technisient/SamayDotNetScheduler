﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;
using System.IO;

namespace Technisient
{
    public class TestListener : Technisient.ISamayEngineListener
    {
        private readonly DateTime _startTime = DateTime.Now;

        public void Log(string logMsg, string job, string task, string id, LogLevel logLevel)
        {
            // TRACE <  DEBUG < INFO < WARN < ERROR < FATAL

            switch (logLevel)
            {
                case LogLevel.Trace:
                    if (_logLevel > LogLevel.Trace)
                        return;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogLevel.Debug:
                    if (_logLevel > LogLevel.Debug)
                        return;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    if (_logLevel > LogLevel.Info)
                        return;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warn:
                    if (_logLevel > LogLevel.Warn)
                        return;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case LogLevel.Error:
                    if (_logLevel > LogLevel.Error)
                        return;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Fatal:
                    if (_logLevel > LogLevel.Fatal)
                        return;
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("logLevel");
            }

            Console.WriteLine(task + " [" + logLevel + "]" + ":\t" + logMsg);
            Console.ResetColor();
        }

        private LogLevel _logLevel;
        void ISamayEngineListener.LoggerLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }


        public void SafeStopEngine(string requester)
        {
            throw new NotImplementedException();
        }

        public EngineStatus GetEngineStatus()
        {
            return EngineStatus.Running;
        }

        public DateTime GetEngineStartTime()
        {
            return _startTime;
        }


        public string GetVersion()
        {
            return "2.1";
        }


        public string GetSamayConfig()
        {
            throw new NotImplementedException();
        }

        public bool SaveSamayConfig(string config, string comment)
        {
            throw new NotImplementedException();
        }


        public void ReloadConfig(string requester)
        {
            throw new NotImplementedException();
        }


        public bool AddJob(string job, string comment)
        {
            throw new NotImplementedException();
        }

        public bool RemoveJob(string jobName, string comment)
        {
            throw new NotImplementedException();
        }
    }
}
