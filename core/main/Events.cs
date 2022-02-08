using System;
using AutoCheck.Core;

namespace AutoCheck.Core.Events
{
    public class LogGeneratedEventArgs : EventArgs
    {   
        public Guid ID { get; set; }            
        public Output.Log Log { get; set; }

        /// <summary>
        /// Creates a new log event instance.
        /// </summary>
        /// <param name="id">The current instance ID (for multi-threading purposes).</param>
        /// <param name="log">The log content.</param>
        public LogGeneratedEventArgs(Guid id, Output.Log log){
            ID = id;
            Log = log;
        }
    }

    public class ScriptExecutionEventArgs : EventArgs
    {
         public enum ExecutionModeType{
            SINGLE,
            BATCH
        }

        public enum ExecutionEventType{
            HEADER,
            INIT,            
            SETUP,
            COPY_DETECTOR,
            BEFORE_ANY_PRE,
            PRE,
            BODY,
            POST,      
            AFTER_ALL_POST,   
            TEARDOWN,
            END
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
        public ScriptExecutionEventArgs(Guid id, ExecutionModeType executionMode, ExecutionEventType eventType){
            ID = id;
            Mode = executionMode;
            Event = eventType;
        }
    }
}