using System;
using System.Collections.Generic;
using UsageTracker.Entities;
using UsageTracker.Interfaces;

namespace UsageTracker.Containers
{
    internal class ProcessContainer : IStorage<ActiveProcess>
    {
        List<ActiveProcess> Storage;

        public ActiveProcess CurrentActive { get; private set; }

        public ProcessContainer()
        {
            Storage = new List<ActiveProcess>();
        }

        public ProcessContainer(List<ActiveProcess> processList, ActiveProcess activeProcess)
        {
            Storage = processList;
            CurrentActive = activeProcess;
        }

        /// <summary>
        /// Adds a process to the storage
        /// </summary>
        /// <param name="process">The process to add</param>
        /// <returns>Returns true if the process has been added successfully</returns>
        public bool Add(ActiveProcess process)
        {
            if (!Storage.Contains(process))
            {
                Storage.Add(process);
                return true;
            }
            return false;
        }

        public void SetActive(ActiveProcess process)
        {
            if (CurrentActive is null)
            {
                if (Contains(process.ProcessName))
                {
                    CurrentActive = process;
                    Storage.Remove(process); // Remove from the storage as it is currently in use.
                }
            } 
            else
            {
                Storage.Add(CurrentActive);
                CurrentActive = process;
                Storage.Remove(process);
            }
        }

        public ActiveProcess Get(ActiveProcess item)
        {
            return Get(item.ProcessName);
        }

        public ActiveProcess Get(string processName)
        {
            foreach (var process in Storage)
            {
                if (processName == process.ProcessName)
                {
                    return process;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if a process already exists
        /// </summary>
        /// <param name="processName">The name of the process</param>
        /// <returns>True if process is found, False otherwise</returns>
        public bool Contains(string processName)
        {            
            //Check active process
            if (CurrentActive != null && CurrentActive.ProcessName != null)
            {
                if (CurrentActive.ProcessName.Equals(processName))
                {
                    return true;
                }
            }

            // Check storage
            foreach (var proc in Storage)
            {
                if (proc.ProcessName == processName)
                    return true;
            }
   
            return false;
        }

        /// <summary>
        /// Removes a <see cref="ActiveProcess"/> from the container
        /// </summary>
        /// <param name="process">The <see cref="ActiveProcess"/> to remove</param>
        /// <returns> </returns>
        public bool Remove(ActiveProcess process)
        {
            if (Storage.Contains(process))
            {
                Storage.Remove(process);
                return true;
            }
            return false;
        }

        public List<ActiveProcess> GetAll()
        {
            if (Storage != null)
            {
                return new List<ActiveProcess>(Storage);
            }
            return null;
        }
    }
}
