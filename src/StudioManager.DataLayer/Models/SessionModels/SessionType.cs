namespace StudioManager.DataLayer.Models.SessionModels
{
    public enum SessionType
    {
        PreProduction,
        Recording,
        Overdubs,
        Editing,
        Mixing,
        Mastering,

        /// <summary>
        /// Full production day (recording and mixing mixed).
        /// </summary>
        Production,

        Other
    }
}
