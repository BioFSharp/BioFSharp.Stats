namespace BioFSharp.XYZ

open BioFSharp

module BioTalk =
    let helloBio name =
        let mass = name |> BioArray.ofAminoAcidString |> BioArray.toAverageMass
        $"Hello {name} the average mass of your name as AminoAcid code is %.2f{mass}"
