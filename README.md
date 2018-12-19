# Duey
A minimal read-only implementation of the [NX PKG4.1 format](http://nxformat.github.io/) specification on .NET Standard 2.0

## 🤔 Why?
* Duey works on runtimes targetting or supporting .NET Standard!
* also, it's strictly parsing only. no caches, no weird voodoo magic.

## ✏️ Usage
to get started, simply create a new NX File object like so.
```csharp
var file = new NXFile("Data.nx");
```
with that, you can do various parsing magic!
```csharp
// store a node object for usage later on!
var node = file.Resolve("Products");

// resolve a node ..in a node!
node.Resolve("Bundled Products");

// resolve a nullable
node.ResolveOrDefault<string>("name");

// resolve a non-nullable
node.Resolve<int>("stock");
node.Resolve<double>("price");
```
also, remember to dispose~!
```csharp
using (var file = new NXFile("Data.nx")) {
    // do your parsing thing here!
}

// or manually call dipose
var file = new NXFile("Data.nx");
file.Dispose();
```

## ⭐️ Acknowledgements
* [reNX](https://github.com/angelsl/ms-reNX) - main reference for implementations.
* [PKG1](https://labs.crr.io/maplestory/PKG1) - for the inspiration to create this project.
* all the kind souls who designed the NX format.

## 🚨 Disclaimer
* this project is purely educational.
* this project has not profitted in any way shape or form.
