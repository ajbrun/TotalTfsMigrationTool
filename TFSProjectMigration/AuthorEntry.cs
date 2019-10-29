using System;
using System.Collections.Generic;

namespace TFSProjectMigration
{
    public class AuthorEntry : IEquatable<AuthorEntry>
    {
        public string UserName { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }

        public bool Equals(AuthorEntry other)
        {
            return other.UserName == UserName;
        }

        public override bool Equals(object obj)
        {
            var e = obj as AuthorEntry;
            if (e == null)
                return false;
            return Equals(e);
        }

        public override int GetHashCode()
        {
            return 404878561 + EqualityComparer<string>.Default.GetHashCode(UserName);
        }
    }

}
