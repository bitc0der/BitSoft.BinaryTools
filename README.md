# BitSoft.BinaryTools

Yet another one tools lib for operations with binary data.

## Binary patch

Calculate diff and create patch for two streams:

```csharp
public async ValueTask CreatePatch(Stream source, Stream target, Stream output, CancellationToken token)
{
    await BinaryPatch.CreateAsync(source, target, output, cancellationToken: token);
}
```

Apply patch to a source stream:

```csharp
public async ValueTask ApplyAsync(Stream source, Stream patch, Stream output, CancellationToken token)
{
    await BinaryPatch.ApplyAsync(source, patch, output, cancellationToken: token);
}
```
