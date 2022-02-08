using System;
using AutoCheck.Core;

namespace AutoCheck.Core.Events
{
    public class LogGeneratedEventArgs : EventArgs
    {               
        public Output.Log Log { get; set; }

        /// <summary>
        /// Creates a new log event instance.
        /// </summary>
        /// <param name="log"></param>
        public LogGeneratedEventArgs(Output.Log log){
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
            PRE,
            BODY,
            POST,            
            TEARDOWN,
            END
        }
        
        public ExecutionModeType Mode { get; set; }
        public ExecutionEventType Event { get; set; }

        /// <summary>
        /// Creates a new execution event instance.
        /// </summary>
        /// <param name="executionMode">The current execution mode.</param>
        /// <param name="eventType">The event type.</param>
        public ScriptExecutionEventArgs(ExecutionModeType executionMode, ExecutionEventType eventType){
            Mode = executionMode;
            Event = eventType;
        }
    }
}