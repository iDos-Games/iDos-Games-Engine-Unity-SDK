using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class UserCustomDataService
    {
        // ===================== Events =====================

        public static event Action<UserCustomDataDefinitions> OnDefinitionsLoaded;
        public static event Action<GetMyUserCustomDataResponse> OnMyDataLoaded;
        public static event Action<GetPublicUserCustomDataResponse> OnPublicDataLoaded;
        public static event Action<SetUserCustomDataResponse> OnPrivateDataSet;
        public static event Action<SetUserCustomDataResponse> OnPublicDataSet;
        public static event Action OnKeyDeleted;
        public static event Action<BatchSetUserCustomDataResponse> OnBatchSet;
        public static event Action<BatchDeleteUserCustomDataResponse> OnBatchDeleted;
        public static event Action<BatchGetPublicUserCustomDataResponse> OnBatchPublicDataLoaded;

        // ===================== Helpers =====================

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static UserCustomDataRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ===================== Actions =====================

        /// <summary>
        /// Fetches the title-level UserCustomData definitions (schema, limits).
        /// On success, patches local TitleConfig and fires OnDefinitionsLoaded.
        /// </summary>
        public static async Task<OperationResult<UserCustomDataDefinitions>> GetUserCustomDataDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await UserCustomDataAPI.GetUserCustomDataDefinitions(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchUserCustomData(result.Data);
                OnDefinitionsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches the current player's Custom Data (Private + Public + ReadOnly buckets).
        /// On success, patches local UserData and fires OnMyDataLoaded.
        /// </summary>
        public static async Task<OperationResult<GetMyUserCustomDataResponse>> GetMyUserCustomData()
        {
            var request = CreateBaseRequest();
            var result = await UserCustomDataAPI.GetMyUserCustomData(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyUserCustomData(result.Data);
                OnMyDataLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches the Public bucket of another player. Fires OnPublicDataLoaded on success.
        /// </summary>
        public static async Task<OperationResult<GetPublicUserCustomDataResponse>> GetPublicUserCustomDataOf(string targetUserID)
        {
            if (string.IsNullOrWhiteSpace(targetUserID))
            {
                Debug.LogWarning("[UserCustomDataService] GetPublicUserCustomDataOf: TargetUserID is required.");
                return OperationResult<GetPublicUserCustomDataResponse>.Fail("TargetUserID is required.");
            }

            var request = CreateBaseRequest();
            request.TargetUserID = targetUserID.Trim();

            var result = await UserCustomDataAPI.GetPublicUserCustomDataOf(request);

            if (result.Success && result.Data != null)
            {
                OnPublicDataLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Writes a value to the current player's Private bucket.
        /// On success, patches local UserData and fires OnPrivateDataSet.
        /// </summary>
        public static async Task<OperationResult<SetUserCustomDataResponse>> SetPrivateData(string keyID, string value)
        {
            if (!ValidateKeyID(keyID, nameof(SetPrivateData), out var keyError))
                return OperationResult<SetUserCustomDataResponse>.Fail(keyError);
            if (value == null)
            {
                Debug.LogWarning("[UserCustomDataService] SetPrivateData: Value is required (use DeleteKey to remove).");
                return OperationResult<SetUserCustomDataResponse>.Fail("Value is required.");
            }

            var request = CreateBaseRequest();
            request.KeyID = keyID.Trim();
            request.Value = value;

            var result = await UserCustomDataAPI.SetPrivateData(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchUserCustomDataKey(
                    CustomDataBucket.Private, result.Data.KeyID, value,
                    result.Data.Version, result.Data.ExpiresAt);
                OnPrivateDataSet?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Writes a value to the current player's Public bucket.
        /// On success, patches local UserData and fires OnPublicDataSet.
        /// </summary>
        public static async Task<OperationResult<SetUserCustomDataResponse>> SetPublicData(string keyID, string value)
        {
            if (!ValidateKeyID(keyID, nameof(SetPublicData), out var keyError))
                return OperationResult<SetUserCustomDataResponse>.Fail(keyError);
            if (value == null)
            {
                Debug.LogWarning("[UserCustomDataService] SetPublicData: Value is required (use DeleteKey to remove).");
                return OperationResult<SetUserCustomDataResponse>.Fail("Value is required.");
            }

            var request = CreateBaseRequest();
            request.KeyID = keyID.Trim();
            request.Value = value;

            var result = await UserCustomDataAPI.SetPublicData(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchUserCustomDataKey(
                    CustomDataBucket.Public, result.Data.KeyID, value,
                    result.Data.Version, result.Data.ExpiresAt);
                OnPublicDataSet?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Deletes a key from the current player's Private or Public bucket.
        /// Idempotent: deleting a non-existent key succeeds. Fires OnKeyDeleted on success.
        /// </summary>
        public static async Task<OperationResult<SuccessResponse>> DeleteKey(string keyID, CustomDataBucket bucket)
        {
            if (!ValidateKeyID(keyID, nameof(DeleteKey), out var keyError))
                return OperationResult<SuccessResponse>.Fail(keyError);
            if (bucket != CustomDataBucket.Private && bucket != CustomDataBucket.Public)
            {
                Debug.LogWarning("[UserCustomDataService] DeleteKey: Client can only delete from Private or Public buckets.");
                return OperationResult<SuccessResponse>.Fail("Client can only delete from Private or Public buckets.");
            }

            var request = CreateBaseRequest();
            request.KeyID = keyID.Trim();
            request.Bucket = bucket;

            var result = await UserCustomDataAPI.DeleteKey(request);

            if (result.Success)
            {
                IDosGamesData.User.RemoveUserCustomDataKey(bucket, keyID.Trim());
                OnKeyDeleted?.Invoke();
            }

            return result;
        }

        /// <summary>
        /// Atomically writes multiple keys to Private/Public buckets.
        /// All-or-nothing: if any item fails validation the whole batch is rejected server-side.
        /// On success, patches local UserData and fires OnBatchSet.
        /// </summary>
        public static async Task<OperationResult<BatchSetUserCustomDataResponse>> BatchSet(List<UserCustomDataBatchSetItem> items)
        {
            if (items == null || items.Count == 0)
            {
                Debug.LogWarning("[UserCustomDataService] BatchSet: BatchSetItems is empty.");
                return OperationResult<BatchSetUserCustomDataResponse>.Fail("BatchSetItems is empty.");
            }

            var request = CreateBaseRequest();
            request.BatchSetItems = items;

            var result = await UserCustomDataAPI.BatchSet(request);

            if (result.Success && result.Data != null)
            {
                foreach (var item in items)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.KeyID)) continue;
                    var resultItem = result.Data.Results?.Find(r =>
                        r.Bucket == item.Bucket &&
                        string.Equals(r.KeyID, item.KeyID.Trim(), StringComparison.Ordinal));
                    if (resultItem == null) continue;

                    IDosGamesData.User.PatchUserCustomDataKey(
                        item.Bucket, resultItem.KeyID, item.Value,
                        resultItem.Version, resultItem.ExpiresAt);
                }

                OnBatchSet?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Atomically deletes multiple keys from Private/Public buckets.
        /// Non-existent keys are silently skipped. Fires OnBatchDeleted on success.
        /// </summary>
        public static async Task<OperationResult<BatchDeleteUserCustomDataResponse>> BatchDelete(List<UserCustomDataBatchDeleteItem> items)
        {
            if (items == null || items.Count == 0)
            {
                Debug.LogWarning("[UserCustomDataService] BatchDelete: BatchDeleteItems is empty.");
                return OperationResult<BatchDeleteUserCustomDataResponse>.Fail("BatchDeleteItems is empty.");
            }

            var request = CreateBaseRequest();
            request.BatchDeleteItems = items;

            var result = await UserCustomDataAPI.BatchDelete(request);

            if (result.Success)
            {
                foreach (var item in items)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.KeyID)) continue;
                    IDosGamesData.User.RemoveUserCustomDataKey(item.Bucket, item.KeyID.Trim());
                }

                OnBatchDeleted?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches the Public buckets of multiple players in one request.
        /// Missing players are reported in NotFoundUserIDs; the operation does not fail for them.
        /// Fires OnBatchPublicDataLoaded on success.
        /// </summary>
        public static async Task<OperationResult<BatchGetPublicUserCustomDataResponse>> BatchGetPublicUserCustomDataOf(List<string> targetUserIDs)
        {
            if (targetUserIDs == null || targetUserIDs.Count == 0)
            {
                Debug.LogWarning("[UserCustomDataService] BatchGetPublicUserCustomDataOf: TargetUserIDs is empty.");
                return OperationResult<BatchGetPublicUserCustomDataResponse>.Fail("TargetUserIDs is empty.");
            }

            var request = CreateBaseRequest();
            request.TargetUserIDs = targetUserIDs;

            var result = await UserCustomDataAPI.BatchGetPublicUserCustomDataOf(request);

            if (result.Success && result.Data != null)
            {
                OnBatchPublicDataLoaded?.Invoke(result.Data);
            }

            return result;
        }

        // ===================== Private helpers =====================

        private static bool ValidateKeyID(string keyID, string callerName, out string error)
        {
            if (string.IsNullOrWhiteSpace(keyID))
            {
                Debug.LogWarning($"[UserCustomDataService] {callerName}: KeyID is required.");
                error = "KeyID is required.";
                return false;
            }

            string trimmed = keyID.Trim();
            if (trimmed.Contains('.') || trimmed.Contains('$'))
            {
                Debug.LogWarning($"[UserCustomDataService] {callerName}: KeyID '{trimmed}' contains invalid characters ('.' or '$').");
                error = $"KeyID '{trimmed}' contains invalid characters ('.' or '$').";
                return false;
            }

            error = null;
            return true;
        }
    }
}
