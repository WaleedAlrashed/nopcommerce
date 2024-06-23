using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tamara.Domain
{
    /// <summary>
    /// Represents an exception details
    /// </summary>
    public class ExceptionDetails
    {
        #region Properties

        /// <summary>
        /// Gets or sets the message
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the details
        /// </summary>
        [JsonProperty(PropertyName = "errors")]
        public List<Error> Errors { get; set; }

        #endregion

        #region Nested classes

        public class Error
        {
            /// <summary>
            /// Gets or sets error code
            /// </summary>
            [JsonProperty(PropertyName = "error_code")]
            public string ErrorCode { get; set; }

            
        }
        #endregion
    }
}
