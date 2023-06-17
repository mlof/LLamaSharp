using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.Frosting;
using Llama.Build.Tasks.Cmake.Msvc;

namespace Llama.Build.Tasks.Cmake
{
    [TaskName("Cmake.Build")]
    [TaskDescription("Builds the C++ project")]
    [IsDependentOn(typeof(ConfigureTask))]
    [IsDependentOn(typeof(Cmake.Msvc.BuildTask))]
    [IsDependentOn(typeof(MoveToRuntimeDirectoryTask))]
    [IsDependeeOf(typeof(Dotnet.BuildTask))]
    public class CmakeBuildTask : FrostingTask<BuildContext>
    {
    }
}