using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Repositories;

namespace FeedbackSystem.API.Services
{
    public class InsightsService : IInsightsService
    {
        private readonly IUserRepository _userRepo;
        private readonly IFeedbackRepository _feedbackRepo;
        private readonly IRecognitionRepository _recognitionRepo;

        public InsightsService(IUserRepository userRepo, IFeedbackRepository feedbackRepo, IRecognitionRepository recognitionRepo)
        {
            _userRepo = userRepo;
            _feedbackRepo = feedbackRepo;
            _recognitionRepo = recognitionRepo;
        }

        public async Task<PagedResult<FeedbackItemDto>> GetFeedbackGivenAsync(
            string requesterUserId, bool isAdmin,
            string userId, DateTime? from, DateTime? to, string? categoryId, string? search, int page, int pageSize, CancellationToken ct)
        {
            await EnsureUserExists(userId, ct);
            var managerDeptId = isAdmin ? null : await GetDepartmentId(requesterUserId, ct);
            if (!isAdmin) await EnsureManagerCanSeeUser(managerDeptId, userId, ct);

            NormalizePaging(ref page, ref pageSize);
            var (items, total) = await _feedbackRepo.GetGivenAsync(userId, from, to, categoryId, search, page, pageSize, ct);
            return new PagedResult<FeedbackItemDto>(page, pageSize, total, items);
        }

        public async Task<PagedResult<FeedbackItemDto>> GetFeedbackReceivedAsync(
            string requesterUserId, bool isAdmin,
            string userId, DateTime? from, DateTime? to, string? categoryId, string? search, int page, int pageSize, CancellationToken ct)
        {
            await EnsureUserExists(userId, ct);
            var managerDeptId = isAdmin ? null : await GetDepartmentId(requesterUserId, ct);
            if (!isAdmin) await EnsureManagerCanSeeUser(managerDeptId, userId, ct);

            NormalizePaging(ref page, ref pageSize);
            var (items, total) = await _feedbackRepo.GetReceivedAsync(userId, from, to, categoryId, search, page, pageSize, ct);
            return new PagedResult<FeedbackItemDto>(page, pageSize, total, items);
        }

        public async Task<PagedResult<RecognitionItemDto>> GetRecognitionsGivenAsync(
            string requesterUserId, bool isAdmin,
            string userId, DateTime? from, DateTime? to, string? search, int page, int pageSize, CancellationToken ct)
        {
            await EnsureUserExists(userId, ct);
            var managerDeptId = isAdmin ? null : await GetDepartmentId(requesterUserId, ct);
            if (!isAdmin) await EnsureManagerCanSeeUser(managerDeptId, userId, ct);

            NormalizePaging(ref page, ref pageSize);
            var (items, total) = await _recognitionRepo.GetGivenAsync(userId, from, to, search, page, pageSize, ct);
            return new PagedResult<RecognitionItemDto>(page, pageSize, total, items);
        }

        public async Task<PagedResult<RecognitionItemDto>> GetRecognitionsReceivedAsync(
            string requesterUserId, bool isAdmin,
            string userId, DateTime? from, DateTime? to, string? search, int page, int pageSize, CancellationToken ct)
        {
            await EnsureUserExists(userId, ct);
            var managerDeptId = isAdmin ? null : await GetDepartmentId(requesterUserId, ct);
            if (!isAdmin) await EnsureManagerCanSeeUser(managerDeptId, userId, ct);

            NormalizePaging(ref page, ref pageSize);
            var (items, total) = await _recognitionRepo.GetReceivedAsync(userId, from, to, search, page, pageSize, ct);
            return new PagedResult<RecognitionItemDto>(page, pageSize, total, items);
        }

        public async Task<UserInsightSummaryDto> GetSummaryAsync(
            string requesterUserId, bool isAdmin, string userId, CancellationToken ct)
        {
            await EnsureUserExists(userId, ct);
            var managerDeptId = isAdmin ? null : await GetDepartmentId(requesterUserId, ct);
            if (!isAdmin) await EnsureManagerCanSeeUser(managerDeptId, userId, ct);

            var fbTask = _feedbackRepo.GetSummaryAsync(userId, ct);
            var rcTask = _recognitionRepo.GetSummaryAsync(userId, ct);
            await Task.WhenAll(fbTask, rcTask);

            var fb = fbTask.Result;
            var rc = rcTask.Result;

            return new UserInsightSummaryDto(
                userId,
                fb.Given, fb.Received,
                rc.Given, rc.Received,
                fb.LatestGivenAt, fb.LatestReceivedAt,
                rc.LatestGivenAt, rc.LatestReceivedAt
            );
        }

        public async Task<PagedResult<FeedbackItemDto>> GetAllFeedbackAsync(
            string requesterUserId, bool isAdmin, FeedbackAllFilter filter, CancellationToken ct)
        {
            NormalizePaging(ref filter);

            string? deptScope = null;
            if (!isAdmin)
                deptScope = await GetDepartmentId(requesterUserId, ct);

            var (items, total) = await _feedbackRepo.GetAllAsync(
                filter.From, filter.To, filter.CategoryId, filter.Search, deptScope,
                filter.FromUserId, filter.ToUserId, filter.Page, filter.PageSize, ct);

            return new PagedResult<FeedbackItemDto>(filter.Page, filter.PageSize, total, items);
        }

