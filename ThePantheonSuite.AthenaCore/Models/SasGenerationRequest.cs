namespace ThePantheonSuite.AthenaCore.Models;

/// <summary>
/// Model representing a request to generate SAS tokens with authorization validation logic.
/// </summary>
/// <remarks>
/// This model enforces group membership validation during property access rather than construction.
/// The authorization checks occur implicitly when accessing <see cref="SelectedGroupId"/> property,
/// ensuring runtime validation against the authenticated user's claims.
/// </remarks>
public class SasGenerationRequest
{
    private ClaimsPrincipal? _principal;
    private readonly string? _selectedGroupId;

    /// <summary>
    /// Gets or initializes the selected group identifier requiring authorization validation.
    /// </summary>
    /// <exception cref="Exception">Thrown when principal is null or user lacks required group membership.</exception>
    /// <remarks>
    /// This property performs runtime validation against the principal's claims.
    /// The getter checks if the user belongs to the requested group via Azure AD groups claim validation.
    /// </remarks>
    public required string SelectedGroupId
    {
        get
        {
            if (_principal is null)
            {
                _logger.LogWarning("Principal not set when accessing SelectedGroupId");
                throw new Exception("Principal has not been set or is null");
            }

            var groupsClaim = _principal.FindFirst("groups")?.Value;
            
            if (string.IsNullOrEmpty(groupsClaim))
            {
                _logger.LogWarning("Groups claim missing during SelectedGroupId validation");
                throw new Exception();
            }

            var groups = groupsClaim.Split(",", StringSplitOptions.RemoveEmptyEntries);
            
            if (!groups.Contains(_selectedGroupId))
            {
                _logger.LogWarning("User lacks required group membership for SelectedGroupId {_SelectedGroupId}", _selectedGroupId);
                throw new Exception();
            }

            _logger.LogInformation("Group validation successful for SelectedGroupId {_SelectedGroupId}", _selectedGroupId);
            
            return _selectedGroupId!;
        }
        init => _selectedGroupId = value;
    }

    /// <summary>
    /// Gets the user identifier from Azure AD claims.
    /// </summary>
    /// <exception cref="Exception">Thrown when principal is null or required claim is missing.</exception>
    /// <remarks>
    /// This property extracts the Azure AD object identifier claim from the principal.
    /// </remarks>
    public string UserId
    {
        get
        {
            if (_principal is null)
            {
                _logger.LogWarning("Principal not set when accessing UserId");
                throw new Exception("Principal has not been set or is null");
            }

            var claim = _principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            
            if (claim is null)
            {
                _logger.LogWarning("Missing required objectidentifier claim");
                throw new Exception("Claim for oid has not been set");
            }

            _logger.LogDebug("Retrieved user identifier: {UserId}", claim);
            
            return claim;
        }
    }

    /// <summary>
    /// Gets or initializes a flag indicating if access should be public.
    /// </summary>
    public bool IsPublic { get; init; }

    private readonly ILogger<SasGenerationRequest> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SasGenerationRequest"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for tracking authorization events.</param>
    public SasGenerationRequest(ILogger<SasGenerationRequest> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sets the principal used for authorization validation.
    /// </summary>
    /// <param name="principal">The authenticated user principal.</param>
    public void SetPrincipal(ClaimsPrincipal principal)
    {
        _principal = principal;
        
        _logger.LogInformation("Set principal for SAS generation request");
    }
}