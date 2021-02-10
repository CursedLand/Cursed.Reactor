using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;

namespace Cursed.Reactor.Stages
{
    internal class JunkRemover
    {
        public static void Execute(Context Ctx)
        {
            foreach (var Type in Ctx.DnModule.Types.ToArray())
            {
                foreach (var Method in Type.Methods.Where(x => x.HasBody && x.IsConstructor ).ToArray())
                {
                    // TODO : add more check for making it more reliable :D
                    Method.Body.Instructions.Clear();
                }
            }
        }
    }
}