using System;
using AutoCheck.Core;

namespace AutoCheck.Core.Events
{       
    public class LogUpdateEventArgs : EventArgs
    {   
        public Guid ID { get; set; }            
        public Output.Log Log { get; set; }
        public bool EndOfScript { get; set; }

        /// <summary>
        /// Creates a new log event instance.
        /// </summary>
        /// <param name="id">The current instance ID (for multi-threading purposes).</param>
        /// <param name="log">The log content.</param>
        public LogUpdateEventArgs(Guid id, Output.Log log, bool endOfScript=false){
            ID = id;
            Log = log;
            EndOfScript = endOfScript;
        }
    }

    public class ScriptStatusEventArgs : EventArgs
    {
         public enum ExecutionModeType{
            SINGLE,
            BATCH
        }

        public enum ExecutionEventType{
            AFTER_HEADER,
            AFTER_INIT,            
            AFTER_SETUP,
            AFTER_COPY_DETECTOR,
            AFTER_PRE,
            AFTER_BODY,
            AFTER_POST,      
            AFTER_SCRIPT,      
            AFTER_TEARDOWN,
            AFTER_END
        }
        
        public Guid ID { get; set; }            
        public ExecutionModeType Mode { get; set; }
        public ExecutionEventType Event { get; set; }

        /// <summary>
        /// Creates a new execution event instance.
        /// </summary>
        /// <param name="id">The current instance ID (for multi-threading purposes).</param>
        /// <param name="executionMode">The current execution mode.</param>
        /// <param name="eventType">The event type.</param>
        public ScriptStatusEventArgs(Guid id, ExecutionModeType executionMode, ExecutionEventType eventType){
            ID = id;
            Mode = executionMode;
            Event = eventType;
        }
    }
}