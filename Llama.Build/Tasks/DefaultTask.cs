using Cake.Frosting;

namespace Llama.Build.Tasks;

[TaskName("Default")]
[IsDependentOn(typeof(Dotnet.TestTask))]
/*[IsDependentOn(typeof(CleanTask))]
[IsDependentOn(typeof(CloneTask))]
[IsDependentOn(typeof(BuildTask))]
[IsDependentOn(typeof(MoveToRuntimeDirectoryTask))]*/
public class DefaultTask : FrostingTask
{
}