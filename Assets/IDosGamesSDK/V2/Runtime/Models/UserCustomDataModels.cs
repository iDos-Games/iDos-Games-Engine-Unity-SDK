using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDosGames
{
    /// <summary>
    /// Request for all UserCustomData actions.
    /// <list type="bullet">
    ///   <item><b>SetPrivateData / SetPublicData:</b> KeyID, Value.</item>
    ///   <item><b>DeleteKey:</b> KeyID, Bucket (Private or Public only).</item>
    ///   <item><b>GetPublicUserCustomDataOf:</b> TargetUserID.</item>
    ///   <item><b>BatchSet:</b> BatchSetItems.</item>
    ///   <item><b>BatchDelete:</b> BatchDeleteItems.</item>
    ///   <item><b>BatchGetPublicUserCustomDataOf:</b> TargetUserIDs.</item>
    ///   <item><b>GetUserCustomDataDefinitions / GetMyUserCustomData:</b> no extra fields required.</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class UserCustomDataRequest : BaseRequest
    {
        /// <summary>Key identifier. Required for SetPrivateData, SetPublicData, DeleteKey. Must not contain '.' or '$'.</summary>
        public string KeyID;

        /// <summary>Value to write. Required for SetPrivateData, SetPublicData.</summary>
        public string Value;

        /// <summary>Bucket for DeleteKey (Private or Public only). Ignored by Set* actions.</summary>
        public CustomDataBucket Bucket;

        /// <summary>Target player ID for GetPublicUserCustomDataOf.</summary>
        public string TargetUserID;

        /// <summary>Items for BatchSet. All-or-nothing; duplicates per (Bucket, KeyID) are rejected.</summary>
        public List<UserCustomDataBatchSetItem> BatchSetItems;

        /// <summary>Items for BatchDelete. Non-existent keys are silently skipped.</summary>
        public List<UserCustomDataBatchDeleteItem> BatchDeleteItems;

        /// <summary>Target player IDs for BatchGetPublicUserCustomDataOf. Missing players go to NotFoundUserIDs.</summary>
        public List<string> TargetUserIDs;
    }

    // ===================== State models =====================

    /// <summary>
    /// Client-side mirror of UserCustomDataState stored in UserDataDocument.CustomData.
    /// Updated via direct patch operations, not via ResourceOperation.
    /// </summary>
    [Serializable]
    public class UserCustomDataState
    {
        /// <summary>Global mutation counter across all four buckets. Increment on every write/delete.</summary>
        public long Version = 0;

        /// <summary>Private bucket. Visible and writable by owner only.</summary>
        public Dictionary<string, UserCustomDataRecord> Private;

        /// <summary>Public bucket. Visible to any player via cross-user endpoint; writable by owner.</summary>
        public Dictionary<string, UserCustomDataRecord> Public;

        /// <summary>ReadOnly bucket. Visible to owner; written only by server code.</summary>
        public Dictionary<string, UserCustomDataRecord> ReadOnly;
    }

    /// <summary>
    /// One record in any Custom Data bucket.
    /// Privacy and writability are determined by which bucket the record lives in, not by fields on this type.
    /// </summary>
    [Serializable]
    public class UserCustomDataRecord
    {
        /// <summary>Serialized value (UTF-8 string). Format is defined by ValueType for schema-managed keys.</summary>
        public string Value;

        /// <summary>Server UTC time of the last write.</summary>
        public DateTime UpdatedAt;

        /// <summary>Per-key write counter. Increments on every successful overwrite of this key.</summary>
        public int Version;

        /// <summary>Who wrote this record last. For audit purposes; does not affect validation.</summary>
        public CustomDataWriter LastWriter;

        /// <summary>Expiry time (UTC). null means the record never expires.</summary>
        public DateTime? ExpiresAt;
    }

    // ===================== Config models =====================

    /// <summary>
    /// Title configuration for Custom User Data V2. Stored in TitlePublicConfigurationModel.UserCustomData.
    /// </summary>
    [Serializable]
    public class UserCustomDataDefinitions
    {
        /// <summary>Schema-managed key definitions keyed by KeyID. null means all keys are free-form.</summary>
        public Dictionary<string, UserCustomDataKeyDefinition> Keys;

        /// <summary>Default max value size in bytes for free-form keys. Recommended: 8192.</summary>
        public int DefaultMaxValueLengthBytes = 8192;

        /// <summary>Default TTL in seconds for free-form keys. null means no expiry.</summary>
        public int? DefaultTtlSeconds;

        /// <summary>Max keys per bucket (applies equally to all four buckets). Recommended: 128.</summary>
        public int MaxKeysPerBucket = 128;

        /// <summary>Total max size of all Custom Data V2 across all buckets per player, in bytes. Recommended: 1_500_000.</summary>
        public int MaxTotalSizeBytes = 1_500_000;

        /// <summary>If true, unregistered keys are rejected. If false, free-form keys are accepted with default limits.</summary>
        public bool RejectUnregisteredKeys = false;
    }

    /// <summary>
    /// Definition of one schema-managed Custom Data key.
    /// </summary>
    [Serializable]
    public class UserCustomDataKeyDefinition
    {
        /// <summary>Unique key identifier. Must not contain '.' or '$'.</summary>
        public string KeyID;

        /// <summary>Bucket this key is locked to. Client cannot write it to a different bucket.</summary>
        public CustomDataBucket Bucket;

        /// <summary>Expected value format. Validated on every write.</summary>
        public CustomDataValueType ValueType;

        /// <summary>Per-key max value size in bytes. Overrides DefaultMaxValueLengthBytes.</summary>
        public int MaxValueLengthBytes = 8192;

        /// <summary>Per-key TTL in seconds. null means no expiry.</summary>
        public int? TtlSeconds;

        /// <summary>
        /// Default value returned when the key is absent or expired.
        /// Not persisted to DB until explicitly written. null means absent key is returned as absent.
        /// </summary>
        public string DefaultValue;

        /// <summary>Developer description for documentation. Not sent to client in Get* operations.</summary>
        public string Description;
    }

    // ===================== DTO models =====================

    /// <summary>One item in a BatchSet request.</summary>
    [Serializable]
    public class UserCustomDataBatchSetItem
    {
        /// <summary>Target bucket. Client API allows Private and Public only.</summary>
        public CustomDataBucket Bucket;

        /// <summary>Key identifier. Must not contain '.' or '$'. Duplicates per (Bucket, KeyID) reject the whole batch.</summary>
        public string KeyID;

        /// <summary>Value to write.</summary>
        public string Value;
    }

    /// <summary>One item in a BatchDelete request.</summary>
    [Serializable]
    public class UserCustomDataBatchDeleteItem
    {
        /// <summary>Bucket to delete from. Client API allows Private and Public only.</summary>
        public CustomDataBucket Bucket;

        /// <summary>Key identifier. Must not contain '.' or '$'.</summary>
        public string KeyID;
    }

    // ===================== Response models =====================

    /// <summary>Response for GetMyUserCustomData. Contains Private, Public, ReadOnly buckets. Internal is never sent to client.</summary>
    [Serializable]
    public class GetMyUserCustomDataResponse
    {
        /// <summary>Global mutation counter at the time of read.</summary>
        public long Version;

        /// <summary>Private bucket records. Expired entries are filtered out. Default values for schema-managed keys are injected.</summary>
        public Dictionary<string, UserCustomDataRecord> Private = new();

        /// <summary>Public bucket records. Expired entries filtered; defaults injected.</summary>
        public Dictionary<string, UserCustomDataRecord> Public = new();

        /// <summary>ReadOnly bucket records. Expired entries filtered; defaults injected.</summary>
        public Dictionary<string, UserCustomDataRecord> ReadOnly = new();
    }

    /// <summary>Response for GetPublicUserCustomDataOf. Contains only the target player's Public bucket.</summary>
    [Serializable]
    public class GetPublicUserCustomDataResponse
    {
        /// <summary>ID of the target player.</summary>
        public string UserID;

        /// <summary>Global mutation counter of the target player at the time of read.</summary>
        public long Version;

        /// <summary>Public bucket records of the target player. Expired entries filtered; defaults injected.</summary>
        public Dictionary<string, UserCustomDataRecord> Public = new();
    }

    /// <summary>Response for SetPrivateData and SetPublicData.</summary>
    [Serializable]
    public class SetUserCustomDataResponse
    {
        /// <summary>Server UTC time when the write was applied.</summary>
        public DateTime ServerTimeUtc;

        /// <summary>Bucket that was written to.</summary>
        public CustomDataBucket Bucket;

        /// <summary>Key that was written.</summary>
        public string KeyID;

        /// <summary>Per-key version after write. Monotonically increasing; use for client cache invalidation.</summary>
        public int Version;

        /// <summary>Expiry time (UTC) if the key has a TTL, otherwise null.</summary>
        public DateTime? ExpiresAt;
    }

    /// <summary>Result for one item in a successful BatchSet response.</summary>
    [Serializable]
    public class UserCustomDataBatchSetResultItem
    {
        /// <summary>Bucket that was written to.</summary>
        public CustomDataBucket Bucket;

        /// <summary>Key that was written.</summary>
        public string KeyID;

        /// <summary>Per-key version after write.</summary>
        public int Version;

        /// <summary>Expiry time (UTC) if the key has a TTL, otherwise null.</summary>
        public DateTime? ExpiresAt;
    }

    /// <summary>Response for BatchSet. All-or-nothing: either all items are written or none (validation failure).</summary>
    [Serializable]
    public class BatchSetUserCustomDataResponse
    {
        /// <summary>Server UTC time when the batch was applied.</summary>
        public DateTime ServerTimeUtc;

        /// <summary>Write results per item, in the same order as the validated input items.</summary>
        public List<UserCustomDataBatchSetResultItem> Results = new();
    }

    /// <summary>Response for BatchDelete.</summary>
    [Serializable]
    public class BatchDeleteUserCustomDataResponse
    {
        /// <summary>Server UTC time when the batch was applied.</summary>
        public DateTime ServerTimeUtc;

        /// <summary>Number of keys actually deleted. 0 means no existing keys were found; no DB patch was performed.</summary>
        public int DeletedCount;
    }

    /// <summary>Response for BatchGetPublicUserCustomDataOf.</summary>
    [Serializable]
    public class BatchGetPublicUserCustomDataResponse
    {
        /// <summary>Public buckets for found players. Same contract as GetPublicUserCustomDataResponse per player.</summary>
        public List<GetPublicUserCustomDataResponse> Results = new();

        /// <summary>Player IDs from the request that were not found in the database.</summary>
        public List<string> NotFoundUserIDs = new();
    }

    // ===================== Enums =====================

    /// <summary>
    /// Storage bucket for Custom User Data V2. Defines both privacy (who reads) and writability (who writes).
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomDataBucket
    {
        /// <summary>Visible and writable by owner only.</summary>
        Private,

        /// <summary>Visible to any player via cross-user endpoint; owner can write.</summary>
        Public,

        /// <summary>Visible to owner; server-write only (tutorial flags, A/B segments, onboarding steps).</summary>
        ReadOnly,

        /// <summary>Not visible to client at all. Server jobs / admin / analytics only.</summary>
        Internal,
    }

    /// <summary>Expected value format for schema-managed keys. The value is always stored as a string.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomDataValueType
    {
        /// <summary>Any string. Validated by length only.</summary>
        String,

        /// <summary>Must parse as long via long.TryParse.</summary>
        Int,

        /// <summary>Must be "true" or "false" (case-insensitive via bool.TryParse).</summary>
        Bool,

        /// <summary>Must be valid JSON (any structure passing JToken.Parse). Size limited by MaxValueLengthBytes.</summary>
        Json,
    }

    /// <summary>Who last wrote a UserCustomDataRecord. For audit only; does not affect validation.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomDataWriter
    {
        /// <summary>Written by client via client API (SetPrivateData / SetPublicData / BatchSet).</summary>
        Client,

        /// <summary>Written by server code handling a client request (e.g. another action writing a ReadOnly flag).</summary>
        Server,

        /// <summary>Written by a background job, admin tool, or migration script.</summary>
        System,
    }

    /// <summary>Action enum for UserCustomData endpoint. Maps 1:1 to backend switch cases.</summary>
    public enum UserCustomDataAction
    {
        GetUserCustomDataDefinitions,
        GetMyUserCustomData,
        GetPublicUserCustomDataOf,
        SetPrivateData,
        SetPublicData,
        DeleteKey,
        BatchSet,
        BatchDelete,
        BatchGetPublicUserCustomDataOf,
    }
}
