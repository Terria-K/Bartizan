﻿using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Xml.Linq;

namespace Patcher
{
  public class PatchAttribute : Attribute {}

  static class CecilExtensions
  {
    public static IEnumerable<TypeDefinition> AllNestedTypes(this TypeDefinition type)
    {
      yield return type;
      foreach (TypeDefinition nested in type.NestedTypes)
        foreach (TypeDefinition moarNested in AllNestedTypes(nested))
          yield return moarNested;
    }

    public static IEnumerable<TypeDefinition> AllNestedTypes(this ModuleDefinition module)
    {
      return module.Types.SelectMany(AllNestedTypes);
    }

    public static string Signature(this MethodReference method)
    {
      return string.Format("{0}({1})", method.Name, string.Join(", ", method.Parameters.Select(p => p.ParameterType)));
    }
  }

  public class Patcher
  {
    /// <summary>
    /// Marker method for calling the base implementation of a patched method
    /// </summary>
    public static void CallRealBase()
    {
    }

    /// <summary>
    /// Unseal, publicize, virtualize.
    /// </summary>
    static void MakeBaseImage(string source, string destination)
    {
      var module = ModuleDefinition.ReadModule(source);
      foreach (var type in module.AllNestedTypes()) {
        if (!type.FullName.StartsWith("TowerFall.") && !type.FullName.StartsWith("Monocle")) {
          continue;
        }
        if (type.Name.StartsWith("<>")) {
          continue;
        }
        if (type.IsNested)
          type.IsNestedPublic = true;
        if (type.IsValueType) {
          continue;
        }

        type.IsSealed = false;
        foreach (var field in type.Fields)
          field.IsPublic = true;
        foreach (var method in type.Methods) {
          method.IsPublic = true;
          if (!method.IsConstructor && !method.IsStatic)
            method.IsVirtual = true;
        }
      }
      module.Write(destination);
    }

    /// <summary>
    /// Inline classes marked as [Patch], copying fields and replacing method implementations.
    /// As you can probably guess from the code, this is wholly incomplete and will certainly break and have to be
    /// extended in the future.
    /// </summary>
    public static void Patch(string modModulePath, string targetDir, string output)
    {
      var baseModule = ModuleDefinition.ReadModule(Path.Combine(targetDir, "TowerFall.exe"));
      var modModule = ModuleDefinition.ReadModule(modModulePath);

      Func<TypeReference, bool> patchType = (type) => {
        if (type.Scope == modModule) {
          return type.Resolve().CustomAttributes.Any(attr => attr.AttributeType.FullName == "Patcher.PatchAttribute");
        }
        return false;
      };

      // baseModule won't recognize MemberReferences from modModule without Import(), so recursively translate them.
      // Furthermore, we have to redirect any references to members in [Patch] classes.
      Func<TypeReference, TypeReference> mapType = null;
      mapType = (modType) => {
        if (modType.IsGenericParameter) {
          return modType;
        }
        if (modType.IsArray) {
          var type = mapType(((ArrayType)modType).ElementType);
          return new ArrayType(type);
        }
        if (patchType(modType))
          modType = modType.Resolve().BaseType;
        return baseModule.Import(modType);
      };
      Action<MethodReference, MethodReference> mapParams = (modMethod, method) => {
        foreach (var param in modMethod.Parameters)
          method.Parameters.Add(new ParameterDefinition(mapType(param.ParameterType)));
      };
      Func<MethodReference, MethodReference> mapMethod = (modMethod) => {
        var method = new MethodReference(modMethod.Name, mapType(modMethod.ReturnType), mapType(modMethod.DeclaringType));
        method.HasThis = modMethod.HasThis;
        mapParams(modMethod, method);

        var modInst = modMethod as GenericInstanceMethod;
        if (modInst != null) {
          method.CallingConvention = MethodCallingConvention.Generic;
          var inst = new GenericInstanceMethod(method);
          foreach (var arg in modInst.GenericArguments) {
            inst.GenericArguments.Add(mapType(arg));
          }
          method = inst;
        }
        return method;
      };
      Func<MethodDefinition, string, MethodDefinition> cloneMethod = (modMethod, prefix) => {
        var method = new MethodDefinition(prefix + modMethod.Name, modMethod.Attributes, mapType(modMethod.ReturnType));
        mapParams(modMethod, method);
        foreach (var modParam in modMethod.GenericParameters) {
          var param = new GenericParameter(modParam.Position, GenericParameterType.Method, modModule);
          method.GenericParameters.Add(param);
        }
        return method;
      };

      foreach (TypeDefinition modType in modModule.Types.SelectMany(CecilExtensions.AllNestedTypes))
        if (patchType(modType)) {
          var type = baseModule.AllNestedTypes().Single(t => t.FullName == modType.BaseType.FullName);

          // copy over fields including their custom attributes
          foreach (var field in modType.Fields)
            if (field.DeclaringType == modType) {
              var newField = new FieldDefinition(field.Name, field.Attributes, mapType(field.FieldType));
              foreach (var attribute in field.CustomAttributes)
                newField.CustomAttributes.Add(new CustomAttribute(mapMethod(attribute.Constructor), attribute.GetBlob()));
              type.Fields.Add(newField);
            }

          // copy over or replace methods
          foreach (var method in modType.Methods)
            if (method.DeclaringType == modType) {
              var original = type.Methods.SingleOrDefault(m => m.Signature() == method.Signature());
              MethodDefinition savedMethod = null;
              if (original == null)
                type.Methods.Add(original = cloneMethod(method, ""));
              else {
                savedMethod = cloneMethod(method, "$original_");
                savedMethod.Body = original.Body;
                savedMethod.IsRuntimeSpecialName = false;
                type.Methods.Add(savedMethod);
              }
              original.Body = method.Body;

              // redirect any references in the body
              var proc = method.Body.GetILProcessor();
              var amendments = new List<Action>();
              foreach (var instr in method.Body.Instructions) {
                if (instr.Operand is MethodReference) {
                  var callee = (MethodReference)instr.Operand;
                  if (callee.Name == "CallRealBase") {
                    instr.OpCode = OpCodes.Call;
                    instr.Operand = type.BaseType.Resolve().Methods.Single(m => m.Name == method.Name);
                    amendments.Add(() => proc.InsertBefore(instr, proc.Create(OpCodes.Ldarg_0)));
                  } else {
                    callee = mapMethod((MethodReference)instr.Operand);
                    if (callee.FullName == original.FullName)
                      // replace base calls with ones to $original
                      instr.Operand = savedMethod;
                    else
                      instr.Operand = callee;
                  }
                }
                else if (instr.Operand is FieldReference) {
                  var field = (FieldReference)instr.Operand;
                  instr.Operand = new FieldReference(field.Name, mapType(field.FieldType), mapType(field.DeclaringType));
                } else if (instr.Operand is TypeReference)
                  instr.Operand = mapType((TypeReference)instr.Operand);
              }
              foreach (var var in method.Body.Variables)
                var.VariableType = mapType(var.VariableType);
              foreach (var amendment in amendments) {
                amendment();
              }
              method.Body = proc.Body;
            }
        }

      baseModule.Write(output);
    }

