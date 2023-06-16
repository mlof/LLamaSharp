using Cake.Common.IO;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Llama.Build.Configuration;

namespace Llama.Build.Tasks.Cmake.Msvc;

[TaskName("Cmake.Msvc.MoveToRuntimeDirectory")]
[TaskDescription("Moves the C++ build output to the runtime directory, so it can be used by the application")]
[IsDependentOn(typeof(BuildTask))]
public sealed class MoveToRuntimeDirectoryTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        foreach (var setting in context.BuildSettings)
        {
            Run(context, setting);
        }
    }

    private void Run(BuildContext context, BuildSettings setting)
    {
        context.EnsureDirectoryExists(context.RuntimeDirectory);


        var runtimeIdentifier = "win-x64";

        var runtimeDirectory = context.RuntimeDirectory.Combine(runtimeIdentifier);

        context.EnsureDirectoryExists(runtimeDirectory);

        var sourceDirectory = context.LlamaBuildDirectory.Combine(setting.BuildPath).Combine("bin").Combine(context.BuildConfiguration);

        context.Log.Information($"Moving {sourceDirectory.FullPath} to {runtimeDirectory.FullPath}");

        context.CopyDirectory(sourceDirectory, runtimeDirectory);
    }
}