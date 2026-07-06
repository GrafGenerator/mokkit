namespace Mokkit.Example1.Integration.Tests.Helpers;

public static class VerifierSetup
{
    static VerifierSetup()
    {
        ClipboardAccept.Enable();
    }

    public static VerifySettings Default(Action<VerifySettings>? configure = null)
    {
        var settings = new VerifySettings();

        settings.DisableDiff();

        settings.DontScrubGuids();
        settings.DontIgnoreEmptyCollections();

        configure?.Invoke(settings);

        return settings;
    }
}