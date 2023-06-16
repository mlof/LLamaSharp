using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Restore;
using Cake.Frosting;

namespace Llama.Build.Tasks.Dotnet;

[TaskName("Dotnet.Restore")]
public sealed class RestoreTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetRestore(context.LLamaSharpDirectory.FullPath, new DotNetRestoreSettings()
        {
        });
    }
}