namespace PgSafeExport.Masking;

public static class MaskerFactory
{
    public static IMasker Create(string? rule, string salt)
    {
        return rule?.Trim().ToLowerInvariant() switch
        {
            "fake_email" => new EmailMasker(salt),
            "fake_name" => new NameMasker(salt),
            "fake_phone" => new PhoneMasker(salt),
            "mask_last4" => new Last4Masker(),
            "redact" => new ConstantMasker("[REDACTED]"),
            "null" => new ConstantMasker(""),
            "json_redact" => new JsonRedactMasker(),
            _ => new PassthroughMasker()
        };
    }
}
