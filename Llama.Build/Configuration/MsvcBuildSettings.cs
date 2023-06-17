namespace Llama.Build.Configuration;

public record MsvcBuildSettings : BuildSettings
{
    public override string CompilerType { get; } = "msvc";
}