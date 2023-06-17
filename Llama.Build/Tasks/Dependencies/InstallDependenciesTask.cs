﻿using Cake.Common;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;

namespace Llama.Build.Tasks.Dependencies
{
    [TaskName("Dependencies.Install")]
    public class InstallDependenciesTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.Log.Information("Checking dependencies...");

            var cmakeExists = CanResolveTool(context, "cmake", "cmake.exe");

            var gitExists = CanResolveTool(context, "git", "git.exe");

            var cudaToolkitExists = CanResolveTool(context, "nvcc", "nvcc.exe");


            if (context.IsRunningOnWindows())
            {
                if (!cmakeExists)
                {
                    context.WingetInstall("Kitware.CMake");
                }

                if (!gitExists)
                {
                    context.WingetInstall("Git.Git");
                }
            }

            if (!cudaToolkitExists)
            {
                context.Log.Error("Could not find nvcc.exe. Please install the CUDA toolkit.");
                context.Log.Error("https://developer.nvidia.com/cuda-toolkit");
            }
        }

        private static bool CanResolveTool(ICakeContext context, params string[] args)
        {
            foreach (var s in args)
            {
                var path = context.Tools.Resolve(s);

                if (path != null)
                {
                    context.Log.Information($"Found {s} at {path}");
                    return true;
                }
            }

            context.Log.Error($"Could not find any of the following: {string.Join(", ", args)}");

            return false;
        }
    }

    public static class CakeContextExtensions
    {
        public static void WingetInstall(this ICakeContext context, string packageName)
        {
            context.StartProcess("winget", new ProcessSettings()
            {
                Arguments = "install " + packageName,
            });
        }
    }
}