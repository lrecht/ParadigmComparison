// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Text
open System.Collections.Generic
open benchmark

[<AbstractClass>]
type IHuffmanTree(id, freq) =
    member val Id: int32 = id with get, set
    member val Frequency: int32 = freq with get
    interface IComparable<IHuffmanTree> with
        override this.CompareTo obj = 
            let res = this.Frequency - obj.Frequency
            if res = 0 then this.Id.CompareTo obj.Id else res


type HuffmanLeaf(c, freq, id) =
    inherit IHuffmanTree(id, freq)
    member val Character = c with get


type HuffmanNode(left: IHuffmanTree, right: IHuffmanTree, id) =
    inherit IHuffmanTree(id, left.Frequency + right.Frequency)
    member val Left = left with get
    member val Right = right with get


type Huffman(stringToEncode) as this =
    let mutable symbolTable = new Dictionary<char, string>()
    do
        let mutable frequencies = new Dictionary<char, int>()
        for ch in stringToEncode do
            if frequencies.ContainsKey(ch)
            then frequencies.[ch] <- frequencies.[ch] + 1
            else frequencies.Add(ch, 1)

        let tree = this.BuildTree frequencies
        this.UpdateSymbolTable tree (StringBuilder())

    member __.BuildTree frequencies =
        let trees = new SortedSet<IHuffmanTree>()
        let mutable id = 0
        for symbol: KeyValuePair<char, int> in frequencies do
            trees.Add(HuffmanLeaf(symbol.Key, symbol.Value, id)) |> ignore
            id <- id + 1

        while trees.Count > 1 do
            let leftChild = trees.Min
            trees.Remove(trees.Min) |> ignore
            let rightChild = trees.Min
            trees.Remove(trees.Min) |> ignore
            trees.Add(HuffmanNode(leftChild, rightChild, id)) |> ignore
            id <- id + 1
        trees.Min
    
    member this.UpdateSymbolTable tree prefix =
        if tree :? HuffmanLeaf
        then 
            let leaf = tree :?> HuffmanLeaf
            symbolTable.[leaf.Character] <- prefix.ToString()
        else
            let node = tree :?> HuffmanNode
            prefix.Append("0") |> ignore
            this.UpdateSymbolTable node.Left prefix
            prefix.Remove(prefix.Length - 1, 1) |> ignore

            prefix.Append("1") |> ignore
            this.UpdateSymbolTable node.Right prefix
            prefix.Remove(prefix.Length - 1, 1) |> ignore

    member __.Encode stringToEncode =
        let encodedString = StringBuilder()
        for ch in stringToEncode do
            encodedString.Append(symbolTable.[ch]) |> ignore
        encodedString.ToString()

[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    let testString = File.ReadAllText("benchmarks/huffman_coding/lines.txt")
    
    bm.Run((fun () ->
        let huffman = Huffman(testString)
        let encodedString = huffman.Encode testString
        encodedString.Length
    ), (fun (res) ->
        printfn "The length is: %d" res
    ))
    
    0 // return an integer exit code
