using dnlib.DotNet.Emit;
using System.Linq;
using UnSealer.Core;

namespace Cursed.Reactor.Stages
{
    internal class AntiShitRemover
    {
        public static void Execute(Context Ctx)
        {
            foreach (var TypeDef in Ctx.DnModule.Types.ToArray())
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody && x.IsStatic).ToArray())
                {
                    foreach (var i in MethodDef.Body.Instructions.Where(x => x.OpCode == OpCodes.Ldstr).ToArray())
                    {
                        if (i.Operand.ToString().Contains("is tampered"))
                        {
                            Executer.CallsToRemove.Add(MethodDef);
                            MethodDef.Body.Instructions.Clear();
                            Ctx.Log.Info("Removed Anti Tamper.");
                        }
                        if(i.Operand.ToString().Contains("Debugger Detected"))
                        {
                            Executer.CallsToRemove.Add(MethodDef);
                            MethodDef.Body.Instructions.Clear();
                            Ctx.Log.Info("Removed Debugger.");
                        }
                    }
                }
            }
        }
    }
}