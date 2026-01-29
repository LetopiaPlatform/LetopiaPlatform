# Letopia AI Agents - Implementation Plan

> **Feature:** Multi-Agent Conversational AI Platform   
> **Teams:** Backend, AI, Frontend, Mobile

---

## 📋 Feature Overview

Build an **extensible multi-agent platform** supporting:
1. **Roadmap Agent** - Generates personalized learning roadmaps with verified URLs
2. **Community Helper Agent** *(Future)* - Assists with community questions and moderation
3. **Study Buddy Agent** *(Future)* - Provides interactive learning assistance
4. Common infrastructure for multi-turn conversations, tool calling, and conversation persistence

### Key Architectural Goals
- **Extensibility**: Add new agents without modifying existing code
- **Shared Infrastructure**: Common tools, caching, and conversation management
- **Unified API**: Single endpoint pattern for all agents (`/api/agent/{agentType}/chat`)
- **Isolated Prompts & Tools**: Each agent has its own prompts and specialized tools

---

## 🏛️ Multi-Agent Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                        API Layer                                  │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  POST /api/agent/{agentType}/chat                           │ │
│  │  POST /api/agent/{agentType}/chat/stream                    │ │
│  └─────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────┐
│                     AgentFactory                                  │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  GetAgent("roadmap") → RoadmapAgentService                  │ │
│  │  GetAgent("community") → CommunityAgentService              │ │
│  │  GetAgent("studyBuddy") → StudyBuddyAgentService           │ │
│  └─────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
                                │
         ┌──────────────────────┼──────────────────────┐
         ▼                      ▼                      ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ RoadmapAgent    │  │ CommunityAgent  │  │ StudyBuddyAgent │
├─────────────────┤  ├─────────────────┤  ├─────────────────┤
│ • System Prompt │  │ • System Prompt │  │ • System Prompt │
│ • RoadmapTools  │  │ • CommunityTools│  │ • QuizTools     │
│ • WebSearchTools│  │ • ModerationTool│  │ • FlashcardTools│
└────────┬────────┘  └────────┬────────┘  └────────┬────────┘
         │                    │                    │
         └────────────────────┼────────────────────┘
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│                   Shared Infrastructure                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐│
│  │ BaseAgent    │  │ SharedTools  │  │ ConversationCache (Redis)││
│  │ (Abstract)   │  │ • WebSearch  │  │ • Thread management      ││
│  └──────────────┘  └──────────────┘  └──────────────────────────┘│
└──────────────────────────────────────────────────────────────────┘
```

---

## 🏗️ Project Structure (Multi-Agent)

```
LetopiaPlatform/
├── src/
│   ├── LetopiaPlatform.API/
│   │   ├── Controllers/
│   │   │   └── AgentController.cs         # Unified controller for all agents
│   │   └── DTOs/
│   │       └── Agent/
│   │           ├── Request/
│   │           │   └── ChatRequest.cs     # Includes agentType
│   │           └── Response/
│   │               └── ChatResponse.cs
│   │
│   ├── LetopiaPlatform.Agent/             # AI Agent Layer
│   │   ├── Abstractions/                  # NEW: Base interfaces & classes
│   │   │   ├── IAgentService.cs           # Generic agent interface
│   │   │   └── BaseAgentService.cs        # Shared logic (thread management, etc.)
│   │   │
│   │   ├── AgentFactory.cs                # NEW: Factory for agent resolution
│   │   │
│   │   ├── Agents/                        # NEW: Agent-specific folders
│   │   │   ├── RoadmapAgent/
│   │   │   │   ├── RoadmapAgentService.cs
│   │   │   │   ├── RoadmapTools.cs
│   │   │   │   └── RoadmapGeneratorPrompt.md
│   │   │   │
│   │   │   └── CommunityAgent/            # Future agent
│   │   │       ├── CommunityAgentService.cs
│   │   │       ├── CommunityTools.cs
│   │   │       └── CommunityHelperPrompt.md
│   │   │
│   │   ├── Configuration/
│   │   │   └── AgentSettings.cs
│   │   │
│   │   ├── Services/                      # Shared services
│   │   │   └── WebSearchService.cs
│   │   │
│   │   └── Tools/                         # Shared tools
│   │       └── WebSearchTools.cs
│   │
│   ├── LetopiaPlatform.Core/
│   │   ├── Entities/AIModels/
│   │   ├── Interfaces/
│   │   │   └── IConversationCache.cs
│   │   └── Services/Interfaces/
│   │       ├── IWebSearchService.cs
│   │       └── IConversationService.cs
│   │
│   └── LetopiaPlatform.Infrastructure/
│       ├── Data/
│       ├── Repositories/
│       └── Services/
│           ├── ConversationService.cs
│           └── RedisConversationCache.cs
```

---

## 🛠️ Technology Stack

| Component | Technology | Notes |
|-----------|------------|-------|
| **AI Framework** | Microsoft.Agents.AI.OpenAI | Microsoft Agent Framework for .NET |
| **LLM Provider** | GitHub Models (`models.github.ai`) | GPT-4o via GitHub Personal Access Token |
| **OpenAI SDK** | OpenAI (v2.8.0+) | Official OpenAI C# SDK |
| **Web Search** | Serper.dev API | Google Search for verified URLs (2,500 free) |
| **Caching** | StackExchange.Redis | Conversation state persistence |
| **Tracing** | OpenTelemetry | Observability & debugging |
| **Runtime** | .NET 8.0 | Cross-platform |

---

## Phase 0: Project Setup & Multi-Agent Foundation (Week 1)

### 0.1 NuGet Package Installation
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Add `Microsoft.Agents.AI.OpenAI` package | Backend | 🔴 High | `LetopiaPlatform.Agent.csproj` |
| Add `OpenAI` SDK package (v2.8.0+) | Backend | 🔴 High | `LetopiaPlatform.Agent.csproj` |
| Add `OpenTelemetry` packages | Backend | 🟡 Medium | `LetopiaPlatform.Agent.csproj` |
| Add `System.ClientModel` package | Backend | 🔴 High | `LetopiaPlatform.Agent.csproj` |

**Updated `.csproj` for Agent project:**
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-preview" />
  <PackageReference Include="OpenAI" Version="2.8.0" />
  <PackageReference Include="System.ClientModel" Version="1.4.0" />
  <PackageReference Include="OpenTelemetry" Version="1.11.2" />
  <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.11.2" />
  <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.11.2" />
</ItemGroup>
```

