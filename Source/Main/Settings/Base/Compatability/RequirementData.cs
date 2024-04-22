using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace DFerrisIgnorance.Compatability;

public class RequirementData
{
    public class RequiredTypeData
    {
        public readonly string TypeName;
        private Type _cachedType;
        private bool _hasChecked = false;

        public RequiredTypeData(string typeName)
        {
            TypeName = typeName;
        }

        public virtual bool ContainsData(bool reCheck = false)
        {
            return GetType(out var _, reCheck);
        }

        public bool GetType(out Type type, bool reCheck = false)
        {
            if (_hasChecked && !reCheck)
            {
                type = _cachedType;
                return _cachedType != null;
            }

            _hasChecked = true;
            _cachedType = AccessTools.TypeByName(TypeName);
            type = _cachedType;
            return _cachedType != null;
        }
    }

    public class RequiredMethodData : RequiredTypeData
    {
        public readonly string MethodName;
        private MethodInfo _cachedMethod;
        private bool _hasChecked = false;

        public RequiredMethodData(string typeName, string methodName) : base(typeName)
        {
            MethodName = methodName;
        }

        public override bool ContainsData(bool reCheck = false)
        {
            return GetMethod(out var _, reCheck);
        }

        public bool GetMethod(out MethodInfo method, bool reCheck = false)
        {
            if (_hasChecked && !reCheck)
            {
                method = _cachedMethod;
                return _cachedMethod != null;
            }

            _hasChecked = true;
            if (base.GetType(out var result, reCheck))
            {
                _cachedMethod = AccessTools.Method(result, MethodName);
                method = _cachedMethod;
                return method != null;
            }

            method = null;
            return false;
        }
    }

    public class RequiredFieldData : RequiredTypeData
    {
        public readonly string FieldName;
        private FieldInfo _cachedField;
        private bool _hasChecked = false;

        public RequiredFieldData(string typeName, string fieldName) : base(typeName)
        {
            FieldName = fieldName;
        }

        public override bool ContainsData(bool reCheck = false)
        {
            return GetField(out var _, reCheck);
        }


        public bool GetField(out FieldInfo method, bool reCheck = false)
        {
            if (_hasChecked && !reCheck)
            {
                method = _cachedField;
                return _cachedField != null;
            }

            _hasChecked = true;
            if (base.GetType(out var result, reCheck))
            {
                _cachedField = AccessTools.Field(result, FieldName);
                method = _cachedField;
                return method != null;
            }

            method = null;
            return false;
        }
    }

    private readonly List<string> _modNamesForIcon = new List<string>();
    private readonly List<string> _requireModNames = new List<string>();
    private readonly List<string> _incompatableModNames = new List<string>();

    private readonly List<RequiredTypeData> _requiredData = new List<RequiredTypeData>();
    private readonly List<RequiredTypeData> _incompatableData = new List<RequiredTypeData>();

    private bool _hasChecked = false;
    private bool _isValid = false;

    private bool _requireAllModNames = false;
    private bool _requireAllClassData = false;

    private string _iconTextureLocation = null;
    private Texture2D _icon = null;
    private bool _hasTriedToLoadIcon = false;


    public string IconTextureLocation => LoadOrGetIconTextureLocation();

    public Texture2D Icon => LoadOrGetIconTexture();


    public RequirementData AddModnameForIcon(string modName)
    {
        _modNamesForIcon.Add(modName);
        return this;
    }


    public RequirementData RequireAllModNames(bool requireAllModNames = true)
    {
        _requireAllModNames = requireAllModNames;
        return this;
    }

    public RequirementData RequireAllClassData(bool requireAllClassData = true)
    {
        _requireAllClassData = requireAllClassData;
        return this;
    }

    public RequirementData AddRequiredModName(string name)
    {
        _requireModNames.Add(name);
        return this;
    }

    public RequirementData AddRequiredType(string type)
    {
        _requiredData.Add(new RequiredTypeData(type));
        return this;
    }

