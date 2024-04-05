using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace DIgnoranceIsBliss
{
    // Proxied Logger class
    internal static class D
    {
        public static void Text(string line, Exception ex = null)
        {
            Ferris.Text(line, ex);
        }

        public static void Warning(string line, Exception ex = null)
        {
            Ferris.Warning(line, ex);
        }


        public static void Debug(string title)
        {
            if (title.NullOrEmpty()) return;
            Ferris.Debug(title);
        }


        public static void Error(string title, Exception ex = null)
        {
            Ferris.Error(title, ex);
        }


        public static void List<T>(string title, IEnumerable<T> list, bool internalList = false)
        {
            Ferris.List(title, list, internalList);
        }
    }
    // Our Core class
    internal static class Ferris
    {
        private static readonly string PREFIX = "[" + Assembly.GetExecutingAssembly().GetName().Name + "] ";
        private static void LOG(string line, string extraPrefix = "", Exception e = null, int level = 0)
        {
            if (line.NullOrEmpty() && e == null) return;

            string output = PREFIX;
            if (!extraPrefix.NullOrEmpty())
                output += extraPrefix + " ";

            if (!line.NullOrEmpty())
                output += line;

            if (e != null)
                output += (output.EndsWith(" ") || output.EndsWith("\n") ? "" : "\n") + e;


            if (level > 1)
                Log.Error(output);
            else if (level == 1)
                Log.Warning(output);
            else
                Log.Message(output);
        }
        public static void Text(string line, Exception ex = null)
        {
            LOG(line, "", ex);
        }
        public static void Warning(string line, Exception ex = null)
        {
            LOG(line, "WARNING", ex, 1);
        }
        public static void Debug(string title)
        {
            if (title.NullOrEmpty()) return;
            LOG(title, "DEBUG");
        }
        public static void Error(string title, Exception ex = null)
        {
            LOG(title, "ERROR", ex, 2);
        }
        public static void List<T>(string title, IEnumerable<T> list, bool internalList = false)
        {
            if (internalList)
            {
                var output = PREFIX + title;
                output += "\n" + "===== LIST =====";
                foreach (var t in list)
                {
                    var str = "     ";
                    var t2 = t;
                    var v = str + (t2 != null ? t2.ToString() : null);
                    if (!v.NullOrEmpty())
                        output += "\n" + v;
                }

                LOG(output);
            }
            else
            {
                LOG(PREFIX + "===== LIST - " + title + " =====");
                try
                {
                    foreach (var t in list)
                    {
                        var str = "     ";
                        var t2 = t;
                        LOG(str + (t2 != null ? t2.ToString() : null));
                    }
                }
                catch (Exception ex)
                {
                    LOG("Error printing list: " + ex.Message);
                }
            }
        }


        // Our Harmony helper class
        internal static class PatchHelper
        {
            private static readonly List<PatchTarget.PatchCallbackResult> PatcherErrorMessages = new List<PatchTarget.PatchCallbackResult>();
            public class PatchTarget
            {

                public class PatchCallbackResult
                {
                    public readonly bool WasSuccess;
                    public readonly string Message;
                    public readonly Exception Exception;
                    public readonly bool WasSevereError;

                    public void Print()
                    {
                        if (Message.NullOrEmpty() && Exception == null) return;
                        var output = (Message.NullOrEmpty() ? "" : Message);
                        if (Exception != null) output += (output.NullOrEmpty() ? "" : "\n") + Exception;
                        if (output.NullOrEmpty()) return;
                        if (WasSevereError)
                            Error(output);
                        else
                            Text(output);
                    }


                    private PatchCallbackResult(bool wasSuccess, string message = "", Exception exception = null, bool WasSevereError = false)
                    {
                        this.WasSuccess = wasSuccess;
                        this.Message = message;
                        this.Exception = exception;
                        this.WasSevereError = WasSevereError;
                    }

                    public class Success : PatchCallbackResult
                    {
                        public Success(string message = "", Exception exception = null) : base(true, message, exception) {}
                    }
                    public class Failure : PatchCallbackResult
                    {
                        public Failure(string message = "", Exception exception = null, bool wasSevere = false) : base(false, message, exception, wasSevere) { }
                    }


                }
                
                public class PatchArguments
                {
                    // Target stuff
                    private readonly Type _targetClass;
                    private readonly string _targetMethodName;

                    private readonly Type[] _parametersTarget;

                    // Patch stuff
                    private readonly PatchTarget.PatchType _patchType;
                    private readonly Type _patchClass;
                    private readonly string _patchName;
                    private readonly Type[] _patchParameters;
                    private readonly Action<PatchCallbackResult> _callback;

                    // debug stuff
                    private string _failurePoint = "";
                    private Exception _failureException = null;
                    private bool _failurePointSevere = false;

                    public PatchArguments(Type targetClass, string targetMethodName, Type[] parametersTarget = null, PatchTarget.PatchType patchType = PatchTarget.PatchType.Invalid, Type patchClass = null, string patchName = "", Type[] patchParameters = null, Action<PatchCallbackResult> callback = null)
                    {
                        this._targetClass = targetClass;
                        this._targetMethodName = targetMethodName;
                        this._parametersTarget = parametersTarget;

                        this._patchType = patchType;
                        this._patchClass = patchClass;
                        this._patchName = patchName;
                        this._patchParameters = patchParameters;

                        this._callback = callback;
                    }

                    public string GetFailurePoint()
                    {
                        return _failurePoint;
                    }


                    private static string BuildParameterList(Type[] types)
                    {
                        if (types == null) return "ANY";
                        if (!types.Any()) return "NO PARAMETER METHOD";
                        string output = "";
                        foreach (var v in types)
                            output += (output.NullOrEmpty() ? "" : ", ") + v.Name;
                        return output;
                    }

                    private static string GetNamedType(PatchType type)
                    {
                        switch (type)
                        {
                            case PatchType.Finalizer: return "Finalizer";
                            case PatchType.Postfix: return "Postfix";
                            case PatchType.Prefix: return "Prefix";
                            case PatchType.Transpiler: return "Transpiler";
                            case PatchType.Invalid: return "Invalid";
                            default: return "InvalidNumeric(" + (int)type + ")";
                        }
                    }

                    public string BuildTargetMethodName(string fields = "")
                    {
                        return (_targetClass == null ? "Null" : _targetClass.Namespace + "." + _targetClass.Name) + "." + (_targetMethodName.NullOrEmpty() ? "Null(" + fields + ")" : _targetMethodName + "(" + fields + ")");
                    }

                    public string BuildPatchMethodName(string fields = "")
                    {
                        return (_patchClass == null ? "Null" : _patchClass.Namespace + "." + _patchClass.Name) + "." + (_patchName.NullOrEmpty() ? "Null(" + fields + ")" : _patchName + "(" + fields + ")");
                    }

                    public bool IsSevereError()
                    {
                        return _failurePointSevere; }

                    private string BuildPatchDetails(bool includeErrorDetails = false)
                    {
                        string output = "======== Details ========";
                        output += "\n" + "Target Class: " + (_targetClass == null ? "Null" : _targetClass.FullDescription());
                        output += "\n" + "Target Method: " + (_targetMethodName.NullOrEmpty() ? "Null" : _targetMethodName);
                        output += "\n" + "Target Parameters: " + BuildParameterList(_parametersTarget);
                        output += "\n";
                        output += "\n" + "Patch Class: " + (_patchClass == null ? "Null" : _patchClass.FullDescription());
                        output += "\n" + "Patch Method: " + (_patchName.NullOrEmpty() ? "Null" : _patchName);
                        output += "\n" + "Patch Parameters: " + BuildParameterList(_patchParameters);
                        output += "\n" + "Patch Type: " + GetNamedType(_patchType);

                        if (includeErrorDetails && (_failureException != null || !_failurePoint.NullOrEmpty()))
                        {
                            output += "\n";
                            output += "\n" + "======== Failure Details ========";
                            output += "\n" + "Failure Point: " + (_failurePoint.NullOrEmpty() ? "Caught Exception While Processing" : _failurePoint);
                            output += "\n" + "Exception: " + (_failureException != null ? "\n" + _failureException : "No Exception Logged");
                        }

                        return output;
                    }

                    private PatchTarget BuildFailure(string message = null, Exception ex = null)
                    {
                        _failureException = ex;
                        _failurePoint = BuildPatchDetails();
                        return new PatchTarget(this);
                    }

                    private string BuildSevereString(string original)
                    {
                        _failurePointSevere = true;
                        return "[SEVERE] " + original;
                    }


                    public PatchTarget BuildPatch()
                    {
                        if (_targetClass != null) Text("Attempting to register patch for " + _targetClass.Name + "." + _targetMethodName + "!");


                        string errors = "";
                        if (_targetClass == null)
                            errors += (errors.NullOrEmpty() ? "" : "\n") + "Target Class was null or empty, assuming this mod was not loaded!";
                        if (_targetMethodName.NullOrEmpty())
                            errors += (errors.NullOrEmpty() ? "" : "\n") + "Target Method was null or empty, assuming this mod was not loaded!!";
                        if (_patchType < PatchType.Prefix || _patchType >= PatchType.Invalid)
                            errors += (errors.NullOrEmpty() ? "" : "\n") + BuildSevereString("Patch type " + _patchType + " was not valid!");
                        if (_patchClass == null)
                            errors += (errors.NullOrEmpty() ? "" : "\n") + BuildSevereString("Patch Class was not valid!");
                        if (_patchName.NullOrEmpty())
                            errors += (errors.NullOrEmpty() ? "" : "\n") + BuildSevereString("Patch MethodName was null or empty!");

                        if (!errors.NullOrEmpty()) return BuildFailure(errors);
                        try
                        {
                            var targetInfo = AccessTools.Method(_targetClass, _targetMethodName, _parametersTarget);
                            if (targetInfo == null)
                                return BuildFailure("Target method could not be found!");
                            var patchInfo = AccessTools.Method(_patchClass, _patchName, _patchParameters);
                            if (patchInfo == null)
                                return BuildFailure("Patch method could not be found!");
                            return new PatchTarget(this, targetInfo, patchInfo, _patchType, _callback);
                        }
                        catch (Exception e)
                        {
                            return BuildFailure("Got exception when using harmony patcher!", e);
                        }
                    }
                }

                public enum PatchType
                {
                    Prefix,
                    Postfix,
                    Transpiler,
                    Finalizer,
                    Invalid
                }


                private static void DEFAULT_CALLBACK(PatchCallbackResult callbackResult)
                {
                    if (callbackResult == null || (callbackResult.Message.NullOrEmpty() && callbackResult.Exception == null)) return;
                    if (callbackResult.WasSuccess)
                    {
                        Text(callbackResult.Message, callbackResult.Exception);
                        return;
                    }
                    PatcherErrorMessages.Add(callbackResult);
                }

                private readonly MethodInfo _targetMethod;
                private readonly MethodInfo _patchMethod;
                private readonly PatchType _patchType;
                private readonly Action<PatchCallbackResult> _callback;
                private readonly PatchArguments _originalArgs;
                private readonly bool _isBrokenPatch;

                public PatchTarget(PatchArguments arguments, MethodInfo target, MethodInfo patch, PatchType type, Action<PatchCallbackResult> callback = null)
                {
                    this._targetMethod = target;
                    this._patchMethod = patch;
                    this._patchType = type;
                    this._callback = callback == null ? DEFAULT_CALLBACK : callback;
                    this._originalArgs = arguments;
                    this._isBrokenPatch = false;
                }

                public PatchTarget(PatchArguments arguments)
                {
                    this._originalArgs = arguments;
                    this._targetMethod = null;
                    this._patchMethod = null;
                    this._patchType = PatchType.Invalid;
                    this._callback = DEFAULT_CALLBACK;
                    this._isBrokenPatch = true;
                }

                private bool OnFailure(string reason = null, Exception exception = null, bool wasSevereError = false)
                {
                    if (_callback == null) return false;
                    try
                    {
                        _callback.Invoke(new PatchCallbackResult.Failure("Failed to patch " + BuildTargetMethodName(true, true) + " with " + BuildPatchMethodName(false) + "\n" + (reason.NullOrEmpty() ? (exception == null ? "No Reason Provided" : "Caught Exception During Patch") : reason), exception, wasSevereError));
                    }
                    catch
                    {
                    }

                    return false;
                }

                private string BuildTargetMethodName(bool includeFields = true, bool onlyShowFieldTypes = false)
                {
                    if (_originalArgs == null) return "NULL";
                    if (!includeFields) return _originalArgs.BuildTargetMethodName();
                    string output = "";
                    if (_targetMethod != null)
                    {
                        foreach (var v in _targetMethod.GetParameters())
                            output += (output.NullOrEmpty() ? "" : ", ") + v.ParameterType.Name + (onlyShowFieldTypes ? "" : " " + v.Name);
                    }

                    return _originalArgs.BuildTargetMethodName(output);
                }

                private string BuildPatchMethodName(bool includeFields = true, bool onlyShowFieldTypes = false)
                {
                    if (_originalArgs == null) return "NULL";
                    if (!includeFields) return _originalArgs.BuildPatchMethodName();
                    string output = "";
                    if (_patchMethod != null)
                    {
                        foreach (var v in _patchMethod.GetParameters())
                            output += (output.NullOrEmpty() ? "" : ", ") + v.ParameterType.Name + (onlyShowFieldTypes ? "" : " " + v.Name);
                    }

                    return _originalArgs.BuildPatchMethodName(output);
                }


                private bool OnSuccess(string message = null)
                {
                    if (_callback == null) return true;
                    try
                    {
                        _callback.Invoke(new PatchCallbackResult.Success("Successfully patched " + BuildTargetMethodName(true, true) + " with " + BuildPatchMethodName(false) + (message.NullOrEmpty() ? "" : "\n" + message)));
                    }
                    catch
                    {
                    }

                    return true;
                }

                public bool Patch(Harmony harmony)
                {
                    try
                    {
                        if (_isBrokenPatch) return OnFailure(_originalArgs.GetFailurePoint(), null, _originalArgs.IsSevereError());
                        if (harmony == null) return OnFailure("Harmny was null!");
                        if (_targetMethod == null) return OnFailure("Could not find class or method to patch!");
                        if (_patchMethod == null) return OnFailure("Patch method was null???? This should never happen!");

                        try
                        {
                            InternalPatch(harmony);
                            return OnSuccess();
                        }
                        catch (Exception e)
                        {
                            return OnFailure(null, e);
                        }
                    }
                    catch (Exception e)
                    {
                        return OnFailure("Failed to Process Patch, Got Exception During Patch(Harmony) Phase! This should never happen!", e);
                    }
                }

                private MethodInfo InternalPatch(Harmony harmony)
                {
                    switch (_patchType)
                    {
                        case PatchType.Prefix: return harmony.Patch(_targetMethod, new HarmonyMethod(_patchMethod));
                        case PatchType.Postfix: return harmony.Patch(_targetMethod, null, new HarmonyMethod(_patchMethod));
                        case PatchType.Transpiler: return harmony.Patch(_targetMethod, null, null, new HarmonyMethod(_patchMethod));
                        case PatchType.Finalizer: return harmony.Patch(_targetMethod, null, null, null, new HarmonyMethod(_patchMethod));
                        default: throw new InvalidOperationException("Patch Type was not valid or correctly specified");
                    }
                }
            }

            private static readonly List<PatchTarget> _patchTargets = new List<PatchTarget>();

            public static void RegisterPatch(Type targetClass, string targetMethodName, Type[] parametersTarget = null, PatchTarget.PatchType patchType = PatchTarget.PatchType.Invalid, Type patchClass = null, string patchName = "", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(targetClass, targetMethodName, parametersTarget, patchType, patchClass, patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterPatch(string targetClass, string targetMethodName, Type[] parametersTarget = null, PatchTarget.PatchType patchType = PatchTarget.PatchType.Invalid, Type patchClass = null, string patchName = "", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(AccessTools.TypeByName(targetClass), targetMethodName, parametersTarget, patchType, patchClass, patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterPatch(Type targetClass, string targetMethodName, Type[] parametersTarget = null, PatchTarget.PatchType patchType = PatchTarget.PatchType.Invalid, string patchClass = null, string patchName = "", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(targetClass, targetMethodName, parametersTarget, patchType, AccessTools.TypeByName(patchClass), patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterPatch(string targetClass, string targetMethodName, Type[] parametersTarget = null, PatchTarget.PatchType patchType = PatchTarget.PatchType.Invalid, string patchClass = null, string patchName = "", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(AccessTools.TypeByName(targetClass), targetMethodName, parametersTarget, patchType, AccessTools.TypeByName(patchClass), patchName, patchParameters, callback).BuildPatch());
            }


            public static void RegisterPrefixPatch(Type targetClass, string targetMethodName, Type[] parametersTarget = null, Type patchClass = null, string patchName = "Prefix", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(targetClass, targetMethodName, parametersTarget, PatchTarget.PatchType.Prefix, patchClass, patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterPrefixPatch(string targetClass, string targetMethodName, Type[] parametersTarget = null, Type patchClass = null, string patchName = "Prefix", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(AccessTools.TypeByName(targetClass), targetMethodName, parametersTarget, PatchTarget.PatchType.Prefix, patchClass, patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterPrefixPatch(Type targetClass, string targetMethodName, Type[] parametersTarget = null, string patchClass = null, string patchName = "Prefix", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(targetClass, targetMethodName, parametersTarget, PatchTarget.PatchType.Prefix, AccessTools.TypeByName(patchClass), patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterPrefixPatch(string targetClass, string targetMethodName, Type[] parametersTarget = null, string patchClass = null, string patchName = "Prefix", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(AccessTools.TypeByName(targetClass), targetMethodName, parametersTarget, PatchTarget.PatchType.Prefix, AccessTools.TypeByName(patchClass), patchName, patchParameters, callback).BuildPatch());
            }


            public static void RegisterPostfixPatch(Type targetClass, string targetMethodName, Type[] parametersTarget = null, Type patchClass = null, string patchName = "Postfix", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(targetClass, targetMethodName, parametersTarget, PatchTarget.PatchType.Postfix, patchClass, patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterPostfixPatch(string targetClass, string targetMethodName, Type[] parametersTarget = null, Type patchClass = null, string patchName = "Postfix", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(AccessTools.TypeByName(targetClass), targetMethodName, parametersTarget, PatchTarget.PatchType.Postfix, patchClass, patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterPostfixPatch(Type targetClass, string targetMethodName, Type[] parametersTarget = null, string patchClass = null, string patchName = "Postfix", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(targetClass, targetMethodName, parametersTarget, PatchTarget.PatchType.Postfix, AccessTools.TypeByName(patchClass), patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterPostfixPatch(string targetClass, string targetMethodName, Type[] parametersTarget = null, string patchClass = null, string patchName = "Postfix", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(AccessTools.TypeByName(targetClass), targetMethodName, parametersTarget, PatchTarget.PatchType.Postfix, AccessTools.TypeByName(patchClass), patchName, patchParameters, callback).BuildPatch());
            }


            public static void RegisterTranspilerPatch(Type targetClass, string targetMethodName, Type[] parametersTarget = null, Type patchClass = null, string patchName = "Transpiler", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(targetClass, targetMethodName, parametersTarget, PatchTarget.PatchType.Transpiler, patchClass, patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterTranspilerPatch(string targetClass, string targetMethodName, Type[] parametersTarget = null, Type patchClass = null, string patchName = "Transpiler", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(AccessTools.TypeByName(targetClass), targetMethodName, parametersTarget, PatchTarget.PatchType.Transpiler, patchClass, patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterTranspilerPatch(Type targetClass, string targetMethodName, Type[] parametersTarget = null, string patchClass = null, string patchName = "Transpiler", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(targetClass, targetMethodName, parametersTarget, PatchTarget.PatchType.Transpiler, AccessTools.TypeByName(patchClass), patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterTranspilerPatch(string targetClass, string targetMethodName, Type[] parametersTarget = null, string patchClass = null, string patchName = "Transpiler", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(AccessTools.TypeByName(targetClass), targetMethodName, parametersTarget, PatchTarget.PatchType.Transpiler, AccessTools.TypeByName(patchClass), patchName, patchParameters, callback).BuildPatch());
            }


            public static void RegisterFinalizerPatch(Type targetClass, string targetMethodName, Type[] parametersTarget = null, Type patchClass = null, string patchName = "Finalizer", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(targetClass, targetMethodName, parametersTarget, PatchTarget.PatchType.Finalizer, patchClass, patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterFinalizerPatch(string targetClass, string targetMethodName, Type[] parametersTarget = null, Type patchClass = null, string patchName = "Finalizer", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(AccessTools.TypeByName(targetClass), targetMethodName, parametersTarget, PatchTarget.PatchType.Finalizer, patchClass, patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterFinalizerPatch(Type targetClass, string targetMethodName, Type[] parametersTarget = null, string patchClass = null, string patchName = "Finalizer", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(targetClass, targetMethodName, parametersTarget, PatchTarget.PatchType.Finalizer, AccessTools.TypeByName(patchClass), patchName, patchParameters, callback).BuildPatch());
            }

            public static void RegisterFinalizerPatch(string targetClass, string targetMethodName, Type[] parametersTarget = null, string patchClass = null, string patchName = "Finalizer", Type[] patchParameters = null, Action<PatchTarget.PatchCallbackResult> callback = null)
            {
                _patchTargets.Add(new PatchTarget.PatchArguments(AccessTools.TypeByName(targetClass), targetMethodName, parametersTarget, PatchTarget.PatchType.Finalizer, AccessTools.TypeByName(patchClass), patchName, patchParameters, callback).BuildPatch());
            }

            

            public static void ProcessRegisteredPatches(Harmony harmony)
            {
                Text("Attempting to Process " + _patchTargets.Count + " Patches!");
                int successful = 0;
                // Run our patches
                foreach (var v in _patchTargets)
                    if (v.Patch(harmony))
                        successful += 1;

                // Process our Error messages last
                foreach (var v in PatcherErrorMessages)
                    if(v != null)
                        v.Print();
                PatcherErrorMessages.Clear();

                // Log our success rate
                Text("Completed " + _patchTargets.Count + " Patches with " + successful + "/" + _patchTargets.Count + " successful patches");
                _patchTargets.Clear();
            }
        }
    }
}
