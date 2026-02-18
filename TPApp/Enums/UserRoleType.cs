namespace TPApp.Enums
{
    /// <summary>
    /// User role types in the technology transfer workflow
    /// </summary>
    public enum UserRoleType
    {
        /// <summary>
        /// Buyer - Organization requesting technology transfer
        /// </summary>
        Buyer = 1,

        /// <summary>
        /// Seller - Technology provider/supplier
        /// </summary>
        Seller = 2,

        /// <summary>
        /// Consultant - Technical advisor assigned to projects
        /// </summary>
        Consultant = 3,

        /// <summary>
        /// Admin - System administrator with full access
        /// </summary>
        Admin = 4
    }
}
