using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsageTracker.Interfaces
{
    internal interface IStorage<T>
    {
        /// <summary>
        /// Adds an item to the storage.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>Returns <see cref="true"/> if successfully added to the storage, <see cref="false"/> otherwise.</returns>
        bool Add(T item);

        /// <summary>
        /// Removes an item from the storage.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>Returns <see cref="true"/> if successfully removed the item from the storage, <see cref="false"/> otherwise.</returns>
        bool Remove(T item);

        /// <summary>
        /// Gets an item from the storage.
        /// </summary>
        /// <param name="item">The item to get.</param>
        /// <returns>Returns the item in the storage.</returns>
        T Get(T item);
    }
}
