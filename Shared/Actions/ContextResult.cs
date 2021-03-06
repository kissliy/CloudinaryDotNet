﻿namespace CloudinaryDotNet.Actions
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Results of context management.
    /// </summary>
    [DataContract]
    public class ContextResult : BaseResult
    {
        /// <summary>
        /// Public IDs of affected assets.
        /// </summary>
        [DataMember(Name = "public_ids")]
        public string[] PublicIds { get; protected set; }
    }
}
