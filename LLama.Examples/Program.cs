using LLama;
using LLama.Common;
using LLama.Examples;
using LLama.Transformations;

Console.WriteLine("======================================================================================================");

Console.WriteLine(" __       __                                       ____     __                                  \r\n/\\ \\     /\\ \\                                     /\\  _`\\  /\\ \\                                 \r\n\\ \\ \\    \\ \\ \\         __       ___ ___       __  \\ \\,\\L\\_\\\\ \\ \\___       __     _ __   _____   \r\n \\ \\ \\  __\\ \\ \\  __  /'__`\\   /' __` __`\\   /'__`\\ \\/_\\__ \\ \\ \\  _ `\\   /'__`\\  /\\`'__\\/\\ '__`\\ \r\n  \\ \\ \\L\\ \\\\ \\ \\L\\ \\/\\ \\L\\.\\_ /\\ \\/\\ \\/\\ \\ /\\ \\L\\.\\_ /\\ \\L\\ \\\\ \\ \\ \\ \\ /\\ \\L\\.\\_\\ \\ \\/ \\ \\ \\L\\ \\\r\n   \\ \\____/ \\ \\____/\\ \\__/.\\_\\\\ \\_\\ \\_\\ \\_\\\\ \\__/.\\_\\\\ `\\____\\\\ \\_\\ \\_\\\\ \\__/.\\_\\\\ \\_\\  \\ \\ ,__/\r\n    \\/___/   \\/___/  \\/__/\\/_/ \\/_/\\/_/\\/_/ \\/__/\\/_/ \\/_____/ \\/_/\\/_/ \\/__/\\/_/ \\/_/   \\ \\ \\/ \r\n                                                                                          \\ \\_\\ \r\n                                                                                           \\/_/ ");

Console.WriteLine("======================================================================================================");



Console.WriteLine();

    Console.WriteLine("The examples for new versions are under working now. We'll soon update the examples." +
        " Thank you for your support!");
    string modelPath = "D:\\development\\llama\\weights\\wizard-vicuna-13B.ggmlv3.q4_1.bin";
    var prompt = File.ReadAllText("Assets/chat-with-bob.txt").Trim();
    //string prompt = " Qeustion: how to do binary search for an array in C#? Answer: ";

    InteractiveExecutor ex = new(new LLamaModel(new ModelParams(modelPath, contextSize: 1024, seed: 1337, gpuLayerCount: 5)));

    ChatSession session = new ChatSession(ex).WithOutputTransform(new KeywordTextOutputStreamTransform(new string[] { "User:", "Bob:" }));

    while (prompt != "skip")
    {
        await foreach (var text in session.ChatAsync(prompt, new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "User:" } }, default(CancellationToken)))
        {
            Console.Write(text);
        }
        prompt = Console.ReadLine();
        if(prompt == "save")
        {
            session.SaveSession("./SessionState");
            Console.WriteLine("Saved session!");
            ex.Model.Dispose();
            ex = new(new LLamaModel(new ModelParams(modelPath, contextSize: 1024, seed: 1337, gpuLayerCount: 5)));
            session = new ChatSession(ex).WithOutputTransform(new KeywordTextOutputStreamTransform(new string[] { "User:", "Bob:" }));
            session.LoadSession("./SessionState");
            Console.WriteLine("Loaded session!");
            prompt = Console.ReadLine();
        }
    }

    ex.Model.Dispose();

    //StatelessExecutor ex = new(new LLamaModel(new ModelParams(modelPath, contextSize: 256)));
    //while (true)
    //{
    //    foreach (var text in ex.Infer(prompt, new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "user:" }, MaxTokens = 256 }))
    //    {
    //        Console.Write(text);
    //    }
    //    prompt = Console.ReadLine();
    //}

    //LLama.Examples.NewVersion.SaveAndLoadState runner = new(modelPath, prompt);
    //while (true)
    //{
    //    var input = Console.ReadLine();
    //    if(input == "save")
    //    {
    //        Console.Write("Your path to save state: ");
    //        input = Console.ReadLine();
    //        runner.SaveState("./ex_state.json", input);
    //        runner.LoadState("./ex_state.json", input);
    //    }
    //    else
    //    {
    //        runner.Run(input);
    //    }
    //}