### 0.2 Multi-Agent Abstractions
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Create `IAgentService` interface | Backend | 🔴 High | `Agent/Abstractions/` |
| Create `BaseAgentService` abstract class | Backend | 🔴 High | `Agent/Abstractions/` |
| Create `IAgentFactory` interface | Backend | 🔴 High | `Agent/AgentFactory.cs` |
| Implement `AgentFactory` | Backend | 🔴 High | `Agent/AgentFactory.cs` |

**`IAgentService.cs` Implementation:**
```csharp
// Agent/Abstractions/IAgentService.cs
namespace LetopiaPlatform.Agent.Abstractions;

/// <summary>
/// Generic interface for all AI agents in the platform.
/// Implement this interface to create new agent types.
/// </summary>
public interface IAgentService
{
    /// <summary>Unique identifier for this agent type</summary>
    string AgentName { get; }
    
    /// <summary>Process user message and stream response</summary>
    IAsyncEnumerable<string> RunStreamingAsync(
        string userInput, 
        string threadId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>Process user message and return complete response</summary>
    Task<string> RunAsync(
        string userInput, 
        string threadId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>Create a new conversation thread</summary>
    string CreateNewThread();
}
```

**`BaseAgentService.cs` Implementation:**
```csharp
// Agent/Abstractions/BaseAgentService.cs
using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Abstractions;
using Microsoft.Agents.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace LetopiaPlatform.Agent.Abstractions;

/// <summary>
/// Base class for all AI agents. Provides shared functionality for:
/// - OpenAI client configuration (GitHub Models)
/// - Conversation thread management
/// - Cache integration
/// - Streaming response handling
/// </summary>
public abstract class BaseAgentService : IAgentService
{
    protected readonly AIAgent Agent;
    protected readonly IConversationCache Cache;
    protected readonly ILogger Logger;
    protected readonly AgentSettings Settings;
    
    public abstract string AgentName { get; }
    
    protected BaseAgentService(
        IOptions<AgentSettings> settings,
        IConversationCache cache,
        ILogger logger)
    {
        Settings = settings.Value;
        Cache = cache;
        Logger = logger;
        
        // Configure OpenAI client for GitHub Models
        var credential = new ApiKeyCredential(Settings.GitHubToken);
        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(Settings.GitHubModelsEndpoint)
        };
        var openAIClient = new OpenAIClient(credential, clientOptions);
        var chatClient = openAIClient.GetChatClient(Settings.ModelId);
        
        // Create AI Agent with derived class's prompt and tools
        Agent = chatClient.CreateAIAgent(GetSystemPrompt(), GetTools());
    }
    
    /// <summary>Override to provide agent-specific system prompt</summary>
    protected abstract string GetSystemPrompt();
    
    /// <summary>Override to provide agent-specific tools</summary>
    protected abstract IList<AIFunction> GetTools();
    
    public string CreateNewThread() => $"{AgentName}:{Guid.NewGuid()}";
    
    public async Task<string> RunAsync(
        string userInput, 
        string threadId, 
        CancellationToken cancellationToken = default)
    {
        var thread = await GetOrCreateThreadAsync(threadId);
        thread.AddUserMessage(userInput);
        
        var response = await Agent.RunAsync(thread, cancellationToken: cancellationToken);
        
        await SaveThreadAsync(threadId, thread);
        return response;
    }
    
    public async IAsyncEnumerable<string> RunStreamingAsync(
        string userInput, 
        string threadId, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var thread = await GetOrCreateThreadAsync(threadId);
        thread.AddUserMessage(userInput);
        
        await foreach (var chunk in Agent.RunStreamingAsync(thread, cancellationToken: cancellationToken))
        {
            yield return chunk;
        }
        
        await SaveThreadAsync(threadId, thread);
    }
    
    protected async Task<AgentThread> GetOrCreateThreadAsync(string threadId)
    {
        var messages = await Cache.GetConversationAsync(threadId);
        var thread = Agent.GetNewThread();
        
        foreach (var msg in messages)
        {
            if (msg.Role == "user")
                thread.AddUserMessage(msg.Content);
            else
                thread.AddAssistantMessage(msg.Content);
        }
        
        return thread;
    }
    
    protected async Task SaveThreadAsync(string threadId, AgentThread thread)
    {
        var messages = thread.Messages.Select(m => new ChatMessage
        {
            Role = m.Role == ChatMessageRole.User ? "user" : "assistant",
            Content = m.GetText()
        }).ToList();
        
        await Cache.SaveConversationAsync(threadId, messages);
    }
}
```

