using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Frosting;

namespace Llama.Build.Tasks.Dotnet
{
    [TaskName("Dotnet.Build")]
    [IsDependentOn(typeof(RestoreTask))]
    public sealed class BuildTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.DotNetBuild(context.LLamaSharpDirectory.FullPath,
                new DotNetBuildSettings
                {
                    Configuration = context.BuildConfiguration, NoRestore = true, NoIncremental = true
                });
        }
    }
}
