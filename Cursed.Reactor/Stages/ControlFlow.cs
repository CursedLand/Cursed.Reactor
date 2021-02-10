using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;

namespace Cursed.Reactor.Stages
{
    internal class ControlFlow
    {
        public static void Execute(Context Ctx)
        {
            var CfDeob = new BlocksCflowDeobfuscator();
            ExecuteArithmetic(Ctx);
            foreach (var TypeDef in Ctx.DnModule.Types.ToArray())
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody && ContainsSwitch(x)).ToArray())
                {
                    try
                    {
                        Blocks blocks = new Blocks(MethodDef);
                        List<Block> test = blocks.MethodBlocks.GetAllBlocks();
                        blocks.RemoveDeadBlocks();
                        blocks.RepartitionBlocks();
                        blocks.UpdateBlocks();
                        blocks.Method.Body.SimplifyBranches();
                        blocks.Method.Body.OptimizeBranches();
                        CfDeob.Initialize(blocks);
                        CfDeob.Deobfuscate();
                        blocks.RepartitionBlocks();
                        IList<Instruction> instructions;
                        IList<ExceptionHandler> exceptionHandlers;
                        blocks.GetCode(out instructions, out exceptionHandlers);
                        DotNetUtils.RestoreBody(MethodDef, instructions, exceptionHandlers);
                    }
                    catch
                    {

                    }
                }
            }
        }
        private static bool ContainsSwitch(MethodDef method)
        {
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                if (method.Body.Instructions[i].OpCode == OpCodes.Switch)
                {
                    return true;
                }
            }
            return false;
        }
        public static void ExecuteArithmetic(Context Ctx)
        {
            foreach (var type in Ctx.DnModule.Types)
            {
                foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
                {

                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        if (method.Body.Instructions[i].OpCode == OpCodes.Brtrue && method.Body.Instructions[i + 1].OpCode == OpCodes.Pop && method.Body.Instructions[i - 1].OpCode == OpCodes.Call)
                        {
                            if (method.Body.Instructions[i - 1].Operand.ToString().Contains("System.Boolean"))
                            {
                                method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                method.Body.Instructions[i].OpCode = OpCodes.Br_S;
                            }
                            else
                            {
                                method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                method.Body.Instructions[i].OpCode = OpCodes.Nop;
                            }
                        }
                        else
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Brfalse && method.Body.Instructions[i + 1].OpCode == OpCodes.Pop && method.Body.Instructions[i - 1].OpCode == OpCodes.Call)
                            {
                                if (method.Body.Instructions[i - 1].Operand.ToString().Contains("System.Boolean"))
                                {
                                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                }
                                else
                                {
                                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i].OpCode = OpCodes.Br_S;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}