**`AgentFactory.cs` Implementation:**
```csharp
// Agent/AgentFactory.cs
using LetopiaPlatform.Agent.Abstractions;
using LetopiaPlatform.Agent.Agents.RoadmapAgent;
// using LetopiaPlatform.Agent.Agents.CommunityAgent; // Future
using Microsoft.Extensions.DependencyInjection;

namespace LetopiaPlatform.Agent;

public interface IAgentFactory
{
    /// <summary>Get an agent service by its type name</summary>
    IAgentService GetAgent(string agentType);
    
    /// <summary>Get all available agent types</summary>
    IEnumerable<string> GetAvailableAgents();
}

public class AgentFactory : IAgentFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    // Registry of available agents - add new agents here
    private static readonly Dictionary<string, Type> AgentRegistry = new(StringComparer.OrdinalIgnoreCase)
    {
        ["roadmap"] = typeof(RoadmapAgentService),
        // ["community"] = typeof(CommunityAgentService), // Future
        // ["studyBuddy"] = typeof(StudyBuddyAgentService), // Future
    };
    
    public AgentFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IAgentService GetAgent(string agentType)
    {
        if (!AgentRegistry.TryGetValue(agentType, out var serviceType))
        {
            throw new ArgumentException(
                $"Unknown agent type: '{agentType}'. Available agents: {string.Join(", ", AgentRegistry.Keys)}");
        }
        
        return (IAgentService)_serviceProvider.GetRequiredService(serviceType);
    }
    
    public IEnumerable<string> GetAvailableAgents() => AgentRegistry.Keys;
}
```

### 0.3 Configuration Setup
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Create `AgentSettings.cs` configuration class | Backend | 🔴 High | `Agent/Configuration/` |
| Add agent settings to `appsettings.json` | Backend | 🔴 High | `API/appsettings.json` |
| Configure User Secrets for GitHub Token | Backend | 🔴 High | User Secrets |
| Configure User Secrets for Serper API Key | Backend | 🔴 High | User Secrets |
| Create `TracingConfiguration.cs` | Backend | 🟡 Medium | `Agent/Configuration/` |

**`AgentSettings.cs` Implementation:**
```csharp
// Agent/Configuration/AgentSettings.cs
namespace LetopiaPlatform.Agent.Configuration;

public class AgentSettings
{
    public const string SectionName = "AgentSettings";
    
    public string GitHubToken { get; set; } = string.Empty;
    public string ModelId { get; set; } = "gpt-4o";
    public string GitHubModelsEndpoint { get; set; } = "https://models.github.ai/inference";
    public string SerperApiKey { get; set; } = string.Empty;
}
```

**`appsettings.json` Addition:**
```json
{
  "AgentSettings": {
    "ModelId": "gpt-4o",
    "GitHubModelsEndpoint": "https://models.github.ai/inference"
  }
}
```

**User Secrets (Development):**
```bash
# Initialize user secrets (already done - UserSecretsId added to .csproj)
cd src/LetopiaPlatform.API

# Set secrets
dotnet user-secrets set "AgentSettings:GitHubToken" "ghp_your_github_pat_here"
dotnet user-secrets set "AgentSettings:SerperApiKey" "your_serper_api_key_here"
```

### 0.4 Dependency Injection Registration (Multi-Agent)
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Create `AddAgentServices()` extension method | Backend | 🔴 High | `Agent/DependencyInjection.cs` |
| Register `AgentFactory` | Backend | 🔴 High | `Agent/DependencyInjection.cs` |
| Register all agent services | Backend | 🔴 High | `Agent/DependencyInjection.cs` |
| Update `Program.cs` | Backend | 🔴 High | `API/Program.cs` |

**`DependencyInjection.cs` for Agent (Multi-Agent):**
```csharp
// Agent/DependencyInjection.cs
using LetopiaPlatform.Agent.Abstractions;
using LetopiaPlatform.Agent.Agents.RoadmapAgent;
using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Agent.Services;

namespace LetopiaPlatform.Agent;

public static class DependencyInjection
{
    public static IServiceCollection AddAgentServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Bind settings
        services.Configure<AgentSettings>(
            configuration.GetSection(AgentSettings.SectionName));
        
        // Register shared services
        services.AddSingleton<IWebSearchService, WebSearchService>();
        
        // Register agent factory
        services.AddSingleton<IAgentFactory, AgentFactory>();
        
        // Register individual agents
        // Each agent is registered with its concrete type for factory resolution
        services.AddScoped<RoadmapAgentService>();
        // services.AddScoped<CommunityAgentService>(); // Future
        // services.AddScoped<StudyBuddyAgentService>(); // Future
        
        // Register conversation service
        services.AddScoped<IConversationService, ConversationService>();
        
        return services;
    }
}
```

**`Program.cs` Update:**
```csharp
// Add after builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAgentServices(builder.Configuration);
```

**Phase 0 Deliverables:**
- ✅ All NuGet packages installed
- ✅ Multi-agent abstractions created (`IAgentService`, `BaseAgentService`)
- ✅ Agent factory implemented
- ✅ Configuration classes created
- ✅ Services registered in DI container

---

## Phase 1: DTOs & Data Layer (Week 1-2)

### 1.1 API DTOs - Chat Communication (Multi-Agent)
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Create `ChatRequest` DTO (with agentType) | Backend | 🔴 High | `API/DTOs/Agent/Request/` |
| Create `ChatResponse` DTO with type discriminator | Backend | 🔴 High | `API/DTOs/Agent/Response/` |
| Create `ConversationThread` DTO | Backend | 🔴 High | `API/DTOs/Agent/Response/` |
| Update existing `RoadmapResponse` DTO | Backend | 🟡 Medium | `API/DTOs/Agent/Response/` |

