﻿using Serilog;
using Serilog.Core;

namespace SqlChangeTrackingProducerConsumer
{
    /// <summary>
    /// Very simple abstract base that sets up the callbacks
    /// This is the base of all the windows services
    /// </summary>
    public abstract class WindowsServiceBase
    {

        public ILogger Logger { get; set; }

        public void Start(ILogger logger)
        {
            logger?.Information("Service base Start [Start]");
            Logger = logger;

            OnStart();

            logger?.Information("Service base Start [End]");
        }
       

        public void Stop()
        {
            OnStop();
        }

        public bool Pause()
        {
            return OnPause();
        }

        public bool Continue()
        {
            return OnContinue();
        }

        public void Shutdown()
        {
            OnShutdown();
        }

        public abstract void OnShutdown();

        public abstract bool OnContinue();

        public abstract bool OnPause();

        public abstract void OnStart();

        public abstract void OnStop();
    }
}
