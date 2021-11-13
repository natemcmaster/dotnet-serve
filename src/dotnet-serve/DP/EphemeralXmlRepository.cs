// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace McMaster.DotNet.Serve;

internal class EphemeralXmlRepository : IXmlRepository
{
    private readonly List<XElement> _storedElements = new();

    public virtual IReadOnlyCollection<XElement> GetAllElements()
    {
        lock (_storedElements)
        {
            return GetAllElementsCore().ToList().AsReadOnly();
        }
    }

    private IEnumerable<XElement> GetAllElementsCore()
    {
        foreach (var element in _storedElements)
        {
            yield return new XElement(element);
        }
    }

    public virtual void StoreElement(XElement element, string friendlyName)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        var cloned = new XElement(element);

        lock (_storedElements)
        {
            _storedElements.Add(cloned);
        }
    }
}