    public RequirementData AddRequiredMethod(string type, string method)
    {
        _requiredData.Add(new RequiredMethodData(type, method));
        return this;
    }

    public RequirementData AddRequiredField(string type, string field)
    {
        _requiredData.Add(new RequiredFieldData(type, field));
        return this;
    }

    public RequirementData AddIncompatableModName(string name)
    {
        _incompatableModNames.Add(name);
        return this;
    }

    public RequirementData AddIncompatableType(string type)
    {
        _incompatableData.Add(new RequiredTypeData(type));
        return this;
    }

    public RequirementData AddIncompatableMethod(string type, string method)
    {
        _incompatableData.Add(new RequiredMethodData(type, method));
        return this;
    }

    public RequirementData AddIncompatableField(string type, string field)
    {
        _incompatableData.Add(new RequiredFieldData(type, field));
        return this;
    }


    private bool IsInvalid(bool reCheck)
    {
        if (!_requireModNames.NullOrEmpty() && (_requireAllModNames ? (!_requireModNames.All(x => LoadedModManager.RunningModsListForReading.Any(y => y.Name.EqualsIgnoreCase(x) || y.PackageId.EqualsIgnoreCase(x)))) : (!_requireModNames.Any(x => LoadedModManager.RunningModsListForReading.Any(y => x.EqualsIgnoreCase(y.Name) || x.EqualsIgnoreCase(y.PackageId)))))) return true;
        if (!_incompatableModNames.NullOrEmpty() && (_incompatableModNames.Any(x => LoadedModManager.RunningModsListForReading.Any(y => x.EqualsIgnoreCase(y.Name) || x.EqualsIgnoreCase(y.PackageId))))) return true;
        if (!_requiredData.NullOrEmpty() && (_requireAllClassData ? _requiredData.Any(x => !x.ContainsData(reCheck)) : !_requiredData.Any(x => x.ContainsData()))) return true;
        if (!_incompatableData.NullOrEmpty() && (_incompatableData.Any(x => x.ContainsData(reCheck)))) return true;
        return false;
    }


    private bool AttemptLoadIcon(string modName)
    {
        var v = LoadedModManager.RunningModsListForReading.FirstOrDefault(y => y.Name.EqualsIgnoreCase(modName) || y.PackageId.EqualsIgnoreCase(modName));
        if (v == null) return false;
        var location = v.ModMetaData.ModIconImagePath;
        if (!location.NullOrEmpty())
        {
            _iconTextureLocation = location;
            if (!File.Exists(v.ModMetaData.ModIconImagePath)) return false;
            _icon = ContentFinder<Texture2D>.Get(IconTextureLocation, false);
            if (Icon == null)
            {
                _icon = new Texture2D(0, 0);
                if (!Icon.LoadImage(File.ReadAllBytes(IconTextureLocation)))
                    _icon = null;
            }
        }

        return _icon != null;
    }




    private Texture2D LoadOrGetIconTexture()
    {
        if (!UnityData.IsInMainThread) return null;
        if (!_hasTriedToLoadIcon) AttemptLoadIcon();
        return _icon;
    }

    private string LoadOrGetIconTextureLocation()
    {
        if (!UnityData.IsInMainThread) return null;
        if (!_hasTriedToLoadIcon) AttemptLoadIcon();
        return _iconTextureLocation;
    }
    

    private void AttemptLoadIcon()
    {
        _hasTriedToLoadIcon = true;
        if (_modNamesForIcon.Any())
        {
            foreach (var modName in _modNamesForIcon)
                if (AttemptLoadIcon(modName))
                    return;
        }

        if (_requireModNames.Any())
        {
            foreach (var modName in _requireModNames)
                if (AttemptLoadIcon(modName))
                    return;
        }
    }


    public bool EnsureValid(bool reCheck = false)
    {
        if (_hasChecked && !reCheck) return _isValid;
        _hasChecked = true;
        if (IsInvalid(reCheck))
        {
            _isValid = false;
            return false;
        }

        _isValid = true;
        return true;
    }
}
