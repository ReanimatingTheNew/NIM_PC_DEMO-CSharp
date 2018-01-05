#region License

// Copyright (c) Newtonsoft. All Rights Reserved.
// License: https://raw.github.com/JamesNK/Newtonsoft.Json.Schema/master/LICENSE.md

#endregion

namespace Newtonsoft.Json.Schema
{
    /// <summary>
    /// JSON Schema version.
    /// </summary>
    public enum SchemaVersion
    {
        /// <summary>
        /// Version has not been set.
        /// </summary>
        Unset,
        /// <summary>
        /// Draft 3. Schema URI <c>http://json-schema.org/draft-03/schema#</c>.
        /// </summary>
        Draft3,
        /// <summary>
        /// Draft 4. Schema URI <c>http://json-schema.org/draft-04/schema#</c>.
        /// </summary>
        Draft4,
        /// <summary>
        /// Draft 6. Schema URI <c>http://json-schema.org/draft-06/schema#</c>.
        /// </summary>
        Draft6
    }
}