Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module ItemExtensions
#Region "Mixing Bowl"
    ReadOnly mixingbowlCounterTable As New Dictionary(Of String, (Name As String, Quantity As Integer)) From
        {
            {Counters.FLOUR, ("Flour", 3)},
            {Counters.SUGAR, ("Sugar", 2)},
            {Counters.VANILLA, ("Vanilla", 1)},
            {Counters.BAKING_POWDER, ("Baking Powder", 1)},
            {Counters.BAKING_SODA, ("Baking Soda", 0)},
            {Counters.SALT, ("Salt", 1)},
            {Counters.EGG, ("Eggs", 2)},
            {Counters.BUTTER, ("Butter", 2)},
            {Counters.MILK, ("Milk", 1)}
        }
    <Extension>
    Sub Mix(item As IItem)
        If item.EntitySubtype <> ItemSubtypes.MIXING_BOWL Then
            Return
        End If
        Dim tally As Integer = 0
        Dim isBatter As Boolean = True
        For Each entry In mixingbowlCounterTable
            Dim counter = item.GetCounter(entry.Key)
            tally += counter
            isBatter = isBatter AndAlso counter = entry.Value.Quantity
            item.MinimizeCounter(entry.Key)
        Next
        item.ChangeDimension(If(isBatter, Dimensions.BATTER, Dimensions.GLOP), tally)
    End Sub
    <Extension>
    Function HasBatter(item As IItem) As Boolean
        Return item.HasDimension(Dimensions.BATTER) AndAlso Not item.IsDimensionMinimum(Dimensions.BATTER)
    End Function
    <Extension>
    Function IsEmpty(item As IItem) As Boolean
        If item.EntitySubtype <> ItemSubtypes.MIXING_BOWL Then
            Return False
        End If
        Return mixingbowlCounterTable.Keys.All(AddressOf item.IsCounterMinimum)
    End Function
    <Extension>
    Sub Empty(item As IItem)
        If item.EntitySubtype <> ItemSubtypes.MIXING_BOWL Then
            Return
        End If
        For Each counterId In mixingbowlCounterTable.Keys
            item.MinimizeCounter(counterId)
        Next
        item.MinimizeDimension(Dimensions.BATTER)
        item.MinimizeDimension(Dimensions.GLOP)
    End Sub
#End Region
#Region "Describe"
    Private Delegate Sub ItemDescriber(item As IItem)
    ReadOnly describeTable As New Dictionary(Of String, ItemDescriber) From
        {
            {ItemSubtypes.MIXING_BOWL, AddressOf DescribeMixingBowl},
            {ItemSubtypes.CAKE_BOARD, AddressOf DescribeCakeBoard},
            {ItemSubtypes.CAKE_PAN, AddressOf DescribeCakePan},
            {ItemSubtypes.RECIPE_CARD, AddressOf DescribeRecipeCard}
        }

    Private Sub DescribeRecipeCard(item As IItem)
        DescribeItem(item)
        item.AddMessage($"Recipe for Batter:")
        For Each entry In mixingbowlCounterTable.Values
            item.AddMessage($"{entry.Name}: {entry.Quantity}")
        Next
    End Sub

    Private Sub DescribeCakeBoard(item As IItem)
        DescribeItem(item)
        Dim layers = item.GetCounter(Counters.LAYERS)
        If layers > 0 Then
            item.AddMessage($"{item.Name} has a {layers} layer cake on it.")
        Else
            item.AddMessage($"{item.Name} is completely cakeless!")
        End If
    End Sub

    Private Sub DescribeCakePan(item As IItem)
        DescribeItem(item)
        DescribeItemBatter(item)
        If item.HasTag(Tags.CAKE) Then
            item.AddMessage($"{item.Name} contains a cake.")
        End If
    End Sub

    Private Sub DescribeItem(item As IItem)
        item.AddMessage($"It is a {item.Name}.")
    End Sub
    Private Sub DescribeMixingBowl(item As IItem)
        DescribeItem(item)
        For Each entry In mixingbowlCounterTable
            Dim amount = item.GetCounter(entry.Key)
            If amount > 0 Then
                item.AddMessage($"{entry.Value.Name}: {amount}")
            End If
        Next
        DescribeItemBatter(item)
        DescribeItemGlop(item)
    End Sub

    Private Sub DescribeItemBatter(item As IItem)
        If Not item.IsDimensionMinimum(Dimensions.BATTER) Then
            Dim batter = item.GetDimension(Dimensions.BATTER)
            item.AddMessage($"Batter: {batter:f2}")
        End If
    End Sub

    Private Sub DescribeItemGlop(item As IItem)
        If Not item.IsDimensionMinimum(Dimensions.GLOP) Then
            Dim glop = item.GetDimension(Dimensions.GLOP)
            item.AddMessage($"Glop: {glop:f2}")
        End If
    End Sub

    <Extension>
    Sub Describe(item As IItem)
        Dim describer As ItemDescriber = Nothing
        If describeTable.TryGetValue(item.EntitySubtype, describer) Then
            describer.Invoke(item)
        Else
            DescribeItem(item)
        End If
    End Sub
#End Region
End Module
