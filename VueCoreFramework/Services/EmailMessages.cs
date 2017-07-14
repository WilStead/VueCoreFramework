namespace VueCoreFramework.Services
{
    /// <summary>
    /// A collection of string constants used to construct email messages.
    /// </summary>
    public class EmailMessages
    {
#pragma warning disable CS1591
        public const string PasswordResetEmailSubject = "Reset Password";
        public const string PasswordResetEmailBody = "Please reset your password by clicking here:";

        public const string ConfirmAccountEmailSubject = "Confirm your account";
        public const string ConfirmAccountEmailBody = "Please confirm your account by clicking this link:";

        public const string ConfirmEmailChangeSubject = "Confirm your email change";
        public const string ConfirmEmailChangeCancelBody = "A request was made to change the email address on your account from this email address to a new one. If this was a mistake, please click this link to reject the requested change:";
        public const string ConfirmEmailChangeBody = "Please confirm your email address change by clicking this link:";

        public const string ConfirmAccountDeletionSubject = "Confirm your account deletion";
        public const string ConfirmAccountDeletionBody = "A request was made to delete your account. If you wish to permanently delete your account, please click this link to confirm:";
        public const string ConfirmAccountDeletionBody2 = "If you did not initiate this action, please do not click the link. Instead, you should log into your account and change your password to prevent any further unauthorized use by clicking here:";

        public const string GroupInviteSubject = "You've been invited to join a group";
        public const string GroupInviteBody = "You've been invited to join the {0} group. If you would like to accept the invitation, please click this link to become a group member:";
#pragma warning restore CS1591
    }
}
