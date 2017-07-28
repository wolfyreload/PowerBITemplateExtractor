section Section1;

shared Person = let
    Source = Table.FromRows(Json.Document(Binary.Decompress(Binary.FromText("i45W8srPyFPSAVJ5qcVKsTpAgcycHLBARl5xfh5YKCC1JLUIKOaeWAJSFAsA", BinaryEncoding.Base64), Compression.Deflate)), let _t = ((type text) meta [Serialized.Text = true]) in type table [Name = _t, Surname = _t]),
    #"Changed Type" = Table.TransformColumnTypes(Source,{{"Name", type text}, {"Surname", type text}})
in
    #"Changed Type";

shared MeasuresTest = let
    Source = "",
    #"Removed Columns" = Table.RemoveColumns(Source,{"Measures"})
in
    #"Removed Columns";