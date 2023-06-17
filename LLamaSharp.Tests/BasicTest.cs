using LLama.Common;

namespace LLama.Tests
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
}
