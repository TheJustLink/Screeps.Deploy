using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using Screeps.Network;
using Screeps.Network.API.User;

namespace Screeps.Deploy;

class Program
{
    private static ConsoleColor s_defaultColor;
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        IncludeFields = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static void Main(string[] args)
    {
        InitializeConsole();

        if (args.Length == 0) return;

        Console.WriteLine(Environment.CurrentDirectory);

        try
        {
            Deploy(args);
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

    private static void Deploy(string[] paths)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Deploying {paths.Length} server configs [");

        foreach (var path in paths)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"=> from {path}");

            Deploy(path);
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("]");
    }
    private static void Deploy(string configPath)
    {
        var config = LoadConfig(configPath);
        if (!TryLoadToken(config.TokenPath, out var token))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR : token file '{token}' not found");
            return;
        }

        var apiClient = new Client();

        if (config.Protocol != default)
            apiClient.Protocol = config.Protocol;
        if (config.Host != default)
            apiClient.Host = config.Host;
        if (config.ServerType != default)
            apiClient.ServerType = config.ServerType;

        var isEmailPasswordPattern = token.Contains(':');
        if (isEmailPasswordPattern)
        {
            var parts = token.Split(':');
            apiClient.SetEmailPasswordAuth(parts[0], parts[1]);
        }
        else
        {
            apiClient.SetToken(token);
        }
        
        DeployCode(apiClient, config.Deploys);
    }
    private static Config LoadConfig(string path)
    {
        if (!File.Exists(path))
            throw new ArgumentException($"Config file '{path}' not found", nameof(path));

        var content = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<Config>(content, s_jsonOptions);

        return config;
    }
    private static bool TryLoadToken(string path, out string token)
    {
        if (!File.Exists(path))
        {
            token = null;
            return false;
        }

        token = File.ReadAllText(path);
        return true;
    }

    private static void DeployCode(Client apiClient, DeployConfig[] configs)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"   Deploying {configs.Length} branches [");

        for (var i = 0; i < configs.Length; i++)
        {
            var config = configs[i];

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"      {config.Branch}");
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" :: ");
            
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"{config.Modules.Count} modules");

            DeployCode(apiClient, config);
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("   ]");
    }
    private static void DeployCode(Client apiClient, DeployConfig config)
    {
        var cursor = Console.CursorLeft;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"   Loading {config.Modules.Count} modules...");
        var modules = LoadModules(config);

        Console.CursorLeft = cursor;
        Console.Write($"   Deploying {config.Modules.Count} modules...");

        var request = new CodeRequest(config.Branch, modules);
        var response = apiClient.UploadCode(request);

        Console.CursorLeft = cursor;
        Console.Write(new string(' ', Console.BufferWidth - cursor - 1));
        Console.CursorLeft = cursor;

        Console.Write(" :: ");

        if (response.Ok)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR : {response.Error}");
        }
    }
    private static Dictionary<string, string> LoadModules(DeployConfig config)
    {
        var modules = new Dictionary<string, string>(config.Modules.Count);

        foreach (var module in config.Modules)
        {
            if (!File.Exists(module.Value))
                throw new ArgumentException($"File '{module.Value}' not found", nameof(module.Value));
                
            var moduleContent = File.ReadAllText(module.Value);

            modules.Add(module.Key, moduleContent);
        }

        return modules;
    }

    private static bool SignIn(Client apiClient, string usernamePassword)
    {
        var parts = usernamePassword.Split(':');
        if (parts.Length == 2)
            return SignIn(apiClient, parts[0], parts[1]);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[Token] Invalid email/username:password format");

        return false;
    }
    private static bool SignIn(Client apiClient, string email, string password)
    {
        var response = apiClient.SignIn(email, password);
        if (true || response.Ok)
        {
            //apiClient.SetToken(response.Token);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Token] OK");

            PrintUsername(email);

            return true;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[Token] Invalid email/username:password");

        return false;
    }
    private static void PrintUsername(Client apiClient)
    {
        PrintUsername(apiClient.GetUsername());
    }
    private static void PrintUsername(string username)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[Username] " + username);
    }
    private static bool IsValidToken(Client apiClient)
    {
        if (apiClient.IsValidToken())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Token] OK");

            return true;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[Token] Invalid token");

        return false;
    }
}