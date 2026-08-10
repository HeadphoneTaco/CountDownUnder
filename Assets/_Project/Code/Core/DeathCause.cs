/// <summary>
/// Why a run ended badly. Each cause has its own ending screen, so the player learns
/// what killed them from the art rather than having to guess.
/// </summary>
public enum DeathCause
{
    /// <summary>Drained dry. Should have fed more.</summary>
    BloodLoss = 0,

    /// <summary>Caught out when the sun came up. Should have been faster.</summary>
    Sunrise = 1
}
