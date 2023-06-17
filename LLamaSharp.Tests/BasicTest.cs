using LLama.Common;

namespace LLama.Unittest
{
    public class BasicTest
    {
        public const string ModelName = "wizardLM-7B.ggmlv3.q4_0.bin";
        public static string ModelPath => Path.Join(Constants.ModelDirectory, ModelName);

        [Fact]
        public void CanLoadModel()
        {
            var modelParameters = new ModelParams(ModelPath);
            using var model = new LLamaModel(modelParameters);
        }

        [Fact]
        public void CanTokenize()
        {
            var modelParameters = new ModelParams(ModelPath);
            using var model = new LLamaModel(modelParameters);

            var tokens = model.Tokenize("Hello world");
            Assert.Equal(3, tokens.Count());
        }
    }

    public static class Constants
    {
        public static string ModelDirectory => Path.Join(GetRepositoryRoot(), "models");

        private static string GetRepositoryRoot()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;


            while (!Directory.Exists(Path.Join(baseDirectory, ".git")))
            {
                var directoryInfo = new DirectoryInfo(baseDirectory).Parent;
                if (directoryInfo != null)
                {
                    baseDirectory = directoryInfo.FullName;
                }
            }

            return baseDirectory;
        }
    }
}