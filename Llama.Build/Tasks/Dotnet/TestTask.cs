using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Restore;
using Cake.Common.Tools.DotNet.Test;
using Cake.Frosting;
using Llama.Build.Tasks.Dependencies;

namespace Llama.Build.Tasks.Dotnet;

[TaskName("Dotnet.Test")]

[IsDependentOn(typeof(DownloadTestModelTask))]
public sealed class TestTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetRestore(context.LlamaSharpTestDirectory.FullPath, new DotNetRestoreSettings()
        {
        });
        context.DotNetBuild(context.LlamaSharpTestDirectory.FullPath, new DotNetBuildSettings()
        {
            Configuration = context.BuildConfiguration,
            NoRestore = true,
            NoIncremental = true
        });
        context.DotNetTest(context.LlamaSharpTestDirectory.FullPath, new DotNetTestSettings()
        {
            Configuration = context.BuildConfiguration,
            NoRestore = true,
            NoBuild = true,
        });
    }
}