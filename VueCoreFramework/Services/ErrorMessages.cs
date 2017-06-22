namespace VueCoreFramework.Services
{
    public static class ErrorMessages
    {
        public const string AddItemError = "Item could not be added.";
        public const string AdminOnlyError = "Only an administrator may perform that action.";
        public const string AlreadyLockedError = "The account you specified is already locked.";
        public const string AlreadyUnlockedError = "The account you specified is not locked.";
        public const string AuthProviderError = "There was a problem authorizing with that provider.";
        public const string ChangeEmailLimitError = "You may not change the email on your account more than once per day.";
        public const string ConfirmEmailLoginError = "You must have a confirmed email to log in. Please check your email for your confirmation link. If you've lost the email, please register again.";
        public const string ConfirmEmailRegisterError = "An account with this email has already been registered, but your email address has not been confirmed. A new link has just been sent, in case the last one got lost. Please check your spam if you don't see it after a few minutes.";
        public const string DataError = "Data could not be accessed. Please refresh the page before trying again.";
        public const string DeleteLimitError = "You may not delete your account less than one day after changing the email on the account.";
        public const string DuplicateEmailError = "An account with this email already exists. If you've forgotten your password, please use the link on the login page.";
        public const string DuplicateUsernameError = "This username is already in use.";
        public const string InvalidDataTypeError = "An error occurred while trying to access this data. Please refresh the page before trying again.";
        public const string InvalidLogin = "Invalid login attempt.";
        public const string InvalidTargetUserError = "There was a problem with the account you specified.";
        public const string InvalidUserError = "Your login appears to be invalid or out of date. Please try to log in again.";
        public const string MissingIdError = "The item ID was missing from your request. Please refresh the page before trying again.";
        public const string MissingPropError = "The property name was missing from your request. Please refresh the page before trying again.";
        public const string NotForAdminsError = "That action is not valid for administrator accounts.";
        public const string SaveItemError = "Item could not be saved.";
        public const string RemoveItemError = "Item could not be removed.";
        public const string RemoveItemsError = "One or more items could not be removed.";

        public const string PermissionAction_AddNew = "add new items of this type";
        public const string PermissionAction_EditItem = "edit this item";
        public const string PermissionAction_ViewItem = "view this item";
        public const string PermissionAction_ViewItems = "view items of this type";
        public const string PermissionAction_RemoveItem = "remove this item";
        public const string PermissionAction_RemoveItems = "remove one or more of these items";

        public static string NoPermission(string action) => $"You don't have permission to {action}.";

        public static string LockedAccount(string email) => $"Your account has been locked. Please contact an administrator at {email} for assistance.";
    }
}
