using Cake.Common.IO;
using Cake.Common.Net;
using Cake.Core.Diagnostics;
using Cake.Frosting;

namespace Llama.Build.Tasks.Dependencies
{
    [TaskName("Dependencies.ClBlast")]
    public class DownloadClBlastTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var openClVersion = context.OpenClVersion;
            var url =
                $"https://github.com/KhronosGroup/OpenCL-SDK/releases/download/v{openClVersion}/OpenCL-SDK-v{openClVersion}-Win-x64.zip";

            var outputDirectory = context.TmpDir.Combine("OpenCl").Combine(openClVersion);
            context.EnsureDirectoryExists(outputDirectory);
            var outputPath = outputDirectory
                .CombineWithFilePath("opencl.zip");
            context.DownloadFile(url, outputPath.FullPath, new DownloadFileSettings()
            {

            });

            context.Unzip(outputPath.FullPath, context.LibPath.Combine("OpenCl").FullPath);
        }
    } 
    [TaskName("Dependencies.TestModel")]
    [IsDependeeOf(typeof(InstallDependenciesTask))]
    public class DownloadTestModelTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.ModelDirectory = context.RepositoryRoot.Combine("models");
            var url = "https://huggingface.co/TheBloke/wizardLM-7B-GGML/resolve/main/wizardLM-7B.ggmlv3.q4_0.bin";
            var fileName = "wizardLM-7B.ggmlv3.q4_0.bin";

            context.EnsureDirectoryExists(context.ModelDirectory);

            if (context.FileExists(context.ModelDirectory.CombineWithFilePath(fileName)))
            {
                context.Log.Information($"File {fileName} already exists, skipping download");
            }
            else
            {
                context.Log.Information($"Downloading {fileName} from {url}");
                context.DownloadFile(url, context.ModelDirectory.CombineWithFilePath(fileName),
                    new DownloadFileSettings());
            }
        }
    }
}
