
/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;

namespace AutoCheck.Exceptions
{
    [Serializable]
    /// <summary>
    /// The exception that is thrown when a document cannot be correctly loaded and parsed into an equivalent object (CSS, HTML, etc.).
    /// </summary>
    public class DocumentInvalidException : Exception
    {
        public DocumentInvalidException(){}
        public DocumentInvalidException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a CSS style property has not been found within a CSS document.
    /// </summary>
    public class StyleNotFoundException : Exception
    {
        public StyleNotFoundException(){}
        public StyleNotFoundException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a CSS style property has been found within a CSS document but not applied over an HTML document.
    /// </summary>
    public class StyleNotAppliedException : Exception
    {
        public StyleNotAppliedException(){}
        public StyleNotAppliedException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when an HTML table is not consistent (the amount of columns missmatches between rows).
    /// </summary>
    public class TableInconsistencyException : Exception
    {
        public TableInconsistencyException(){}
        public TableInconsistencyException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a database connection cannot be established.
    /// </summary>
    public class ConnectionInvalidException : Exception
    {
        public ConnectionInvalidException(){}
        public ConnectionInvalidException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a database query cannot be executed.
    /// </summary>
    public class QueryInvalidException : Exception
    {
        public QueryInvalidException(){}
        public QueryInvalidException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when an argument is not valid.
    /// </summary>
    public class ArgumentInvalidException : Exception
    {
        public ArgumentInvalidException(){}
        public ArgumentInvalidException(string message, Exception innerException = null) : base(message, innerException){}
    }
}
