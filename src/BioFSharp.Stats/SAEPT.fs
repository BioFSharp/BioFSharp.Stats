﻿namespace BioFSharp.Stats

open FSharp.Stats
open OntologyEnrichment

/// Surprisal Analysis Empirical Permutation Test (SAEPT) 
module SAEPT =
    
    type SAEPTCharacterization = {
        OntologyTerm : string
        PValue : float
        BinSize: int
        WeightSum: float
    }

    let private createSAEPTCharacterization ontTerm pVal binSize wS = {OntologyTerm = ontTerm; PValue = pVal; BinSize = binSize; WeightSum=wS}
        

    type SAEPTResult<'a> = {
        RawData:                OntologyItem<'a> array
        AbsoluteDescriptor:     SAEPTCharacterization list
        PositiveDescriptor:     SAEPTCharacterization list
        NegativeDescriptor:     SAEPTCharacterization list
        BootstrapIterations:    int
    }

    let private createSAEPTResult raw abs pos neg iter = {RawData=raw; AbsoluteDescriptor=abs; PositiveDescriptor=pos; NegativeDescriptor=neg; BootstrapIterations=iter}

    //create distribution of iter weight sums for a bin of size binSize 
    let private bootstrapBin (binSize: int) (weightArray:float[]) (iter: int) =
        let steps = iter / 10
        let startTime = System.DateTime.Now

        let rec sumRandomEntriesBy k sum =
            if k < binSize then
                sumRandomEntriesBy (k+1) (sum + (weightArray.[Random.rndgen.NextInt(weightArray.Length)]))
            else 
                sum

        let rec loop currentIter resultList =
            if currentIter < iter then
                let tmp = sumRandomEntriesBy 0 0.
                loop (currentIter+1) (tmp::resultList)
            else
                resultList |> Array.ofList

        loop 1 []

    let private getEmpiricalPvalue (testDistributions: Map<int,Map<float,int>>) (weightSum:float) (binSize:int) =
        match Map.tryFind binSize testDistributions with
        |Some dist ->   let testDist = dist
                        float (testDist |> Map.fold (fun acc key value -> if abs key > abs weightSum then acc + value else acc) 0) / (float (testDist |> Map.fold (fun acc key value -> acc + value) 0))
        |_ -> 10000000.

    let private assignPValues (testDistributions:Map<int,Map<float,int>>) (testTargets:(string*int*float)[])=
        testTargets 
        |> Array.map (fun (name,binSize,weightSum) -> createSAEPTCharacterization name (getEmpiricalPvalue testDistributions weightSum binSize) binSize weightSum)
    
    ///utility function to prepare a dataset column for SAEPT characterization. The ontology map can be created by using the BioFSharp.BioDB module. 
    ///
    ///identifiers: a string array containing the annotations of the data at the same index, used as lookup in the ontology map. 
    ///rawData: feature array of interest, must be same length as annotations.
    let prepareDataColumn (ontologyMap:Map<string,(string*string) [] >) (identifiers: string []) (rawData:float []) =

        if rawData.Length <> identifiers.Length then
            failwithf "data column and identifiers dont have the same length (%i vs %i)" rawData.Length identifiers.Length
        else
            let annotatedIds =
                identifiers
                |> Array.map (fun id -> match Map.tryFind id ontologyMap with
                                        |Some ann -> ann |> Array.map snd |> Array.ofSeq
                                        |_ -> [|"35.2"|]
                                        )
            rawData
            |> Array.mapi (fun i x ->  annotatedIds.[i] 
                                       |> Array.map (fun ann -> identifiers.[i],ann,0,x))
            |> Array.concat
                                                          
            |> Array.map (fun (identifier,annotation,indx,value) -> createOntologyItem identifier annotation indx value)

    ///utility function to prepare a dataset (in column major form) for SAEPT characterization. The ontology map can be created by using the BioFSharp.BioDB module.
    ///identifiers: a string array containing the annotations of the data at the same index, used as lookup in the ontology map. 
    ///rawData: feature matrix of interest, columns must have same length as identifiers
    let prepareDataset (ontologyMap:Map<string,(string*string) [] >) (identifiers: string []) (rawDataset:float [] []) =
        rawDataset
        |> Array.map (prepareDataColumn ontologyMap identifiers)

    ///Compute SAEPT (Surprisal AnalysIs EmpiricaL pErmutatioN Test) for the given annotated dataset. This empirical test was
    ///initially designed for the biological application of Surprisal Analysis to test the weight distribution of a given bin of annotations is significantly different than a random distribution 
    ///of the same size given the whole dataset, but it should be applicable to similar types of datasets.
    ///
    ///Input: 
    ///
    ///- verbose: if true, bootstrap iterations and runtime for bootstrapping is printed
    ///
    ///- bootstrapIterations: the amount of distributions to sample from the whole dataset to create test distributions for each binsize present in the data
    ///
    ///- data: annotated dataset (containing ontology items with the associated feature)
    ///
    ///a SAEPT test returns 3 descriptors for the input data:
    ///Absolute descriptor: test distributions and tests are performed on the absolute values of the dataset
    ///Negative descriptor: test distributions and tests are performed on the negative values of the dataset only
    ///Absolute descriptor: test distributions and tests are performed on the positive values of the dataset only
    let compute (verbose:bool) (bootstrapIterations:int) (data: OntologyItem<float> array) =

        if verbose then printfn "starting SAEPT characterization"

        let groups = data |> Array.groupBy (fun x -> x.OntologyTerm)
        
        let absoluteTestTargets = 
            groups
            |> Array.map (fun (termName,tmp) ->   
                termName,tmp.Length,tmp |> Array.sumBy (fun x -> abs x.Item))
            |> Array.filter (fun (termName,binSize,weightSum) -> binSize>0)
        
        let positiveTestTargets = 
            groups
            |> Array.map (fun (termName,tmp) ->   
                let tmp = tmp |> Array.filter (fun ann -> ann.Item > 0.)
                termName,tmp.Length,tmp |> Array.sumBy (fun x -> x.Item))
            |> Array.filter (fun (termName,binSize,weightSum) -> binSize>0)
        
        let negativeTestTargets = 
            groups
            |> Array.map (fun (termName,tmp) ->   
                let tmp = tmp |> Array.filter (fun ann -> ann.Item < 0.)
                termName,tmp.Length,tmp |> Array.sumBy (fun x -> x.Item))
            |> Array.filter (fun (termName,binSize,weightSum) -> binSize>0)

        let absoluteBinsizes = absoluteTestTargets |> Array.map (fun (_,binSize,_) -> binSize) |> Array.distinct
        let positiveBinsizes = positiveTestTargets |> Array.map (fun (_,binSize,_) -> binSize) |> Array.distinct
        let negativeBinsizes = negativeTestTargets |> Array.map (fun (_,binSize,_) -> binSize) |> Array.distinct

        let weightArr =     data        |> Array.map (fun ann -> ann.Item)
        let absWeightArr =  weightArr   |> Array.map abs
        let posWeightArr =  weightArr   |> Array.filter(fun x -> x>0.)
        let negWeightArr =  weightArr   |> Array.filter(fun x -> x<0.)


        // create bootstrapped test distributions for all test targets
        if verbose then printfn "bootstrapping absolute test distributions for %i bins" absoluteBinsizes.Length
        let absoluteTestDistributions =

            let startTime = System.DateTime.Now

            absoluteBinsizes
            |> Array.mapi 
                (fun i binSize ->

                    if verbose && (i % (absoluteBinsizes.Length / 10) = 0 ) then
                        let elapsed = System.DateTime.Now.Subtract(startTime)
                        printfn "[%i/%i] bins @ %imin %is" i absoluteBinsizes.Length elapsed.Minutes elapsed.Seconds

                    let tmp = bootstrapBin binSize absWeightArr bootstrapIterations
                    (binSize,Distributions.Frequency.create (Distributions.Bandwidth.nrd0 tmp) tmp)
                )
            |> Map.ofArray

        if verbose then printfn "bootstrapping positive test distributions for %i bins" positiveBinsizes.Length
        let positiveTestDistributions =

            let startTime = System.DateTime.Now

            positiveBinsizes
            |> Array.mapi 
                (fun i binSize ->

                    if verbose && (i % (positiveBinsizes.Length / 10) = 0 ) then
                        let elapsed = System.DateTime.Now.Subtract(startTime)
                        printfn "[%i/%i] bins @ %imin %is" i positiveBinsizes.Length elapsed.Minutes elapsed.Seconds

                    let tmp = bootstrapBin binSize posWeightArr bootstrapIterations
                    (binSize,Distributions.Frequency.create (Distributions.Bandwidth.nrd0 tmp) tmp)
                )
            |> Map.ofArray
            
        if verbose then printfn "bootstrapping negative test distributions for %i bins" negativeBinsizes.Length
        let negativeTestDistributions = 

            let startTime = System.DateTime.Now

            negativeBinsizes
            |> Array.mapi 
                (fun i binSize ->

                    if verbose && (i % (negativeBinsizes.Length / 10) = 0 ) then
                        let elapsed = System.DateTime.Now.Subtract(startTime)
                        printfn "[%i/%i] bins @ %imin %is" i negativeBinsizes.Length elapsed.Minutes elapsed.Seconds

                    let tmp = bootstrapBin binSize negWeightArr bootstrapIterations
                    (binSize,Distributions.Frequency.create (Distributions.Bandwidth.nrd0 tmp) tmp)
                )
            |> Map.ofArray

        if verbose then printfn "assigning empirical pValues for all bins..."

        //assign Pvalues for all test targets
        let absResults = assignPValues absoluteTestDistributions absoluteTestTargets |> Array.toList
        let posResults = assignPValues positiveTestDistributions positiveTestTargets |> Array.toList
        let negResults = assignPValues negativeTestDistributions negativeTestTargets |> Array.toList

        createSAEPTResult data absResults posResults negResults bootstrapIterations


    ///Compute SAEPT (Surprisal AnalysIs EmpiricaL pErmutatioN Test) for the given Surprisal Analysis result. This empirical test was
    ///designed for the biological application of Surprisal Analysis to test the weight distribution of a given bin of annotations is significantly different than a random distribution 
    ///of the same size given the whole dataset.
    ///
    ///Input: 
    ///
    ///- verbose: if true, bootstrap iterations and runtime for bootstrapping is printed
    ///
    ///- ontologyMap: maps identifiers of the data to ontology annotations (can be created using the BioFSharp.BioDB module)
    ///
    ///- identifiers: a string array containing the annotations of the data at the same index, used as lookup in the ontology map. 
    ///
    ///- bootstrapIterations: the amount of distributions to sample from the whole dataset to create test distributions for each binsize present in the data
    ///
    ///- saRes: the Surprisal Analysis Result to test
    ///
    ///a SAEPT test returns 3 descriptors for each constraint of the Surprisal anlysis result:
    ///Absolute descriptor: test distributions and tests are performed on the absolute values of the dataset
    ///Negative descriptor: test distributions and tests are performed on the negative values of the dataset only
    ///Absolute descriptor: test distributions and tests are performed on the positive values of the dataset only

    let computeOfSARes (verbose:bool) (ontologyMap:Map<string,(string*string) [] >) (identifiers: string []) (bootstrapIterations:int) (saRes:FSharp.Stats.ML.SurprisalAnalysis.SAResult) =
        saRes.MolecularPhenotypes
        |> Matrix.toJaggedArray
        // Matrices are sadly row major =(
        |> JaggedArray.transpose
        |> prepareDataset ontologyMap identifiers
        |> Array.mapi 
            (fun i p ->
                if verbose then printfn "SAEPT of constraint %i" i
                compute verbose bootstrapIterations p
            ) 
    
    ///Async version of computeOfSARes to use for parallelization (computeOfSAResAsync ( .. ) |> Async.Parallel |> Async.RunSynchronously)
    let computeOfSAResAsync (verbose:bool) (ontologyMap:Map<string,(string*string) [] >) (identifiers: string []) (bootstrapIterations:int) (saRes:FSharp.Stats.ML.SurprisalAnalysis.SAResult) =
        saRes.MolecularPhenotypes
        |> Matrix.toJaggedArray
        // Matrices are sadly row major =(
        |> JaggedArray.transpose
        |> prepareDataset ontologyMap identifiers
        |> Array.mapi 
            (fun i p ->
                async {
                    return compute verbose bootstrapIterations p
                }
            ) 
    
