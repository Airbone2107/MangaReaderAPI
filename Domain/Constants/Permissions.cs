namespace Domain.Constants
{
    public static class Permissions
    {
        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
        }

        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Create = "Permissions.Roles.Create";
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";
        }

        // Thêm các module quyền hạn khác ở đây, ví dụ:
        // public static class Mangas
        // {
        //     public const string View = "Permissions.Mangas.View";
        //     public const string Edit = "Permissions.Mangas.Edit";
        // }
    }
} 