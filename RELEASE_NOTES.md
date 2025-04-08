### 2.0.0 (TBD)

From 2.0.0 onwards, this package will have a decoupled release schedule from the core `BioFSharp` package.
This means that the versioning will be independent and may not follow the same versioning scheme as `BioFSharp`.

The last coupled release was `2.0.0-preview.3`.

- `PhylTree` creation based on Hierarchical Clustering was ported from `BioFSharp` to `BioFSharp.Stats` (this package).
- Updated to FSharp.Stats 0.6.0 and adopted `PhylTree` creation accordingly.
- `Sailent` module was renamed to `SAEPT`
- `RNASeq` type create functions changed to tupled arguments