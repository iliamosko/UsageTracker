using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsageTracker.Entities;
using UsageTracker.Containers;

namespace UsageTracker.Background
{
    internal class ProgressBarWorker
    {
        ProcessContainer processContainer;
        List<ActiveProcess> processList;
        bool isRunning;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="processContainer"></param>
        public ProgressBarWorker(ProcessContainer processContainer)
        {
            this.processContainer = processContainer;
            processList = new List<ActiveProcess>();
        }


        /// <summary>
        /// Adds a process to the worker.
        /// </summary>
        /// <param name="process">The active <see cref="ActiveProcess"/>.</param>
        public void AddProcess(ActiveProcess process)
        {
            //add a check
            processList.Add(process);
        }

        public void Start()
        {
            isRunning = true;
            while(isRunning)
            Task.Run(() =>
            {
                Task.Delay(1000);
                Console.WriteLine($"This is the start method running at {DateTime.Now}");
                foreach (var process in processList)
                {
                    process.UpdateTime();
                }
            }).ConfigureAwait(true);
        }
        
        public void Stop()
        {
            isRunning = false;
            Console.WriteLine("Stopping ProgressBarWorker");
        }

        public bool IsRunning()
        {
            return isRunning;
        }


    }
}
