// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace McMaster.DotNet.Serve.RazorPages
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