**`ChatRequest.cs` Implementation (Multi-Agent):**
```csharp
// API/DTOs/Agent/Request/ChatRequest.cs
namespace LetopiaPlatform.API.DTOs.Agent.Request;

public class ChatRequest
{
    /// <summary>User's message to the AI agent</summary>
    public required string Message { get; set; }
    
    /// <summary>Thread ID for conversation continuity. Null = new conversation</summary>
    public string? ThreadId { get; set; }
}
```

**`ChatResponse.cs` Implementation (Multi-Agent):**
```csharp
// API/DTOs/Agent/Response/ChatResponse.cs
namespace LetopiaPlatform.API.DTOs.Agent.Response;

public class ChatResponse
{
    /// <summary>Agent type that generated this response</summary>
    public required string AgentType { get; set; }
    
    /// <summary>Response type: "text", "roadmap", "community", or "error"</summary>
    public required string Type { get; set; }
    
    /// <summary>Thread ID for conversation continuity</summary>
    public required string ThreadId { get; set; }
    
    /// <summary>Text content when Type="text"</summary>
    public string? TextContent { get; set; }
    
    /// <summary>Roadmap data when Type="roadmap" (RoadmapAgent only)</summary>
    public RoadmapResponse? Roadmap { get; set; }
    
    /// <summary>Error message when Type="error"</summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>Response timestamp</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### 1.2 Core Entities (Already Exist - Verify/Update)
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Verify `Roadmap.cs` entity structure | Backend | 🟡 Medium | `Core/Entities/AIModels/` |
| Add `Id`, `UserId`, `CreatedAt` to `Roadmap` for persistence | Backend | 🔴 High | `Core/Entities/AIModels/` |
| Verify `Phase.cs`, `Topic.cs`, `Resource.cs` entities | Backend | 🟡 Medium | `Core/Entities/AIModels/` |

**`Roadmap.cs` Updates (if needed):**
```csharp
// Add to existing Roadmap.cs
public Guid Id { get; set; } = Guid.NewGuid();
public Guid UserId { get; set; }
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime? UpdatedAt { get; set; }
```

### 1.3 Service Interfaces (Multi-Agent)
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| ~~Create `IRoadmapAgentService` interface~~ | ~~Backend~~ | ~~High~~ | Use `IAgentService` instead |
| Create `IConversationService` interface | Backend | 🔴 High | `Core/Services/Interfaces/` |
| Create `IWebSearchService` interface | Backend | 🔴 High | `Core/Services/Interfaces/` |

> **Note:** The specific agent interfaces are replaced by the generic `IAgentService` from the abstractions layer. Each agent implements `IAgentService` and provides its own `AgentName`.

**`IConversationService.cs` Implementation (Multi-Agent):**
```csharp
// Core/Services/Interfaces/IConversationService.cs
namespace LetopiaPlatform.Core.Services.Interfaces;

public interface IConversationService
{
    /// <summary>Process a message through the specified agent type</summary>
    Task<ChatResponse> ProcessMessageAsync(
        string agentType, 
        ChatRequest request, 
        string userId);
    
    /// <summary>Get available agent types</summary>
    IEnumerable<string> GetAvailableAgents();
}
```

**`IWebSearchService.cs` Implementation:**
```csharp
// Core/Services/Interfaces/IWebSearchService.cs
namespace LetopiaPlatform.Core.Services.Interfaces;

public interface IWebSearchService
{
    /// <summary>Search for a verified URL for a learning resource</summary>
    Task<string?> SearchResourceUrlAsync(string title, string platform, string topic);
    
    /// <summary>Validate if a URL is accessible</summary>
    Task<bool> ValidateUrlAsync(string url);
}
```

### 1.4 Caching Layer
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Create `IConversationCache` interface | Backend | 🔴 High | `Core/Interfaces/` |
| Implement `RedisConversationCache` | Backend | 🔴 High | `Infrastructure/Services/` |
| Define conversation TTL (24 hours) | Backend | 🟡 Medium | Configuration |

**`IConversationCache.cs` Implementation:**
```csharp
// Core/Interfaces/IConversationCache.cs
namespace LetopiaPlatform.Core.Interfaces;

public interface IConversationCache
{
    Task<List<ChatMessage>> GetConversationAsync(string threadId);
    Task SaveConversationAsync(string threadId, List<ChatMessage> messages);
    Task<bool> ThreadExistsAsync(string threadId);
    Task DeleteConversationAsync(string threadId);
}

