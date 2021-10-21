using System;
using System.IO;
using System.Collections.Generic;

using Screeps.Network;
using Screeps.Network.API.User;

using Newtonsoft.Json;

namespace Screeps.Deploy
{
    class Program
    {
        private static ConsoleColor s_defaultColor;

        private static void Main(string[] args)
        {
            InitializeConsole();

            if (args.Length == 0) return;

            try
            {
                Deploy(args[0]);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
        }

        private static void InitializeConsole()
        {
            s_defaultColor = Console.ForegroundColor;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            Console.Title = "Screeps deploy";
            TrySetCursorSize(1);
        }
        private static void TrySetCursorSize(int size)
        {
            try { Console.CursorSize = size; } catch { };
        }
        private static void OnProcessExit(object sender, EventArgs e)
        {
            Console.ForegroundColor = s_defaultColor;
        }

        private static void Deploy(string configPath)
        {
            var config = LoadConfig(configPath);
            var apiClient = new Client(config.Token);

            if (!IsValidToken(apiClient))
                return;

            PrintUsername(apiClient);
            DeployCode(apiClient, config.Deploys);
        }
        private static Config LoadConfig(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException($"Config file '{path}' not found", nameof(path));

            var content = File.ReadAllText(path);
            var config = JsonConvert.DeserializeObject<Config>(content);

            return config;
        }

        private static void DeployCode(Client apiClient, DeployConfig[] configs)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[Deploying {configs.Length} configs]");

            for (var i = 0; i < configs.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[Deploy #{i + 1}] ");
                DeployCode(apiClient, configs[i]);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Deployed]");
        }
        private static void DeployCode(Client apiClient, DeployConfig config)
        {
            var cursor = Console.CursorLeft;
            Console.ForegroundColor = ConsoleColor.Yellow;

            if (config.Protocol != default)
                apiClient.Protocol = config.Protocol;

            if (config.Host != default)
                apiClient.Host = config.Host;

            if (config.ServerType != default)
                apiClient.ServerType = config.ServerType;

            Console.Write($"Loading {config.Modules.Length} modules...");
            var modules = LoadModules(config);
            
            Console.CursorLeft = cursor;
            Console.Write($"Deploying {config.Modules.Length} modules...");

            var request = new CodeRequest(config.Branch, modules);
            var response = apiClient.UploadCode(request);

            Console.CursorLeft = cursor;
            Console.Write(new string(' ', Console.BufferWidth - cursor - 1));
            Console.CursorLeft = cursor;

            if (response.OK)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR : " + response.Error);
            }
        }
        private static Dictionary<string, string> LoadModules(DeployConfig config)
        {
            var modules = new Dictionary<string, string>(config.Modules.Length);

            foreach (var modulePath in config.Modules)
            {
                if (!File.Exists(modulePath))
                    throw new ArgumentException($"File '{modulePath}' not found", nameof(modulePath));

                var moduleName = Path.GetFileNameWithoutExtension(modulePath);
                var module = File.ReadAllText(modulePath);

                modules.Add(moduleName, module);
            }

            return modules;
        }

        private static void PrintUsername(Client api)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Username] " + api.GetUsername());
        }
        private static bool IsValidToken(Client api)
        {
            if (api.IsValidToken())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[Token] OK");

                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Token] Invalid token");

                return false;
            }
        }
    }
}