# BitSoft.BinaryTools

Yet another one tools lib for operations with binary data.

## Binary patch

Calculate diff and create patch for two streams:

```csharp
using System.IO;
using System.Threading.Tasks;
using BitSoft.BinaryTools.Patch;

public async ValueTask CreatePatchAsync(Stream source, Stream target, Stream output, CancellationToken token)
{
    await BinaryPatch.CreateAsync(source, target, output, cancellationToken: token);
}
```

Apply patch to a source stream:

```csharp
using System.IO;
using System.Threading.Tasks;
using BitSoft.BinaryTools.Patch;

public async ValueTask ApplyAsync(Stream source, Stream patch, Stream output, CancellationToken token)
{
    await BinaryPatch.ApplyAsync(source, patch, output, cancellationToken: token);
}
```
