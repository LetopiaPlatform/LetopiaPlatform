using FluentValidation.TestHelper;
using LetopiaPlatform.API.Validators;
using LetopiaPlatform.Core.DTOs.Agent;
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.UnitTests.Validators;

public class StartConversationRequestValidatorTests
{
    private readonly StartConversationRequestValidator _validator = new();

    [Fact]
    public void ValidInitialMessagePassesValidation()
    {
        var request = new StartConversationRequest("I want to learn C#");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyInitialMessageFailsValidation(string? message)
    {
        var request = new StartConversationRequest(message!);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InitialMessage)
            .WithErrorMessage("Initial message is required.");
    }

    [Fact]
    public void OversizedInitialMessageFailsValidation()
    {
        var request = new StartConversationRequest(new string('a', 1001));
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InitialMessage)
            .WithErrorMessage("Initial message must not exceed 1000 characters.");
    }

    [Fact]
    public void MaxLengthInitialMessagePassesValidation()
    {
        var request = new StartConversationRequest(new string('a', 1000));
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class SendMessageRequestValidatorTests
{
    private readonly SendMessageRequestValidator _validator = new();

    [Fact]
    public void ValidContentPassesValidation()
    {
        var request = new SendMessageRequest("Tell me more about phase 2");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyContentFailsValidation(string? content)
    {
        var request = new SendMessageRequest(content!);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Message content is required.");
    }

    [Fact]
    public void OversizedContentFailsValidation()
    {
        var request = new SendMessageRequest(new string('a', 2001));
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Message must not exceed 2000 characters.");
    }

    [Fact]
    public void MaxLengthContentPassesValidation()
    {
        var request = new SendMessageRequest(new string('a', 2000));
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdatePhaseStatusRequestValidatorTests
{
    private readonly UpdatePhaseStatusRequestValidator _validator = new();

    [Theory]
    [InlineData(PhaseStatus.NotStarted)]
    [InlineData(PhaseStatus.InProgress)]
    [InlineData(PhaseStatus.Completed)]
    public void ValidPhaseStatusPassesValidation(PhaseStatus status)
    {
        var request = new UpdatePhaseStatusRequest(status);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidEnumValueFailsValidation()
    {
        var request = new UpdatePhaseStatusRequest((PhaseStatus)999);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Invalid phase status.");
    }
}
