using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;

namespace Library.Validation
{
    public static class Validation
    {
        public static string AllowedADGroups { get; private set; }
        public static string ValidationSuccessMessage { get; private set; } = "Validation succeeded";
        public static string ValidationFailedMessage { get; private set; } = "Validation failed";

        /// <summary>
        /// Verifies that the user belongs to designated AD-group
        /// </summary>
        /// <param name="User"></param>
        /// <param name="NTGroups"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Exception ValidateADGroups(this ClaimsPrincipal User, string NTGroups = "")
        {
            bool exists = false;
            string allowedADGroups = AllowedADGroups;

            if (NTGroups.Length != 0)
                allowedADGroups = NTGroups;

            // If there is more than 1 designated NT/AD group in groupName, create an array with them.
            string[] arrGroupNames = allowedADGroups.Replace(",", ";").Split(';');

            // Fetch user identity.
            var identity = (WindowsIdentity?)User.Identity;
            if (identity.Groups != null)
            {
                if (allowedADGroups.Length != 0)
                {
                    // Check if user belongs to any of the designated AD-groups.
                    for (int i = 0; i < arrGroupNames.Length; i++)
                    {
                        if (arrGroupNames[i].Trim().Length != 0)
                        {
                            // Check if user belongs to AD-group
                            var groups = from sid in identity.Groups
                                         where sid.Translate(typeof(NTAccount)).Value == arrGroupNames[i].Trim()
                                         select sid.Translate(typeof(NTAccount)).Value;

                            if (groups.Count() != 0)
                            {
                                exists = true;
                                break;
                            }
                        }
                    }
                }
                if (!exists)
                    return new Exception(ValidationFailedMessage);

                else if (exists)
                    return new Exception(ValidationSuccessMessage);
            }

            throw new Exception("Unexpected validation error");
        }
    }
}