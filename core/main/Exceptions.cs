
/*
    Copyright © 2023 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

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

namespace AutoCheck.Core.Exceptions
{
    [Serializable]
    /// <summary>
    /// The exception that is thrown when a config file is missing.
    /// </summary>
    public class ConfigFileMissingException : Exception
    {
        public ConfigFileMissingException(){}
        public ConfigFileMissingException(string message, Exception innerException = null) : base(message, innerException){}
    }

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
    /// The exception that is thrown when a document cannot be correctly loaded and parsed into an equivalent object (CSS, HTML, etc.).
    /// </summary>
    public class StyleInvalidException : Exception
    {
        public StyleInvalidException(){}
        public StyleInvalidException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a document cannot be correctly loaded and parsed into an equivalent object (CSS, HTML, etc.).
    /// </summary>
    public class RegexInvalidException : Exception
    {
        public RegexInvalidException(){}
        public RegexInvalidException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when an item cannot be found within a collection.
    /// </summary>
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(){}
        public ItemNotFoundException(string message, Exception innerException = null) : base(message, innerException){}
    }    

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a document cannot be correctly loaded and parsed into an equivalent object (CSS, HTML, etc.).
    /// </summary>
    public class VariableNotFoundException : ItemNotFoundException
    {
        public VariableNotFoundException(){}
        public VariableNotFoundException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a document cannot be correctly loaded and parsed into an equivalent object (CSS, HTML, etc.).
    /// </summary>
    public class ArgumentNotFoundException : ItemNotFoundException
    {
        public ArgumentNotFoundException(){}
        public ArgumentNotFoundException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a document cannot be correctly loaded and parsed into an equivalent object (CSS, HTML, etc.).
    /// </summary>
    public class PorpertyNotFoundException : ItemNotFoundException
    {
        public PorpertyNotFoundException(){}
        public PorpertyNotFoundException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a connector cannot be correctly loaded.
    /// </summary>
    public class ConnectorNotFoundException : ItemNotFoundException
    {
        public ConnectorNotFoundException(){}
        public ConnectorNotFoundException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a yaml script file cannot be correctly loaded.
    /// </summary>
    public class ScriptNotFoundException : ItemNotFoundException
    {
        public ScriptNotFoundException(){}
        public ScriptNotFoundException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a script cannot be correctly loaded.
    /// </summary>
    public class ScriptInvalidException : Exception
    {
        public ScriptInvalidException(){}
        public ScriptInvalidException(string message, Exception innerException = null) : base(message, innerException){}
    }

    

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a connector cannot be correctly loaded.
    /// </summary>
    public class ConnectorInvalidException : Exception
    {
        public ConnectorInvalidException(){}
        public ConnectorInvalidException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when a document cannot be correctly loaded and parsed into an equivalent object (CSS, HTML, etc.).
    /// </summary>
    public class VariableInvalidException : Exception
    {
        public VariableInvalidException(){}
        public VariableInvalidException(string message, Exception innerException = null) : base(message, innerException){}
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
    /// The exception that is thrown when an HTML table is not consistent (the amount of columns mismatches between rows).
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

    [Serializable]
    /// <summary>
    /// The exception that is thrown when an download failed is not valid.
    /// </summary>
    public class DownloadFailedException : Exception
    {
        public DownloadFailedException(){}
        public DownloadFailedException(string message, Exception innerException = null) : base(message, innerException){}
    }

    [Serializable]
    /// <summary>
    /// The exception that is thrown when an download failed is not valid.
    /// </summary>
    public class ResultMismatchException : Exception
    {
        public ResultMismatchException(){}
        public ResultMismatchException(string message, Exception innerException = null) : base(message, innerException){}
    }
   
}
