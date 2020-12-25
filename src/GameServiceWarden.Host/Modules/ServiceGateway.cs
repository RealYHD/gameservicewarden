using System.Collections.Generic;
using System.IO;
using GameServiceWarden.Host.Modules.Exceptions;

namespace GameServiceWarden.Host.Modules
{
    public class ServiceGateway
    {
        private readonly string dataDirectory;
        private const string SERVICE_NAME = "Service Name";
        private const string ASSEMBLY_NAME = "Assembly Name";
        private const string MODULE_NAME = "Module Name";
        private const string EXTENSION = ".sin"; //Service info

        public ServiceGateway(string dataDirectory)
        {
            if (!Directory.Exists(dataDirectory)) Directory.CreateDirectory(dataDirectory);
            this.dataDirectory = dataDirectory;
        }
        public void SaveService(string serviceName, string assemblyName, string moduleName)
        {
            string serviceInfoPath = dataDirectory + Path.DirectorySeparatorChar + serviceName + EXTENSION;
            using (StreamWriter writer = File.CreateText(serviceInfoPath))
            {
                writer.WriteLine($"{SERVICE_NAME} : {serviceName}");
                writer.WriteLine($"{ASSEMBLY_NAME} : {assemblyName}");
                writer.WriteLine($"{MODULE_NAME} : {moduleName}");
            }
        }

        public string GetServiceName(string path)
        {
            return GetServiceInfoValue(path, SERVICE_NAME);
        }

        public string GetServiceModuleName(string path)
        {
            return GetServiceInfoValue(path, MODULE_NAME);
        }

        public string GetServiceAssemblyName(string path)
        {
            return GetServiceInfoValue(path, ASSEMBLY_NAME);
        }

        private string GetServiceInfoValue(string path, string key)
        {
            IEnumerable<string> lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (line.StartsWith($"{key}: "))
                {
                    return line.Substring(key.Length + 2);
                }
            }
            throw new CorruptedServiceInfoException($"\"{path}\" is corrupted. Could not find value for: {key}.");

        }

        public IEnumerable<string> GetAllServiceInfoPaths()
        {
            string[] files = Directory.GetFiles(dataDirectory);
            foreach (string filePath in files)
            {
                if (Path.GetExtension(filePath).Equals(EXTENSION))
                {
                    yield return filePath;
                }
            }
        }
    }
}