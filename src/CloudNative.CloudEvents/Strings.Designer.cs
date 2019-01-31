﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CloudNative.CloudEvents {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CloudNative.CloudEvents.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input message is not a structured CloudEvents batch.
        /// </summary>
        internal static string ErrorBatchBinaryMode {
            get {
                return ResourceManager.GetString("ErrorBatchBinaryMode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to In binary content mode, the data field must either contain a stream, a byte arry, or a string value..
        /// </summary>
        internal static string ErrorBinaryModeDataEncoding {
            get {
                return ResourceManager.GetString("ErrorBinaryModeDataEncoding", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;contenttype&apos; attribute value must be a content-type expression compliant with RFC2046.
        /// </summary>
        internal static string ErrorContentTypeIsNotRFC2046 {
            get {
                return ResourceManager.GetString("ErrorContentTypeIsNotRFC2046", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;id&apos; attribute value must be a string.
        /// </summary>
        internal static string ErrorIdValueIsNotAString {
            get {
                return ResourceManager.GetString("ErrorIdValueIsNotAString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;sampledrate&apos; attribute value must be an integer.
        /// </summary>
        internal static string ErrorSampledRateValueIsaNotAnInteger {
            get {
                return ResourceManager.GetString("ErrorSampledRateValueIsaNotAnInteger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;schemaurl&apos; attribute value must be a valid absolute or relative URI.
        /// </summary>
        internal static string ErrorSchemaUrlIsNotAUri {
            get {
                return ResourceManager.GetString("ErrorSchemaUrlIsNotAUri", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;sequencetype&apos; attribute value must be an integer.
        /// </summary>
        internal static string ErrorSequenceTypeValueIsaNotAString {
            get {
                return ResourceManager.GetString("ErrorSequenceTypeValueIsaNotAString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;sequence&apos; attribute value must be an integer.
        /// </summary>
        internal static string ErrorSequenceValueIsaNotAString {
            get {
                return ResourceManager.GetString("ErrorSequenceValueIsaNotAString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;source&apos; attribute value must be a valid absolute or relative URI.
        /// </summary>
        internal static string ErrorSourceValueIsNotAUri {
            get {
                return ResourceManager.GetString("ErrorSourceValueIsNotAUri", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;specversion&apos; attribute value must be a string.
        /// </summary>
        internal static string ErrorSpecVersionValueIsNotAString {
            get {
                return ResourceManager.GetString("ErrorSpecVersionValueIsNotAString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;time&apos; attribute value must be a valid ISO8601 timestamp expression.
        /// </summary>
        internal static string ErrorTimeValueIsNotATimestamp {
            get {
                return ResourceManager.GetString("ErrorTimeValueIsNotATimestamp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;traceparent&apos; attribute value is required with the distributed tracing extension and must be a string.
        /// </summary>
        internal static string ErrorTraceParentValueIsaNotAString {
            get {
                return ResourceManager.GetString("ErrorTraceParentValueIsaNotAString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;tracestate&apos; attribute value must be a string.
        /// </summary>
        internal static string ErrorTraceStateValueIsaNotAString {
            get {
                return ResourceManager.GetString("ErrorTraceStateValueIsaNotAString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;type&apos; attribute value must be a string.
        /// </summary>
        internal static string ErrorTypeValueIsNotAString {
            get {
                return ResourceManager.GetString("ErrorTypeValueIsNotAString", resourceCulture);
            }
        }
    }
}