public class ChatMessage
{
    public required string Role { get; set; }  // "user" or "assistant"
    public required string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

**Phase 1 Deliverables:**
- ✅ All DTOs defined with proper structure
- ✅ Service interfaces defined
- ✅ Redis caching layer ready

---

## 🤖 Phase 2: AI Agent Implementation (Week 2-3)

### 2.1 Shared Services - Web Search (For Verified URLs)
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Create `WebSearchService.cs` | Backend | 🔴 High | `Agent/Services/` |
| Implement Serper.dev API integration | Backend | 🔴 High | `Agent/Services/` |
| Add site filtering (YouTube, Udemy, etc.) | Backend | 🔴 High | `Agent/Services/` |
| Add URL validation logic | Backend | 🟡 Medium | `Agent/Services/` |

**`WebSearchService.cs` Implementation:**
```csharp
// Agent/Services/WebSearchService.cs
namespace LetopiaPlatform.Agent.Services;

public class WebSearchService : IWebSearchService
{
    private readonly HttpClient _httpClient;
    private readonly AgentSettings _settings;
    
    private static readonly Dictionary<string, string> PlatformSiteFilters = new()
    {
        ["YouTube"] = "site:youtube.com",
        ["Udemy"] = "site:udemy.com",
        ["Coursera"] = "site:coursera.org",
        ["Pluralsight"] = "site:pluralsight.com",
        ["FreeCodeCamp"] = "site:freecodecamp.org",
        ["MDN"] = "site:developer.mozilla.org"
    };
    
    public WebSearchService(IOptions<AgentSettings> settings, IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient();
    }
    
    public async Task<string?> SearchResourceUrlAsync(string title, string platform, string topic)
    {
        var siteFilter = PlatformSiteFilters.GetValueOrDefault(platform, "");
        var query = $"{title} {topic} {siteFilter}".Trim();
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://google.serper.dev/search")
        {
            Headers = { { "X-API-KEY", _settings.SerperApiKey } },
            Content = JsonContent.Create(new { q = query, num = 3 })
        };
        
        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        if (json.TryGetProperty("organic", out var results) && results.GetArrayLength() > 0)
        {
            return results[0].GetProperty("link").GetString();
        }
        
        return null;
    }
    
    public async Task<bool> ValidateUrlAsync(string url)
    {
        try
        {
            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, url));
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
```

### 2.2 Shared Tools
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Create `WebSearchTools.cs` | Backend | 🔴 High | `Agent/Tools/` |
| Implement `SearchResourceUrl` function | Backend | 🔴 High | `Agent/Tools/` |
| Implement `ValidateUrl` function | Backend | 🟡 Medium | `Agent/Tools/` |

**`WebSearchTools.cs` Implementation (Shared):**
```csharp
// Agent/Tools/WebSearchTools.cs
using System.ComponentModel;
using Microsoft.Agents.AI.Abstractions;

namespace LetopiaPlatform.Agent.Tools;

/// <summary>
/// Shared web search tools that can be used by any agent.
/// </summary>
public class WebSearchTools
{
    private readonly IWebSearchService _searchService;
    
    public WebSearchTools(IWebSearchService searchService)
    {
        _searchService = searchService;
    }
    
    [Description("Search for a verified URL for a learning resource. Returns the URL or 'Not found'.")]
    public async Task<string> SearchResourceUrl(
        [Description("The title of the resource (e.g., 'React Crash Course')")] string resourceTitle,
        [Description("The platform name (e.g., 'YouTube', 'Udemy', 'Coursera')")] string platform,
        [Description("The topic being searched (e.g., 'React', 'Node.js')")] string topic)
    {
        var url = await _searchService.SearchResourceUrlAsync(resourceTitle, platform, topic);
        return url ?? "Not found";
    }
    
    [Description("Validate if a URL is accessible and returns HTTP 200")]
    public async Task<string> ValidateUrl(
        [Description("The URL to validate")] string url)
    {
        var isValid = await _searchService.ValidateUrlAsync(url);
        return isValid ? "Valid" : "Invalid";
    }
}
```

### 2.3 Roadmap Agent Implementation ⭐ Using BaseAgentService
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Create `RoadmapAgentService.cs` | Backend | 🔴 High | `Agent/Agents/RoadmapAgent/` |
| Create `RoadmapTools.cs` | Backend | 🔴 High | `Agent/Agents/RoadmapAgent/` |
| Create `RoadmapGeneratorPrompt.md` | AI | 🔴 High | `Agent/Agents/RoadmapAgent/` |

**`RoadmapAgentService.cs` Implementation (Extends BaseAgentService):**
```csharp
// Agent/Agents/RoadmapAgent/RoadmapAgentService.cs
using LetopiaPlatform.Agent.Abstractions;
using LetopiaPlatform.Agent.Configuration;
using LetopiaPlatform.Agent.Tools;
using Microsoft.Agents.AI.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LetopiaPlatform.Agent.Agents.RoadmapAgent;

public class RoadmapAgentService : BaseAgentService
{
    private readonly IWebSearchService _searchService;
    private readonly IRoadmapService _roadmapService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public override string AgentName => "roadmap";
    
    public RoadmapAgentService(
        IOptions<AgentSettings> settings,
        IWebSearchService searchService,
        IRoadmapService roadmapService,
        IHttpContextAccessor httpContextAccessor,
        IConversationCache cache,
        ILogger<RoadmapAgentService> logger)
        : base(settings, cache, logger)
    {
        _searchService = searchService;
        _roadmapService = roadmapService;
        _httpContextAccessor = httpContextAccessor;
    }
    
    protected override string GetSystemPrompt()
    {
        var promptPath = Path.Combine(
            AppContext.BaseDirectory, 
            "Agents", "RoadmapAgent", "RoadmapGeneratorPrompt.md");
        return File.ReadAllText(promptPath);
    }
    
    protected override IList<AIFunction> GetTools()
    {
        // Shared tools
        var webSearchTools = new WebSearchTools(_searchService);
        
        // Agent-specific tools
        var roadmapTools = new RoadmapTools(_roadmapService, _httpContextAccessor);
        
        return new List<AIFunction>
        {
            AIFunctionFactory.Create(webSearchTools.SearchResourceUrl),
            AIFunctionFactory.Create(webSearchTools.ValidateUrl),
            AIFunctionFactory.Create(roadmapTools.SaveRoadmap)
        };
    }
}
```

**`RoadmapTools.cs` Implementation (Agent-Specific):**
```csharp
// Agent/Agents/RoadmapAgent/RoadmapTools.cs
using System.ComponentModel;
using System.Text.Json;

namespace LetopiaPlatform.Agent.Agents.RoadmapAgent;

/// <summary>
/// Tools specific to the Roadmap Agent.
/// </summary>
public class RoadmapTools
{
    private readonly IRoadmapService _roadmapService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public RoadmapTools(IRoadmapService roadmapService, IHttpContextAccessor httpContextAccessor)
    {
        _roadmapService = roadmapService;
        _httpContextAccessor = httpContextAccessor;
    }
    
    [Description("Save the approved roadmap to the database for the current user")]
    public async Task<string> SaveRoadmap(
        [Description("The roadmap JSON to save")] string roadmapJson)
    {
        try
        {
            var roadmap = JsonSerializer.Deserialize<Roadmap>(roadmapJson);
            if (roadmap == null) return "Error: Invalid roadmap JSON";
            
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return "Error: User not authenticated";
            
            roadmap.UserId = Guid.Parse(userId);
            await _roadmapService.SaveAsync(roadmap);
            
            return $"Roadmap '{roadmap.Title}' saved successfully!";
        }
        catch (Exception ex)
        {
            return $"Error saving roadmap: {ex.Message}";
        }
    }
}
```

### 2.4 Adding a New Agent (Template) ⭐ How to Extend
To add a new agent (e.g., Community Helper Agent):

1. **Create folder**: `Agent/Agents/CommunityAgent/`
2. **Create service**: Extend `BaseAgentService`
3. **Create tools**: Agent-specific tools
4. **Create prompt**: `CommunityHelperPrompt.md`
5. **Register in factory**: Add to `AgentRegistry` in `AgentFactory.cs`
6. **Register in DI**: Add `services.AddScoped<CommunityAgentService>()`

**Example `CommunityAgentService.cs` Template:**
```csharp
// Agent/Agents/CommunityAgent/CommunityAgentService.cs
namespace LetopiaPlatform.Agent.Agents.CommunityAgent;

public class CommunityAgentService : BaseAgentService
{
    public override string AgentName => "community";
    
    public CommunityAgentService(
        IOptions<AgentSettings> settings,
        ICommunityService communityService, // Agent-specific dependency
        IConversationCache cache,
        ILogger<CommunityAgentService> logger)
        : base(settings, cache, logger)
    {
        // Store agent-specific dependencies
    }
    
    protected override string GetSystemPrompt()
    {
        var promptPath = Path.Combine(
            AppContext.BaseDirectory, 
            "Agents", "CommunityAgent", "CommunityHelperPrompt.md");
        return File.ReadAllText(promptPath);
    }
    
    protected override IList<AIFunction> GetTools()
    {
        // Return agent-specific tools
        return new List<AIFunction>
        {
            AIFunctionFactory.Create(/* community-specific tools */)
        };
    }
}
```

**Phase 2 Deliverables:**
- ✅ Shared web search service configured
- ✅ Roadmap Agent implemented using `BaseAgentService`
- ✅ Agent-specific tools created
- ✅ System prompt ready
- ✅ Template for adding new agents documented

---

## 🔌 Phase 3: API Layer (Week 3-4)

### 3.1 Conversation Service (Multi-Agent)
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Implement `ConversationService.cs` | Backend | 🔴 High | `Infrastructure/Services/` |
| Use `AgentFactory` to resolve agents | Backend | 🔴 High | |
| Implement thread creation/retrieval | Backend | 🔴 High | |
| Implement chat history persistence (Redis) | Backend | 🔴 High | |
| Implement response type detection | Backend | 🔴 High | |

**`ConversationService.cs` Implementation (Multi-Agent):**
```csharp
// Infrastructure/Services/ConversationService.cs
using LetopiaPlatform.Agent;

namespace LetopiaPlatform.Infrastructure.Services;

public class ConversationService : IConversationService
{
    private readonly IAgentFactory _agentFactory;
    private readonly IConversationCache _cache;
    
    public ConversationService(IAgentFactory agentFactory, IConversationCache cache)
    {
        _agentFactory = agentFactory;
        _cache = cache;
    }
    
    public IEnumerable<string> GetAvailableAgents() => _agentFactory.GetAvailableAgents();
    
    public async Task<ChatResponse> ProcessMessageAsync(
        string agentType, 
        ChatRequest request, 
        string userId)
    {
        // Get the appropriate agent from factory
        var agent = _agentFactory.GetAgent(agentType);
        
        // Create new thread if not provided
        var threadId = request.ThreadId ?? agent.CreateNewThread();
        
        try
        {
            var response = await agent.RunAsync(request.Message, threadId);
            
            // Detect response type based on agent and content
            var responseType = DetectResponseType(agentType, response);
            
            return new ChatResponse
            {
                AgentType = agentType,
                Type = responseType,
                ThreadId = threadId,
                TextContent = responseType == "text" ? response : null,
                Roadmap = responseType == "roadmap" ? ParseRoadmap(response) : null
            };
        }
        catch (Exception ex)
        {
            return new ChatResponse
            {
                AgentType = agentType,
                Type = "error",
                ThreadId = threadId,
                ErrorMessage = ex.Message
            };
        }
    }
    
    private string DetectResponseType(string agentType, string response)
    {
        // Agent-specific response type detection
        if (agentType == "roadmap" && TryParseRoadmapJson(response, out _))
        {
            return "roadmap";
        }
        
        // Future: Add detection for other agent types
        // if (agentType == "community" && ...) return "community_response";
        
        return "text";
    }
    
    private bool TryParseRoadmapJson(string response, out Roadmap? roadmap)
    {
        roadmap = null;
        
        var jsonMatch = Regex.Match(response, @"```json\s*([\s\S]*?)\s*```");
        if (!jsonMatch.Success)
        {
            if (!response.TrimStart().StartsWith("{")) return false;
            jsonMatch = Regex.Match(response, @"(\{[\s\S]*\})");
        }
        
        if (!jsonMatch.Success) return false;
        
        try
        {
            roadmap = JsonSerializer.Deserialize<Roadmap>(
                jsonMatch.Groups[1].Value,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return roadmap != null;
        }
        catch
        {
            return false;
        }
    }
    
    private RoadmapResponse? ParseRoadmap(string response)
    {
        if (TryParseRoadmapJson(response, out var roadmap) && roadmap != null)
        {
            return MapToRoadmapResponse(roadmap);
        }
        return null;
    }
}
```

### 3.2 Update Agent Controller (Multi-Agent Routes)
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Update `POST /api/agent/{agentType}/chat` | Backend | 🔴 High | `API/Controllers/AgentController.cs` |
| Add `POST /api/agent/{agentType}/chat/stream` (SSE) | Backend | 🟡 Medium | `API/Controllers/AgentController.cs` |
| Add `GET /api/agent/types` (list available agents) | Backend | 🟡 Medium | `API/Controllers/AgentController.cs` |
| Add `GET /api/agent/thread/{threadId}` | Backend | 🟡 Medium | `API/Controllers/AgentController.cs` |
| Add `DELETE /api/agent/thread/{threadId}` | Backend | 🟢 Low | `API/Controllers/AgentController.cs` |

**`AgentController.cs` Implementation (Multi-Agent):**
```csharp
// API/Controllers/AgentController.cs
using LetopiaPlatform.Agent;

namespace LetopiaPlatform.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AgentController : BaseController
{
    private readonly IConversationService _conversationService;
    private readonly IAgentFactory _agentFactory;
    private readonly IConversationCache _cache;
    
    public AgentController(
        IConversationService conversationService,
        IAgentFactory agentFactory,
        IConversationCache cache)
    {
        _conversationService = conversationService;
        _agentFactory = agentFactory;
        _cache = cache;
    }
    
    /// <summary>Get available agent types</summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetAgentTypes()
    {
        return Ok(_conversationService.GetAvailableAgents());
    }
    
    /// <summary>Send a message to a specific agent type</summary>
    [HttpPost("{agentType}/chat")]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromRoute] string agentType,
        [FromBody] ChatRequest request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _conversationService.ProcessMessageAsync(agentType, request, userId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>Send a message and stream the response (SSE)</summary>
    [HttpPost("{agentType}/chat/stream")]
    public async IAsyncEnumerable<string> ChatStream(
        [FromRoute] string agentType,
        [FromBody] ChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var agent = _agentFactory.GetAgent(agentType);
        var threadId = request.ThreadId ?? agent.CreateNewThread();
        
        // Send thread ID and agent type first
        yield return $"data: {{\"agentType\": \"{agentType}\", \"threadId\": \"{threadId}\"}}\n\n";
        
        await foreach (var chunk in agent.RunStreamingAsync(
            request.Message, threadId, cancellationToken))
        {
            yield return $"data: {JsonSerializer.Serialize(new { content = chunk })}\n\n";
        }
        
        yield return "data: [DONE]\n\n";
    }
    
    /// <summary>Get conversation history for a thread</summary>
    [HttpGet("thread/{threadId}")]
    public async Task<ActionResult<List<ChatMessage>>> GetThread(string threadId)
    {
        var messages = await _cache.GetConversationAsync(threadId);
        if (messages.Count == 0)
            return NotFound();
        return Ok(messages);
    }
    
    /// <summary>Delete a conversation thread</summary>
    [HttpDelete("thread/{threadId}")]
    public async Task<IActionResult> DeleteThread(string threadId)
    {
        await _cache.DeleteConversationAsync(threadId);
        return NoContent();
    }
}
```

### 3.3 API Routes Summary (Multi-Agent)
| HTTP Method | Route | Description |
|-------------|-------|-------------|
| `GET` | `/api/agent/types` | List available agent types |
| `POST` | `/api/agent/{agentType}/chat` | Send message to specific agent |
| `POST` | `/api/agent/{agentType}/chat/stream` | Stream response from agent (SSE) |
| `GET` | `/api/agent/thread/{threadId}` | Get conversation history |
| `DELETE` | `/api/agent/thread/{threadId}` | Delete conversation |

**Example API Calls:**
```http
### Get available agents
GET /api/agent/types

### Chat with Roadmap Agent
POST /api/agent/roadmap/chat
Content-Type: application/json

{
    "message": "I want to learn React in 3 months",
    "threadId": null
}

### Chat with Community Agent (future)
POST /api/agent/community/chat
Content-Type: application/json

{
    "message": "How do I ask good questions in the community?",
    "threadId": null
}
```

### 3.3 Roadmap CRUD Endpoints
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Implement `RoadmapService.cs` | Backend | 🔴 High | `Infrastructure/Services/` |
| Add `GET /api/roadmap/{id}` | Backend | 🔴 High | `API/Controllers/` |
| Add `GET /api/roadmap/user/me` | Backend | 🔴 High | `API/Controllers/` |
| Add `PUT /api/roadmap/{id}` | Backend | 🟡 Medium | `API/Controllers/` |
| Add `DELETE /api/roadmap/{id}` | Backend | 🟡 Medium | `API/Controllers/` |

### 3.4 Database Integration
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Add `Roadmap` DbSet to ApplicationDbContext | Backend | 🔴 High | `Infrastructure/Data/` |
| Create EF Core migration | Backend | 🔴 High | `Infrastructure/Migrations/` |
| Implement `RoadmapRepository` | Backend | 🔴 High | `Infrastructure/Repositories/` |

**Phase 3 Deliverables:**
- ✅ All API endpoints working
- ✅ Conversation state persisted in Redis
- ✅ Roadmaps saved to PostgreSQL

---

## 📱 Phase 4: Client Integration (Week 4-5)

### 4.1 API Documentation
| Task | Owner | Priority | Dependencies |
|------|-------|----------|--------------|
| Add Swagger/OpenAPI annotations | Backend | 🔴 High | Endpoints |
| Generate TypeScript types for Frontend | Backend | 🔴 High | OpenAPI spec |
| Generate Dart types for Mobile | Backend | 🔴 High | OpenAPI spec |

### 4.2 Frontend Integration (Web)
| Task | Owner | Priority | Dependencies |
|------|-------|----------|--------------|
| Create chat UI component | Frontend | 🔴 High | - |
| Implement message state management | Frontend | 🔴 High | - |
| Handle `type: "text"` responses | Frontend | 🔴 High | DTOs |
| Handle `type: "roadmap"` responses | Frontend | 🔴 High | DTOs |
| Create roadmap visualization component | Frontend | 🔴 High | - |
| Add loading/streaming states | Frontend | 🟡 Medium | - |

### 4.3 Mobile Integration (Flutter)
| Task | Owner | Priority | Dependencies |
|------|-------|----------|--------------|
| Create chat screen | Mobile | 🔴 High | - |
| Implement conversation state (Provider/Riverpod) | Mobile | 🔴 High | - |
| Handle response type switching | Mobile | 🔴 High | DTOs |
| Create roadmap graph widget | Mobile | 🔴 High | - |

**Client Response Handling:**
```typescript
// Frontend/Mobile pseudocode
interface ChatResponse {
  type: 'text' | 'roadmap' | 'error';
  threadId: string;
  textContent?: string;
  roadmap?: RoadmapResponse;
  errorMessage?: string;
}

function handleResponse(response: ChatResponse) {
  switch (response.type) {
    case 'text':
      displayChatMessage(response.textContent);
      break;
    case 'roadmap':
      displayRoadmapGraph(response.roadmap);
      showSavePrompt();
      break;
    case 'error':
      showErrorToast(response.errorMessage);
      break;
  }
}
```

**Phase 4 Deliverables:**
- ✅ Web chat interface working
- ✅ Mobile chat interface working
- ✅ Roadmap visualization on both platforms

---

## 🧪 Phase 5: Testing (Week 5-6)

### 5.1 Unit Tests
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Test `WebSearchService` | Backend | 🔴 High | `UnitTests/` |
| Test `ConversationService` | Backend | 🔴 High | `UnitTests/` |
| Test `RoadmapAgentService` (mock LLM) | Backend | 🔴 High | `UnitTests/` |
| Test response type detection | Backend | 🔴 High | `UnitTests/` |

### 5.2 Integration Tests
| Task | Owner | Priority | File/Location |
|------|-------|----------|---------------|
| Test `/api/agent/chat` endpoint | Backend | 🔴 High | `IntegrationTests/` |
| Test conversation persistence | Backend | 🔴 High | `IntegrationTests/` |
| Test roadmap save flow | Backend | 🔴 High | `IntegrationTests/` |

### 5.3 AI/Prompt Testing
| Task | Owner | Priority | Dependencies |
|------|-------|----------|--------------|
| Test prompt with edge cases | AI | 🔴 High | Prompts |
| Validate JSON output consistency | AI | 🔴 High | - |
| Test function calling reliability | AI | 🔴 High | Tools |

**Phase 5 Deliverables:**
- ✅ >80% code coverage
- ✅ All integration tests passing
- ✅ Prompt tested with various inputs

---

## 🚀 Phase 6: Deployment (Week 6-7)

### 6.1 Environment Configuration
| Task | Owner | Priority | Dependencies |
|------|-------|----------|--------------|
| Configure GitHub token in staging secrets | DevOps | 🔴 High | - |
| Configure Serper API key in staging | DevOps | 🔴 High | - |
| Set up Redis in staging | DevOps | 🔴 High | - |
| Configure monitoring (Application Insights) | DevOps | 🟡 Medium | - |

### 6.2 Deployment
| Task | Owner | Priority | Dependencies |
|------|-------|----------|--------------|
| Deploy to staging | DevOps | 🔴 High | All tests pass |
| Run smoke tests | QA | 🔴 High | Staging |
| Deploy to production | DevOps | 🔴 High | Staging OK |
| Monitor error rates | DevOps | 🔴 High | Production |

**Phase 6 Deliverables:**
- ✅ Staging deployment successful
- ✅ Production deployment successful
- ✅ Monitoring in place

---

## Phase 7: Post-Launch (Week 7+)

| Task | Owner | Priority |
|------|-------|----------|
| Monitor LLM token usage & costs | AI/DevOps | 🔴 High |
| Analyze conversation success rates | AI | 🔴 High |
| Gather user feedback | Product | 🟡 Medium |
| Optimize prompts based on feedback | AI | 🟡 Medium |
| Add roadmap sharing feature | Backend | 🟢 Low |

---

## ⚠️ Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| LLM response inconsistency | High | Strict JSON schema in prompt, validation layer |
| GitHub Models rate limits | Medium | Implement retry with exponential backoff |
| Broken resource URLs | High | Serper.dev web search + URL validation |
| Redis cache failures | Medium | Fallback to in-memory cache |
| Slow roadmap generation | Medium | Streaming responses, loading UI |

---

## ✅ Definition of Done

A task is **DONE** when:
- [ ] Code written and follows project coding standards
- [ ] Unit tests written and passing
- [ ] Code reviewed and approved
- [ ] Documentation updated (if applicable)
- [ ] Works in local development environment
- [ ] No critical bugs

---

## 🔗 Quick Links

- [Sample System Prompt](../../src/LetopiaPlatform.Agent/Prompts/sample.md)
- [API DTOs](../../src/LetopiaPlatform.API/DTOs/Agent/)
- [Agent Services](../../src/LetopiaPlatform.Agent/Services/)
