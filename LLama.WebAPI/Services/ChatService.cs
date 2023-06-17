using LLama.Common;
using LLama.WebAPI.Models;

namespace LLama.WebAPI.Services;

public class ChatService
{
    private readonly ChatSession _session;

    public ChatService()
    {
        // new LLamaParams(model: @"", n_ctx: 512, interactive: true, repeat_penalty: 1.0f, verbose_prompt: false)
        var modelParams = new ModelParams("ggml-model-q4_0.bin");
        LLamaModel model = new LLamaModel(modelParams);
        var executor = new InteractiveExecutor(model);
        _session = new ChatSession(executor);
    }

    public string Send(SendMessageInput input)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(input.Text);

        Console.ForegroundColor = ConsoleColor.White;
        var outputs = _session.Chat(input.Text);
        var result = "";
        foreach (var output in outputs)
        {
            Console.Write(output);
            result += output;
        }

        return result;
    }
}
