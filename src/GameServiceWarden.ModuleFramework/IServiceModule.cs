using System.Collections.Generic;

namespace GameServiceWarden.ModuleFramework
{
    public interface IServiceModule
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
        /// <param name="workspace">The workspace directory. All service required files should be stored here. Expect the directory to be created.</param>
        /// <returns>The <see cref="IService"/> responsible for the instance of the game service.</returns>
        IService InstantiateService(string workspace);
    }
}