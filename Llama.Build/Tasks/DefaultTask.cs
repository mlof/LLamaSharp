using System;
using System.IO;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;

namespace Llama.Build.Tasks;

[TaskName("Default")]
[TaskDescription("A friendly entrypoint for the build system")]
public class DefaultTask : FrostingTask
{
    public override void Run(ICakeContext context)
    {
        var filePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "DefaultMessage.txt");
        var message = File.ReadAllText(filePath);

        context.Log.Information(message);
    }
}