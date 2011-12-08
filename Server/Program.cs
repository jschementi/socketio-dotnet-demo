using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Server {
    class Program {
        static void Main(string[] args) {
            var randomNumberService = new RandomNumberService();
            var server = new Transport(30000, randomNumberService);
            server.Start();

            // Launch node.js server
            var nodeEnv = new Dictionary<string, string>();
            if (args.FirstOrDefault() == "production") nodeEnv["NODE_ENV"] = "production";
            StartNode("index.js", nodeEnv);

            while (server.IsRunning) 
                System.Threading.Thread.Sleep(500);
        }

        #region Starting NodeJS

        static void StartNode(string args, Dictionary<string, string> envVars = null) {
            var riskMonWeb = GetRiskMonWebPath();
            var nodeExePath = Path.Combine(riskMonWeb, "bin", "node.exe");
            var psi = new ProcessStartInfo(nodeExePath, args) {
                WorkingDirectory = riskMonWeb,
                UseShellExecute = false,
            };
            if (envVars != null) {
                foreach (var envVar in envVars) {
                    psi.EnvironmentVariables.Add(envVar.Key, envVar.Value);
                }
            }
            Process.Start(psi);
        }

        static string GetRiskMonWebPath() {
            var assemblyUri = Assembly.GetExecutingAssembly().CodeBase;
            var assemblyPath = Path.GetDirectoryName(new Uri(assemblyUri).PathAndQuery.Replace('/', '\\'));
            var root = Path.GetFullPath(Path.Combine(assemblyPath, "..", "..", ".."));
            return Path.Combine(root, "Web");
        }

        #endregion
    }
}
