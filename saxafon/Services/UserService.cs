using saxafon.Data;
using Shared.DTO;
using saxafon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace saxafon.Services
{


    public interface IUserService {
        Task<IEnumerable<UserMinDTO>> GetAllUsers();
        Task<UserMinDTO> GetUserById(int id);
        Task<UserMinDTO> CreateUser(UserRegisterDTO userRegisterDTO);
        Task<UserResponseAuthDTO> LoginUser(UserLoginDTO userLoginDTO);

        /// <summary>Full profile + settings of an active user, or null if the user does not exist.</summary>
        Task<UserProfileDTO?> GetUserProfile(int userId);

        /// <summary>Updates name / username / email / phone, enforcing case-insensitive username and email uniqueness.</summary>
        Task<UserOperationResult<UserProfileDTO>> UpdateUserProfile(int userId, UpdateUserProfileDTO dto);

        /// <summary>Updates notification and appearance settings.</summary>
        Task<UserOperationResult<UserProfileDTO>> UpdateUserSettings(int userId, UpdateUserSettingsDTO dto);

        /// <summary>Verifies the current password, then stores a hash of the new one.</summary>
        Task<UserOperationResult<bool>> ChangePassword(int userId, ChangePasswordDTO dto);
    }
    public class UserService : IUserService
    {
        private readonly AppDbContext context;
        private readonly IPasswordHasher<User> passwordHasher;

        private static readonly Expression<Func<User, UserProfileDTO>> ToProfileExpression = usr => new UserProfileDTO(
            usr.Id,
            usr.first_name,
            usr.last_name,
            usr.email,
            usr.user_name,
            usr.phone_number,
            usr.role,
            usr.profile_image_url,
            usr.email_notifications_enabled,
            usr.reservation_reminders_enabled,
            usr.theme,
            usr.created_at,
            usr.updated_at,
            usr.last_time_active_at
        );
        private static readonly Func<User, UserProfileDTO> ToProfile = ToProfileExpression.Compile();


        public UserService(AppDbContext _context, IPasswordHasher<User> _passwordHasher) {
            context = _context;
            passwordHasher = _passwordHasher;
        }

        public async Task<IEnumerable<UserMinDTO>> GetAllUsers() {
            return await context.Users.Where(u => u.deleted_at == null).Select(usr => new UserMinDTO(
                usr.Id,
                usr.first_name,
                usr.last_name,
                usr.email,
                usr.role,
                usr.profile_image_url
            )).ToListAsync();
        }

        public async Task<UserMinDTO> GetUserById(int id) {
            return await context.Users.Where(u => u.Id == id && u.deleted_at == null).Select(usr => new UserMinDTO(
                usr.Id,
                usr.first_name,
                usr.last_name,
                usr.email,
                usr.role,
                usr.profile_image_url
            )).FirstOrDefaultAsync();
        }




        public async Task<UserMinDTO> CreateUser(UserRegisterDTO userRegisterDTO)
        {
            var user = new User
            {
                first_name = userRegisterDTO.first_name,
                last_name = userRegisterDTO.last_name,
                email = userRegisterDTO.email,
                user_name = userRegisterDTO.user_name,
                phone_number = userRegisterDTO.phone_number,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                last_time_active_at = DateTime.UtcNow,
                role = Shared.Role.USER
            };
            user.passwordHash = passwordHasher.HashPassword(user, userRegisterDTO.password);


            context.Users.Add(user);
            await context.SaveChangesAsync();
            return new UserMinDTO(
                user.Id,
                user.first_name,
                user.last_name,
                user.email,
                user.role,
                user.profile_image_url
            );
        }

        public async Task<UserResponseAuthDTO> LoginUser(UserLoginDTO userLoginDTO)
        {
            var user = await context.Users.Where(u => ((u.user_name == userLoginDTO.email_or_username)
            || (u.email == userLoginDTO.email_or_username) )
            && u.deleted_at == null).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }

            var verification = VerifyPassword(user, userLoginDTO.password);
            if (verification == PasswordVerificationResult.Failed)
            {
                return null;
            }
            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                // Transparently upgrade legacy plaintext / outdated hashes on successful login.
                user.passwordHash = passwordHasher.HashPassword(user, userLoginDTO.password);
                await context.SaveChangesAsync();
            }

            return new UserResponseAuthDTO(
                user.Id
            );
        }


        // ------------------------------------------------------------------
        // Profile & account settings
        // ------------------------------------------------------------------

        /// <inheritdoc />

        public async Task<UserProfileDTO?> GetUserProfile(int userId)
        {
            return await context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId && u.deleted_at == null)
                .Select(ToProfileExpression)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />

        public async Task<UserOperationResult<UserProfileDTO>> UpdateUserProfile(int userId, UpdateUserProfileDTO dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.deleted_at == null);
            if (user == null)
            {
                return UserOperationResult<UserProfileDTO>.NotFound($"User with id {userId} was not found.");
            }

            var userName = dto.user_name.Trim();
            var email = dto.email.Trim();

            var conflicts = await FindIdentityConflicts(userId, userName, email);
            if (conflicts.Count > 0)
            {
                return UserOperationResult<UserProfileDTO>.Conflict(BuildConflictMessage(conflicts), conflicts);
            }

            user.first_name = dto.first_name.Trim();
            user.last_name = dto.last_name.Trim();
            user.user_name = userName;
            user.email = email;
            user.phone_number = string.IsNullOrWhiteSpace(dto.phone_number) ? null : dto.phone_number.Trim();
            user.updated_at = DateTime.UtcNow;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Race: another request claimed the username/email between our check and the save.
                // The unique indexes on User reject it; re-run the check to report which field clashed.
                var raceConflicts = await FindIdentityConflicts(userId, userName, email);
                if (raceConflicts.Count > 0)
                {
                    return UserOperationResult<UserProfileDTO>.Conflict(BuildConflictMessage(raceConflicts), raceConflicts);
                }
                throw;
            }

            return UserOperationResult<UserProfileDTO>.Success(ToProfile(user));
        }

        /// <inheritdoc />

        public async Task<UserOperationResult<UserProfileDTO>> UpdateUserSettings(int userId, UpdateUserSettingsDTO dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.deleted_at == null);
            if (user == null)
            {
                return UserOperationResult<UserProfileDTO>.NotFound($"User with id {userId} was not found.");
            }

            user.email_notifications_enabled = dto.email_notifications_enabled;
            user.reservation_reminders_enabled = dto.reservation_reminders_enabled;
            user.theme = dto.theme;
            user.updated_at = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return UserOperationResult<UserProfileDTO>.Success(ToProfile(user));
        }

        /// <inheritdoc />

        public async Task<UserOperationResult<bool>> ChangePassword(int userId, ChangePasswordDTO dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.deleted_at == null);
            if (user == null)
            {
                return UserOperationResult<bool>.NotFound($"User with id {userId} was not found.");
            }

            if (VerifyPassword(user, dto.current_password) == PasswordVerificationResult.Failed)
            {
                return UserOperationResult<bool>.ValidationFailed(nameof(ChangePasswordDTO.current_password), "Current password is incorrect.");
            }

            if (VerifyPassword(user, dto.new_password) != PasswordVerificationResult.Failed)
            {
                return UserOperationResult<bool>.ValidationFailed(nameof(ChangePasswordDTO.new_password), "New password must be different from the current password.");
            }

            user.passwordHash = passwordHasher.HashPassword(user, dto.new_password);
            user.updated_at = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return UserOperationResult<bool>.Success(true);
        }


        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Returns field-keyed conflicts if <paramref name="userName"/> or <paramref name="email"/> is already used
        /// by a different user (case-insensitive). Soft-deleted users are deliberately included: the unique
        /// indexes on User cover every row, so their identifiers are still reserved at the database level.
        /// </summary>
        private async Task<Dictionary<string, string[]>> FindIdentityConflicts(int userId, string userName, string email)
        {
            var normalizedUserName = userName.ToLowerInvariant();
            var normalizedEmail = email.ToLowerInvariant();

            var clashes = await context.Users
                .AsNoTracking()
                .Where(u => u.Id != userId
                    && (u.user_name.ToLower() == normalizedUserName || u.email.ToLower() == normalizedEmail))
                .Select(u => new
                {
                    UserNameTaken = u.user_name.ToLower() == normalizedUserName,
                    EmailTaken = u.email.ToLower() == normalizedEmail
                })
                .ToListAsync();

            var conflicts = new Dictionary<string, string[]>();
            if (clashes.Any(c => c.UserNameTaken))
            {
                conflicts[nameof(UpdateUserProfileDTO.user_name)] = new[] { $"The username '{userName}' is already taken." };
            }
            if (clashes.Any(c => c.EmailTaken))
            {
                conflicts[nameof(UpdateUserProfileDTO.email)] = new[] { $"The email '{email}' is already registered to another account." };
            }
            return conflicts;
        }

        private static string BuildConflictMessage(IReadOnlyDictionary<string, string[]> conflicts) =>
            conflicts.Count > 1
                ? "Both the username and the email are already in use."
                : conflicts.Values.First()[0];

        /// <summary>
        /// Verifies <paramref name="providedPassword"/> against the stored value. Accounts created before hashing
        /// was introduced still hold the plaintext password; those are compared in constant time and reported as
        /// <see cref="PasswordVerificationResult.SuccessRehashNeeded"/> so callers can upgrade them to a hash.
        /// </summary>
        private PasswordVerificationResult VerifyPassword(User user, string providedPassword)
        {
            if (!IsIdentityPasswordHash(user.passwordHash))
            {
                var stored = Encoding.UTF8.GetBytes(user.passwordHash);
                var provided = Encoding.UTF8.GetBytes(providedPassword);
                return CryptographicOperations.FixedTimeEquals(stored, provided)
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : PasswordVerificationResult.Failed;
            }
            return passwordHasher.VerifyHashedPassword(user, user.passwordHash, providedPassword);
        }

        /// <summary>
        /// True if the value is a PasswordHasher output: base64 whose first byte is the format marker
        /// (0x00 = Identity v2, exactly 49 bytes; 0x01 = Identity v3, at least 61 bytes).
        /// </summary>
        private static bool IsIdentityPasswordHash(string stored)
        {
            if (string.IsNullOrEmpty(stored))
            {
                return false;
            }
            Span<byte> buffer = stackalloc byte[stored.Length];
            if (!Convert.TryFromBase64String(stored, buffer, out var written) || written == 0)
            {
                return false;
            }
            return (buffer[0] == 0x00 && written == 49) || (buffer[0] == 0x01 && written >= 61);
        }

    }



}
