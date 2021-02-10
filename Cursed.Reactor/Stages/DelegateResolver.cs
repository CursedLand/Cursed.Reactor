using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;

namespace Cursed.Reactor.Stages
{
    internal class DelegateResolver
    {
        public static void Execute(Context Ctx)
        {
            foreach (var TypeDef in Ctx.DnModule.Types.ToArray())
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody).ToArray())
                {
                    IList<Instruction> IL = MethodDef.Body.Instructions;
                    for (int x = 0; x < IL.Count; x++)
                    {
                        try
                        {
                            if (IL[x].OpCode == OpCodes.Ldsfld &&
                                IL[x].Operand is IField &&
                                ((FieldDef)IL[x].Operand).DeclaringType.Namespace == Ctx.DnModule.GlobalType.Namespace &&
                                IL[x + 1].OpCode == OpCodes.Call)
                            {

                                var DnField = (IField)IL[x].Operand;
                                var DicField = GetDictionary((TypeDef)DnField.DeclaringType, Ctx);
                                var SysField = (Dictionary<int, int>)Ctx.SysModule.ResolveField(DicField.MDToken.ToInt32()).GetValue(null);
                                if (!(SysField != null)) continue;
                                var MethodToken = SysField[DnField.MDToken.ToInt32()];
                                bool flag2 = (MethodToken & 1073741824) > 0;
                                MethodToken &= 1073741823;

                                // Restoring Process

                                var Handler = Type.GetTypeFromHandle(Ctx.SysModule.ResolveType(DnField.DeclaringType.MDToken.ToInt32()).TypeHandle);
                                var Method = Ctx.SysModule.ResolveMethod(MethodToken, Handler.GetGenericArguments(), new Type[0]);
                                if (flag2)
                                    IL[x + 1].OpCode = OpCodes.Callvirt;
                                IL[x + 1].Operand = Ctx.DnModule.Import(Method);
                                Ctx.Log.Info($"Restored Method : {Method.Name}");
                                IL.RemoveAt(x);
                            }
                        }
                        catch (Exception /*e*/)
                        {
                            /* uhm x(
                            Ctx.Log.Error(e.Message);*/
                        }
                    }
                }
            }
        }

        private static IField GetDictionary(TypeDef Constructor, Context Ctx)
        {
            TypeDef TypeHaveDic = null;
            var Meth = LetMeGetMyDelegateShit(Constructor.FindStaticConstructor().Body);
            if (Meth != null)
            {
                TypeHaveDic = (TypeDef)Meth.DeclaringType;
                Ctx.SysModule.ResolveMethod( // Just Fake Shit ( for initiating the dic ) nvm
                    (Meth).MDToken.ToInt32()).Invoke(null, new[] { Convert.ChangeType(typeof(Console).TypeHandle, typeof(RuntimeTypeHandle)) });
                Executer.ILToRemove.Add((List<Instruction>)Constructor.FindStaticConstructor().Body.Instructions);
            }
            else
                return null;
            foreach (var Field in TypeHaveDic.Fields.Where(x => x.IsStatic))
                if (Field.FieldSig.Type.FullName.Contains("System.Collections.Generic.Dictionary`2<System.Int32,System.Int32>"))
                    return Field;
            return null;
        }

        private static IMethod LetMeGetMyDelegateShit(CilBody ConsBody)
        {
            for (var x = 0; x < ConsBody.Instructions.Count; x++)
            {
                if (ConsBody.Instructions[x].OpCode == OpCodes.Ldtoken &&
                    ConsBody.Instructions[x + 1].OpCode == OpCodes.Call &&
                    ConsBody.Instructions[x + 1].Operand is IMethod)
                {
                    return (IMethod)ConsBody.Instructions[x + 1].Operand;
                }
            }
            return null;
        }
    }
}