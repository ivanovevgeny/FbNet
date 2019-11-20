namespace FbNet.Exception
{
    public enum FbErrorCodes
    {
        AppToManyCalls = 4,
        UserToManyCalls = 17,
        PageToManyCalls = 32,
        CustomLevelThrottling = 613,
        PermissionDenied = 10,
        TokenExpired = 190,
        MissionPermission = 200,
        AppLimitReached = 341,
        TemporarilyBlockedForPoliciesViolations = 368,
        DuplicatePost= 506,
        ErrorPostingLink = 1609005
    }
    
    public enum FbErrorAuthSubcodes
    {
        AppNotInstalled = 458,
        UserCheckpointed = 459,
        PasswordChanged = 460,
        Expired = 463,
        UnconfirmedUser = 464,
        InvalidAccessToken = 467,
        InvalidSession = 492
    }
}