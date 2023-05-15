namespace DeepArtConsoleClient.Errors
{
    public enum AppErrorCode
    {
        Unknown = 1,
        InvalidSettings = 101,
        ConvertingError = 102,
        InputFilesNotFound = 110,
        GetEffectsFailed = 111,
        EffectListIsEmpty = 112,
        EffectNotFound = 113,
        ConvertationTimeout = 114
    }
}
