namespace Implementations.OpenAI;

using System.Collections.Generic;

public class ChatCompletion
{
    public string Id { get; set; }
    public string Object { get; set; }
    public long Created { get; set; }
    public string Model { get; set; }
    public Usage Usage { get; set; }
    public List<Choice> Choices { get; set; }
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}

public class Choice
{
    public Message Message { get; set; }
    public FinishDetails FinishDetails { get; set; }
    public int Index { get; set; }
}

public class Message
{
    public string Role { get; set; }
    public string Content { get; set; }
}

public class FinishDetails
{
    public string Type { get; set; }
    public string Stop { get; set; }
}

