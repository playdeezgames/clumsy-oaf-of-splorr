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
    Function HasBatter(item As IItem) As Boolean
        Return item.HasDimension(Dimensions.BATTER) AndAlso Not item.IsDimensionMinimum(Dimensions.BATTER)
    End Function
#End Region
#Region "Describe"
    Private Delegate Sub ItemDescriber(item As IItem)
    ReadOnly describeTable As New Dictionary(Of String, ItemDescriber) From
        {
        }
    Private Sub DescribeItem(item As IItem)
        item.AddMessage($"It is a {item.Name}.")
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
