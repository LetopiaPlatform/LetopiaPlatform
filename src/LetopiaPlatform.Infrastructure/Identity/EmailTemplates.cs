namespace LetopiaPlatform.Infrastructure.Identity;

internal static class EmailTemplates
{
    // ── Illustration file names ──
    public const string WelcomeIllustration = "welcome.svg";
    public const string VerifyIllustration = "verify.svg";
    public const string ResetPasswordIllustration = "reset-password.svg";
    public const string OnboardingIllustration = "onboarding.svg";

    // ── Default fallback name ──
    public const string DefaultUserName = "there";

    // ── Welcome ──
    public const string WelcomeSubject = "Welcome to LeTopia!";
    public const string WelcomeTitle = "Welcome to LeTopia!";
    public const string WelcomeBody =
        "<p>We're thrilled to have you join our community of curious minds, builders, and lifelong learners.</p>" +
        "<p>At LeTopia, you'll find everything you need to grow your skills, connect with like-minded people, " +
        "and explore exciting projects.</p>" +
        "<p>Please check your inbox for a verification code to activate your account.</p>";

    // ── Email Verification ──
    public const string VerifySubject = "Your Email Verification Code";
    public const string VerifyTitle = "Final Step to Join LeTopia";
    public const string VerifyBody =
        "<p>Thank you for signing up! To activate your account, please use the verification code below:</p>";
    public const string VerifyAfterCodeBody =
        "<p>Enter this code within the next 10 minutes to verify your email address.</p>" +
        "<p>This one-time code is essential to confirm your email and ensure the security of your account. " +
        "If you didn't request this code, please ignore this email.</p>";

    // ── Password Reset ──
    public const string ResetSubject = "Your Password Reset Code";
    public const string ResetTitle = "Reset Your LeTopia Password";
    public const string ResetBody =
        "<p>We received a request to reset the password for your LeTopia account. Use the code below to proceed:</p>";
    public const string ResetAfterCodeBody =
        "<p>Once you've entered the code, you'll be able to create a new, secure password for your account.</p>" +
        "<p>If you did not request a password reset, please ignore this email. Your password will remain unchanged.</p>";

    // ── Onboarding ──
    public const string OnboardingSubject = "Explore, Connect, and Grow in LeTopia!";
    public const string OnboardingTitle = "Explore, Connect, and Grow in LeTopia!";
    public const string OnboardingBody =
        "<p>Your email has been successfully verified – you're all set!</p>" +
        "<p>Here's what you can do next:</p>" +
        "<ul>" +
        "<li><strong>Explore Communities</strong> – Find groups that match your interests.</li>" +
        "<li><strong>Join Projects</strong> – Collaborate with others on exciting projects.</li>" +
        "<li><strong>Start Learning</strong> – Dive into curated learning paths.</li>" +
        "</ul>" +
        "<p>We can't wait to see what you'll achieve. Welcome aboard!</p>";
    public const string OnboardingButtonText = "Explore Now";
}
