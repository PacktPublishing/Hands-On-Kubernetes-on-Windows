namespace VotingApplication.Services
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using Ninject.Extensions.Logging;

    using VotingApplication.Interfaces;

    public class CpuStressWorker : ICpuStressWorker
    {
        private readonly ILogger logger;
        private readonly object startLock = new object();

        private bool isEnabled = false;
        private int targetCpuLoad;

        public CpuStressWorker(ILogger logger)
        {
            this.logger = logger;
        }

        public void Enable(int value)
        {
            this.logger.Info($"Setting CPU stress target value to {value}%");

            this.targetCpuLoad = value;

            lock (this.startLock)
            {
                if (this.isEnabled)
                {
                    return;
                }

                this.isEnabled = true;
                this.StartCpuStress();
            }
        }

        public void Disable()
        {
            this.logger.Info("Disabling CPU stress worker");
            this.isEnabled = false;
        }

        private void StartCpuStress()
        {
            this.logger.Info($"Environment.ProcessorCount: {Environment.ProcessorCount}");

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                var thread = new Thread(
                    () =>
                        {
                            var watch = new Stopwatch();
                            watch.Start();

                            while (this.isEnabled)
                            {
                                if (watch.ElapsedMilliseconds <= this.targetCpuLoad)
                                {
                                    continue;
                                }

                                Thread.Sleep(100 - this.targetCpuLoad);

                                watch.Reset();
                                watch.Start();
                            }
                        });

                thread.Start();
            }
        }
    }
}