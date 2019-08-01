using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Duey.NX.Layout;

namespace Duey.NX
{
    public class NXResolutionNode : INXNode
    {
        public NXNodeType Type => _node.Type;
        public string Name => _node.Name;
        public INXNode Parent => _node.Parent;
        public IEnumerable<INXNode> Children => _children.Values;

        private readonly INXNode _node;
        private readonly Dictionary<string, INXNode> _children;

        public NXResolutionNode(INXNode node)
        {
            _node = node;
            _children = _node.ToDictionary(n => n.Name, n => n);
        }

        public INXNode ResolveAll()
            => _node.ResolveAll();

        public void ResolveAll(Action<INXNode> context)
            => _node.ResolveAll(context);

        public object Resolve()
            => _node.Resolve();

        private INXNode ResolveChild(string name)
            => _children.ContainsKey(name) ? _children[name] : null;

        public INXNode ResolvePath(string path = null)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var split = path.Split('/');
            return ResolveChild(split[0])?.ResolvePath(string.Join("/", split.Skip(1).ToArray()));
        }

        public T? Resolve<T>(string path = null) where T : struct
        {
            if (string.IsNullOrEmpty(path)) return _node.Resolve<T>();
            var split = path.Split('/');
            return ResolveChild(split[0])?.Resolve<T>(string.Join("/", split.Skip(1).ToArray()));
        }

        public T ResolveOrDefault<T>(string path = null) where T : class
        {
            if (string.IsNullOrEmpty(path)) return _node.ResolveOrDefault<T>();
            var split = path.Split('/');
            return ResolveChild(split[0])?.ResolveOrDefault<T>(string.Join("/", split.Skip(1).ToArray()));
        }

        public IEnumerator<INXNode> GetEnumerator()
            => _children.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}