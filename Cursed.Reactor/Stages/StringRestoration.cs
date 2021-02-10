using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;

namespace Cursed.Reactor.Stages
{
    internal class StringRestoration
    {
        public static void Execute(Context Ctx)
        {
            var DecMeth = GetDecryptionMethod(Ctx.DnModule);
            if (DecMeth == null) return;
            foreach (var TypeDef in Ctx.DnModule.Types.ToArray())
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody).ToArray())
                {
                    IList<Instruction> IL = MethodDef.Body.Instructions;
                    for (int x = 0; x < IL.Count; x++)
                    {
                        try
                        {
                            if (IL[x].IsLdcI4() &&
                            IL[x + 1].Operand is IMethod &&
                            IL[x + 1].Operand == DecMeth)
                            {
                                var Result = Ctx.SysModule.ResolveMethod(DecMeth.MDToken.ToInt32()).Invoke(null, new object[] { IL[x].GetLdcI4Value() });
                                IL[x].OpCode = OpCodes.Ldstr;
                                IL[x].Operand = Result;
                                IL.RemoveAt(x + 1);
                                Ctx.Log.Info($"Restored Str : {Result}");
                                if (Result.ToString().Contains("This assembly is protected by an unregistered version of Eziriz's")) { Executer.CallsToRemove.Add(MethodDef); Executer.ILToRemove.Add((List<Instruction>)MethodDef.Body.Instructions); Ctx.Log.Info("Removed Watermark Successfly x)"); }
                            }
                        }
                        catch (Exception)
                        {
                            // just ignore kek
                        }
                    }
                }
            }
        }
        private static IMethod GetDecryptionMethod(ModuleDef Module)
        {
            foreach (var Type in Module.Types)
                foreach (var Method in Type.Methods)
                    if (Method.ParamDefs.Count == 1 && 
                        Method.CustomAttributes.Count == 1 &&
                        Method.CustomAttributes[0].Constructor.FullName.Contains("::.ctor(System.Object)") &&
                        Method.ReturnType == Module.CorLibTypes.String) // accurate as fuck xD
                        return Method;
            return null;
        }
    }
}