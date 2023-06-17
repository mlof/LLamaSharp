using Cake.Frosting;
using Llama.Build.Tasks.Cmake;

namespace Llama.Build.Tasks
{
    [TaskName("Build")]
    [TaskDescription("Builds the complete project, C++ and .net")]
    [IsDependentOn(typeof(CmakeBuildTask))]
    [IsDependentOn(typeof(Dotnet.BuildTask))]
    public class BuildTask : FrostingTask<BuildContext>
    {
    }
}
