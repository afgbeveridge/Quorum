#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Collections.Generic;

namespace Quorum.Integration {

    /// <summary>
    /// Instantiated and used by inbound event listeners to validate requests
    /// </summary>
    public interface IRequestValidator {
        /// <summary>
        /// Examine the supplied directives, with attendant outer context, and throw an exception if unacceptable
        /// </summary>
        /// <param name="directives">a collection of directives</param>
        /// <param name="outerContext">weakly typed outer context</param>
        void Validate(IDictionary<string, string> directives, object outerContext);

    }
}