    /// <summary>
    /// Insert new sprites into Atlas.
    /// </summary>
    public static void PatchResources(string targetDir)
    {
      foreach (var atlasPath in Directory.EnumerateDirectories(Path.Combine("Content", "Atlas"))) {
        var xml = XElement.Load(Path.Combine(targetDir, atlasPath + ".xml"));

        string[] files = Directory.GetFiles(atlasPath, "*.png", SearchOption.AllDirectories);
        int x = 0;
        int y = 0;

        using (var baseImage = Bitmap.FromFile(Path.Combine(targetDir, atlasPath + ".png"))) {
          using (var g = Graphics.FromImage(baseImage))
            foreach (string file in files)
              using (var image = Bitmap.FromFile(file)) {
                string name = file.Substring(atlasPath.Length + 1).Replace(Path.DirectorySeparatorChar, '/');
                name = name.Substring(0, name.Length - ".png".Length);
                g.DrawImage(image, x, y);
                xml.Add(new XElement("SubTexture",
                  new XAttribute("name", name),
                  new XAttribute("x", x),
                  new XAttribute("y", y),
                  new XAttribute("width", image.Width),
                  new XAttribute("height", image.Height)
                ));
                y += image.Height;
              }
          baseImage.Save(atlasPath + ".png");
        }
        xml.Save(atlasPath + ".xml");
      }
    }

    public static int Main (string[] args)
    {
      if (args.Length == 0) {
        Console.WriteLine("Usage: Patcher.exe makeBaseImage <source> <destination>| patch <patch DLLs>*");
        return -1;
      }
      if (args[0] == "makeBaseImage") {
        if (args.Length < 3) {
          Console.WriteLine("Patcher.exe makeBaseImage <source> <destination>");
          return -1;
        }
        string source = args[1];
        string destination = args[2];
        MakeBaseImage(source, destination);
      } else if (args[0] == "patch-exe") {
        if (args.Length < 4) {
          Console.WriteLine("Patcher.exe patch-exe <patch DLL> <targetDir> <output>");
          return -1;
        }
        string modModulePath = args[1];
        string targetDir = args[2];
        string output = args[3];
        Patch(modModulePath, targetDir, output);
      } else if (args[0] == "patch-resources") {
        if (args.Length < 2) {
          Console.WriteLine("Patcher.exe patch-resources <targetDir>");
          return -1;
        }
        string targetDir = args[1];
        PatchResources(targetDir);
      }
      return 0;
    }
  }
}
