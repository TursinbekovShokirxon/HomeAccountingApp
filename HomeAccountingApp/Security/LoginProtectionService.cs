using Microsoft.Extensions.Caching.Memory;

namespace Application.Security
{
    public class LoginProtectionService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<LoginProtectionService> _logger;
        private const int MaxAttempts = 5;
        private static readonly TimeSpan BlockTime = TimeSpan.FromMinutes(5);

        public LoginProtectionService(IMemoryCache cache, ILogger<LoginProtectionService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        private string GetKey(string login) => $"login_attempts_{login}";

        public bool IsBlocked(string login)
        {
            var key = GetKey(login);
            if (_cache.TryGetValue(key, out LoginAttempt attempt))
            {
                return attempt.IsBlocked && attempt.BlockedUntil > DateTime.UtcNow;
            }
            return false;
        }

        public int RegisterFailedAttempt(string login)
        {
            var key = GetKey(login);
            var attempt = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = BlockTime;
                return new LoginAttempt();
            });

            attempt.Attempts++;
            if (attempt.Attempts >= MaxAttempts)
            {
                attempt.IsBlocked = true;
                attempt.BlockedUntil = DateTime.UtcNow.Add(BlockTime);
                _logger.LogWarning($"Логин {login} заблокирован на {attempt.BlockedUntil} времени");
            }

            _cache.Set(key, attempt, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = BlockTime
            });
            return attempt.Attempts;
        }

        public void ResetAttempts(string login)
        {
            _cache.Remove(GetKey(login));
        }

        private class LoginAttempt
        {
            public int Attempts { get; set; } = 0;
            public bool IsBlocked { get; set; } = false;
            public DateTime BlockedUntil { get; set; }
        }
    }

}
