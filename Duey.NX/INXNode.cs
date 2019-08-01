using System;
using System.Collections.Generic;
using Duey.NX.Layout;

namespace Duey.NX
{
    public interface INXNode : IEnumerable<INXNode>
    {
        NXNodeType Type { get; }
        
        string Name { get; }
        INXNode Parent { get; }
        IEnumerable<INXNode> Children { get; }

        INXNode ResolveAll();
        void ResolveAll(Action<INXNode> context);
        object Resolve();
        INXNode ResolvePath(string path = null);
        T? Resolve<T>(string path = null) where T : struct;
        T ResolveOrDefault<T>(string path = null) where T : class;
    }
}