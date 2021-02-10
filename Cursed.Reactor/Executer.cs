using UnSealer.Core;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Cursed.Reactor
{
    public class Executer : Protection
    {
        public override string Name => ".Net Reactor Unpacker";

        public override string Author => "CursedLand";

        public override string Description => ".Net Reactor Full Unpacker";

        public override ProtectionType Type => ProtectionType.Dnlib;

        public override void Execute(Context Context)
        {
            if (Context.DnModule != null && Context.SysModule != null) {
                Stages.DelegateResolver.Execute(Context);
                Stages.AntiShitRemover.Execute(Context);
                Stages.StringRestoration.Execute(Context);
                Stages.ControlFlow.Execute(Context);
                // Stages.JunkRemover.Execute(Context); <|> Enable It If You Want Only <|>
                ILToRemove.ForEach(x => x.Clear());
            }
            else { Context.Log.Custom("Reflection Or Dnlib Module Not Founded !", "Skipping"); }
        }
        public static List<MethodDef> CallsToRemove = new List<MethodDef>();

        public static List<List<Instruction>> ILToRemove = new List<List<Instruction>>();
    }
}