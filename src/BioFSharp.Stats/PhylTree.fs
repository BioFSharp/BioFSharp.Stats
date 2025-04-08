namespace BioFSharp.Stats

open BioFSharp
open FSharp.Stats.ML.Unsupervised
open Priority_Queue

module PhylTree =

    /// <summary>
    /// converts the input hierarchical clustering to a phylogenetig tree and conserves the distance insformation.
    /// In contrast to the clustering result, the distance value of a Branch represents the distance to its Parent, 
    /// not the distance that all children have to this Branch.
    /// </summary>
    let ofHierarchicalCluster (branchTag:'T) (distanceConverter: float -> 'Distance) (hCluster:HierarchicalClustering.Cluster<'T>) : PhylogeneticTree<'T * 'Distance>=
        let rec loop distance (c: HierarchicalClustering.Cluster<'T>) =
            match c with
            | HierarchicalClustering.Cluster.Node (cIndex, distance, lCount, left, right) ->    
                PhylogeneticTree.Branch ((branchTag, distanceConverter distance), [loop distance left; loop distance right])
            | HierarchicalClustering.Cluster.Leaf (id, lCount, tag) -> PhylogeneticTree.Leaf((tag, distanceConverter distance))
        loop 0. hCluster 

    /// <summary>
    /// Performs hierarchical clustering of the input TaggedSequences using the provided distance function and linker. Returns the result as a Phylogenetic tree.</summary>
    /// </summary>
    /// <parameter name="branchTag">a tag to give the infered common ancestor branches (these are not tagged in contrast to the input sequence.)</parameter>
    /// <parameter name="distanceConverter">a converter function for the distance between nodes of the tree. Usually, a conversion to a string makes sense for downstream conversion to Newick format</parameter>
    /// <parameter name="distanceFunction">a function that determines the distance between two sequences e.g. evolutionary distance based on a substitution model</parameter>
    /// <parameter name="linker">the linker function to join clusters with</parameter>
    /// <parameter name="sequences">the input TaggedSequences</parameter>
    let ofTaggedSequencesWithLinker (branchTag:'T) (distanceConverter: float -> 'Distance) (distanceFunction: seq<'S> -> seq<'S> -> float) linker (sequences: TaggedSequence<'T,'S> array) =
        let dist = (fun (a: TaggedSequence<'T,'S>) (b: TaggedSequence<'T,'S>)  -> distanceFunction a.Sequence b.Sequence)
        let clustering = 
            HierarchicalClustering.generate<TaggedSequence<'T,'S>>
                dist
                linker
                sequences
            |> Seq.item 0
            |> (fun x -> x.Key)
        clustering
        |> ofHierarchicalCluster 
            (TaggedSequence.create branchTag Seq.empty<'S>) 
            distanceConverter


    /// <summary>
    /// Performs hierarchical clustering of the input TaggedSequences using the provided distance function. Returns the result as a Phylogenetic tree.</summary>
    /// </summary>
    /// <parameter name="distanceFunction">a function that determines the distance between two sequences e.g. evolutionary distance based on a substitution model</parameter>
    /// <parameter name="sequences">the input TaggedSequences</parameter>
    let ofTaggedBioSequences (distanceFunction: seq<#IBioItem> -> seq<#IBioItem> -> float) (sequences: TaggedSequence<string,#IBioItem> array) : PhylogeneticTree<TaggedSequence<string,#IBioItem>*float> =
        sequences
        |> ofTaggedSequencesWithLinker
            "Ancestor"
            id
            distanceFunction
            HierarchicalClustering.Linker.upgmaLwLinker