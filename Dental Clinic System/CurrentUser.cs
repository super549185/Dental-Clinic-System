namespace Dental_Clinic_System
{
    /// <summary>
    /// Global session user holder
    /// </summary>
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static string Username { get; set; }
        public static string FullName { get; set; }
        public static string ClinicName { get; set; }
    }
}
