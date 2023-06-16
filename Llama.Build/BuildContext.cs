using System.Collections.Generic;
using System.Linq;
using Cake.Common;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Llama.Build.Configuration;

namespace Llama.Build;

public class BuildContext : FrostingContext
{
    public string LlamaRepositoryName = "llama.cpp";
    public string LlamaRepositoryUrl = "https://github.com/ggerganov/llama.cpp";

    public BuildContext(ICakeContext context)
        : base(context)
    {
        RepositoryRoot = GetRepositoryRoot(context);
        LibPath = RepositoryRoot.Combine("lib");
        TmpDir = RepositoryRoot.Combine("tmp");
        RuntimeDirectory = RepositoryRoot.Combine("runtime");
        LlamaCppCommitSha = context.Argument("llama-cpp-commit-sha", "7e4ea5beff567f53be92f75f9089e6f11fa5dabd");
        LLamaSharpDirectory = RepositoryRoot.Combine("LLamaSharp");
        LlamaSharpTestDirectory = RepositoryRoot.Combine("LLamaSharp.Tests");
        SolutionPath = RepositoryRoot.CombineWithFilePath("LLamaSharp.sln");
        MsvcGenerator = context.Argument("msvc-generator", "Visual Studio 17 2022");
        BuildConfiguration = context.Argument("build-configuration", "Release");
        ModelDirectory = new DirectoryPath("D:\\LLM");
        BuildSettings = new List<BuildSettings>
        {
            new MsvcBuildSettings()
            {
                FriendlyName = "Bells-and-whistles",
                Platform = "X64",
                BlasType =
                    BlasType.CuBlas,
                EnableKQuants = true,
                Avx512Support = Avx512Support.Avx512 | Avx512Support.Vbmi | Avx512Support.Vnni
            },
            /*new MsvcBuildSettings()
            {
                Platform = "X64",
                BlasType = BlasType.CuBlas,
            }*/
        };
    }

    public DirectoryPath LlamaSharpTestDirectory { get; set; }

    public FilePath SolutionPath { get; set; }

    public DirectoryPath LLamaSharpDirectory { get; set; }

    public DirectoryPath RepositoryRoot { get; init; }

    public List<BuildSettings> BuildSettings { get; init; }

    public List<MsvcBuildSettings> MsvcBuildSettings => BuildSettings.OfType<MsvcBuildSettings>().ToList();
    public string MsvcGenerator { get; init; }

    public DirectoryPath LibPath { get; init; }

    public DirectoryPath TmpDir { get; init; }
    public string LlamaCppCommitSha { get; init; }
    public DirectoryPath LlamaSourceDirectory => LibPath.Combine(LlamaRepositoryName);
    public DirectoryPath LlamaBuildDirectory => TmpDir.Combine("llama.cpp").Combine("build");
    public string BuildConfiguration { get; init; }
    public DirectoryPath RuntimeDirectory { get; init; }
    public DirectoryPath ModelDirectory { get; set; }

    private static DirectoryPath GetRepositoryRoot(ICakeContext context)
    {
        var directoryPath = context.Environment.WorkingDirectory;

        while (!context.DirectoryExists(directoryPath.Combine(".git"))) directoryPath = directoryPath.GetParent();

        return directoryPath;
    }
}