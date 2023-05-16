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
        /// Verifies that the user belongs to designated AD-group.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="ntGroups"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Exception ValidateADGroups(this ClaimsPrincipal user, string ntGroups = "")
        {
            bool exists = false;
            string allowedADGroups = AllowedADGroups;

            if (ntGroups.Length != 0)
                allowedADGroups = ntGroups;

            // If there is more than 1 designated NT/AD group in groupName, create an array with them.
            string[] arrGroupNames = allowedADGroups.Replace(",", ";").Split(';');

            // Fetch user identity.
            var identity = (WindowsIdentity?)user.Identity;
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

        /// <summary>
        /// Fetches current username.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetCurrentUser(this ClaimsPrincipal user)
        {
            string currentUser = user.Identity.Name;
            int index = currentUser.LastIndexOf('\\');
            string username = currentUser.Substring(++index);
            return username;
        }

        /// <summary>
        /// Returns a list with the AD-groups that the user belongs to.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<string> GetADGroups(this ClaimsPrincipal user)
        {
            var groups = new List<string>();
            var identity = (WindowsIdentity?)user.Identity;

            if (identity.Groups != null)
            {
                foreach (var group in identity.Groups)
                {
                    try
                    {
                        groups.Add(group.Translate(typeof(NTAccount)).ToString());
                    }
                    catch (Exception e)
                    {
                        // Ignored
                    }
                }
            }

            return groups;
        }
    }
}