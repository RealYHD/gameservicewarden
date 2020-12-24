using System.Collections.Generic;

namespace GameServiceWarden.ModuleAPI
{
    public interface IGameServiceModule
    {
        /// <summary>
        /// The name of the game service this module handles.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the game service this module handles.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The authors responsible for creating this module.
        /// </summary>
        IEnumerable<string> Authors { get; }

        /// <summary>
        /// Creates an instance of a the service to be used.
        /// </summary>
        /// <returns>The <see cref="IGameService"/> responsible for the instance of the game service.</returns>
        IGameService CreateGameService();
    }
}