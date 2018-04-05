using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace McMaster.DotNet.Server.RazorPages
{
    class TpaReferencesProvider : ApplicationPart,
        ICompilationReferencesProvider
    {
        public override string Name => nameof(TpaReferencesProvider);

        public IEnumerable<string> GetReferencePaths()
        {
            if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string tpaList)
            {
                foreach (var assembly in tpaList.Split(Path.PathSeparator))
                {
                    yield return assembly;
                }
            }
        }
    }
}
