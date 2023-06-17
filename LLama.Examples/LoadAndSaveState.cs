using LLama.Common;

namespace LLama.Examples
{
    public class SaveAndLoadState : IDisposable
    {
        private InteractiveExecutor _executor;

        public SaveAndLoadState(string modelPath, string prompt)
        {
            _executor = new InteractiveExecutor(new LLamaModel(new ModelParams(modelPath: modelPath)));
            foreach (var text in _executor.Infer(prompt, new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "user:" } }))
            {
                Console.Write(text);
            }
        }

        public void Run(string prompt)
        {
            InferenceParams sessionParams = new InferenceParams() { Temperature = 0.2f, AntiPrompts = new List<string> { "user:" } };
            foreach (var text in _executor.Infer(prompt, sessionParams))
            {
                Console.Write(text);
            }
        }

        public void SaveState(string executorStateFile, string modelStateFile)
        {
            _executor.Model.SaveState(modelStateFile);
            _executor.SaveState(executorStateFile);
            Console.WriteLine("Saved state!");
        }

        public void LoadState(string executorStateFile, string modelStateFile)
        {
            var model = _executor.Model;
            model.LoadState(modelStateFile);
            _executor = new InteractiveExecutor(model);
            _executor.LoadState(executorStateFile);
            Console.WriteLine("Loaded state!");
        }

        public void Dispose()
        {
            _executor.Model.Dispose();
        }
    }
}
