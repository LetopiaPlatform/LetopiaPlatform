namespace LetopiaPlatform.Agent.Abstractions;

/// <summary>
/// Defines the contract for an agent service that processes user input and manages conversation threads.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Gets the type identifier for this agent.
    /// </summary>
    string AgentType { get; }

    /// <summary>
    /// Executes the agent asynchronously with streaming output, returning results as they become available.
    /// </summary>
    /// <param name="userInput">The input text from the user to process.</param>
    /// <param name="threadId">The unique identifier of the conversation thread.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of string chunks representing the streaming response.</returns>
    IAsyncEnumerable<string> RunStreamingAsync(string userInput, string threadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the agent asynchronously and returns the complete response.
    /// </summary>
    /// <param name="userInput">The input text from the user to process.</param>
    /// <param name="threadId">The unique identifier of the conversation thread.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the agent's response.</returns>
    Task<string> RunAsync(string userInput, string threadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new conversation thread for the agent.
    /// </summary>
    /// <returns>The unique identifier of the newly created thread.</returns>
    string CreateNewThread();
}