        public async Task<PagedResult<RecognitionItemDto>> GetAllRecognitionsAsync(
            string requesterUserId, bool isAdmin, RecognitionAllFilter filter, CancellationToken ct)
        {
            NormalizePaging(ref filter);

            string? deptScope = null;
            if (!isAdmin)
                deptScope = await GetDepartmentId(requesterUserId, ct);

            var (items, total) = await _recognitionRepo.GetAllAsync(
                filter.From, filter.To, filter.Search, deptScope,
                filter.FromUserId, filter.ToUserId, filter.Page, filter.PageSize, ct);

            return new PagedResult<RecognitionItemDto>(filter.Page, filter.PageSize, total, items);
        }

        // ✅ Count-only across users
        public async Task<CountResultDto> GetAllFeedbackCountAsync(
            string requesterUserId, bool isAdmin, FeedbackAllFilter filter, CancellationToken ct)
        {
            string? deptScope = null;
            if (!isAdmin)
                deptScope = await GetDepartmentId(requesterUserId, ct);

            var total = await _feedbackRepo.CountAllAsync(
                filter.From, filter.To, filter.CategoryId, filter.Search, deptScope,
                filter.FromUserId, filter.ToUserId, ct);

            return new CountResultDto(total);
        }

        public async Task<CountResultDto> GetAllRecognitionsCountAsync(
            string requesterUserId, bool isAdmin, RecognitionAllFilter filter, CancellationToken ct)
        {
            string? deptScope = null;
            if (!isAdmin)
                deptScope = await GetDepartmentId(requesterUserId, ct);

            var total = await _recognitionRepo.CountAllAsync(
                filter.From, filter.To, filter.Search, deptScope,
                filter.FromUserId, filter.ToUserId, ct);

            return new CountResultDto(total);
        }

        // ✅ Category-based statistics
        public async Task<IReadOnlyList<CategoryStatsDto>> GetFeedbackByCategoryAsync(
            string requesterUserId, bool isAdmin, CategoryStatsFilter filter, CancellationToken ct)
        {
            string? deptScope = null;
            if (!isAdmin)
                deptScope = await _userRepo.GetDepartmentIdAsync(requesterUserId, ct);

            var targetDept = filter.DepartmentId ?? deptScope;
            return await _feedbackRepo.GetByCategoryAsync(filter.From, filter.To, targetDept, filter.UserId, ct);
        }

        public async Task<IReadOnlyList<RecognitionBadgeStatsDto>> GetRecognitionsByBadgeAsync(
            string requesterUserId, bool isAdmin, CategoryStatsFilter filter, CancellationToken ct)
        {
            string? deptScope = null;
            if (!isAdmin)
                deptScope = await _userRepo.GetDepartmentIdAsync(requesterUserId, ct);

            var targetDept = filter.DepartmentId ?? deptScope;
            return await _recognitionRepo.GetByBadgeAsync(filter.From, filter.To, targetDept, filter.UserId, ct);
        }

        // ✅ Parameterless by-category (all data, no filters)
        public async Task<IReadOnlyList<CategoryStatsDto>> GetFeedbackByCategoryAsync(CancellationToken ct)
        {
            return await _feedbackRepo.GetByCategoryAsync(null, null, null, null, ct);
        }

        public async Task<IReadOnlyList<RecognitionBadgeStatsDto>> GetRecognitionsByBadgeAsync(CancellationToken ct)
        {
            return await _recognitionRepo.GetByBadgeAsync(null, null, null, null, ct);
        }

        // -------------- helpers --------------
        private async Task EnsureUserExists(string userId, CancellationToken ct)
        {
            var exists = await _userRepo.UserExistsAsync(userId, ct);
            if (!exists) throw new KeyNotFoundException($"User {userId} not found.");
        }

        private async Task<string> GetDepartmentId(string userId, CancellationToken ct)
        {
            return await _userRepo.GetDepartmentIdAsync(userId, ct);
        }

        private async Task EnsureManagerCanSeeUser(string? managerDeptId, string targetUserId, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(managerDeptId)) throw new UnauthorizedAccessException("Manager department scope missing.");
            var userDepId = await _userRepo.GetDepartmentIdAsync(targetUserId, ct);

            if (string.IsNullOrEmpty(userDepId) || userDepId != managerDeptId)
                throw new UnauthorizedAccessException("You are not allowed to view this user's data.");
        }

        private static void NormalizePaging(ref int page, ref int pageSize)
        {
            page = page < 1 ? 1 : page;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;
        }

        private static void NormalizePaging(ref FeedbackAllFilter f)
        {
            var p = f.Page; var s = f.PageSize;
            NormalizePaging(ref p, ref s);
            f = f with { Page = p, PageSize = s };
        }

        private static void NormalizePaging(ref RecognitionAllFilter f)
        {
            var p = f.Page; var s = f.PageSize;
            NormalizePaging(ref p, ref s);
            f = f with { Page = p, PageSize = s };
        }
    }
}