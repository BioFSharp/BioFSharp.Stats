namespace BioFSharp.Stats.Tests

open Xunit
open BioFSharp
open BioFSharp.Stats

module RNASeqTests =

    let testSeq = seq { for i in 1. .. 2. -> ("stringtest"+ i.ToString(),(i,i))}
    let testgeneID = seq { "stringtest1";  "stringtest2"}
    let testLength = seq {1.; 2.}
    let testCount = seq {1.;2.}
    let testInSeq = Seq.map3 (fun id gl gc ->  RNASeq.RNASeqInput.Create(id, gl, gc)) testgeneID testLength testCount

    let resultRPKM= seq {("stringtest1", 333333333.3333333); ("stringtest2",333333333.3333333)}
    let resultTPM= seq {("stringtest1", 500000.); ("stringtest2", 500000.)}
    let RPKMres = Seq.map (fun (id,rpkm) ->  RNASeq.NormalizedCounts.Create(id, rpkm, RNASeq.NormalizationMethod.RPKM)) resultRPKM
    let TPMres = Seq.map (fun (id,tpm) ->  RNASeq.NormalizedCounts.Create(id, tpm, RNASeq.NormalizationMethod.TPM)) resultTPM
    
    [<Fact>]
    let ``RPKM`` () =
        let actual = RNASeq.rpkms testInSeq
        let expected = RPKMres
        Assert.Equal<seq<RNASeq.NormalizedCounts>>(expected, actual)

    [<Fact>]
    let ``TPM`` () =
        let actual = RNASeq.tpms testInSeq
        let expected = TPMres
        Assert.Equal<seq<RNASeq.NormalizedCounts>>(expected, actual)
